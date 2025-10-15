using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rise.Services.Moderation;

/// <summary>
/// Wraps the NudeNet classifier to evaluate whether an uploaded profile picture is acceptable.
/// </summary>
public sealed class NudeNetImageModerationService : IImageModerationService, IDisposable
{
    private readonly ILogger<NudeNetImageModerationService> logger;
    private readonly object classifierInstance;
    private readonly MethodInfo classificationMethod;
    private readonly IDisposable? classifierDisposable;
    private readonly IAsyncDisposable? classifierAsyncDisposable;

    public NudeNetImageModerationService(ILogger<NudeNetImageModerationService> logger)
    {
        this.logger = logger;

        if (!TryCreateClassifier(out classifierInstance, out classificationMethod, out classifierDisposable, out classifierAsyncDisposable))
        {
            throw new InvalidOperationException(
                "Kon de NudeNet classifier niet initialiseren. Controleer of de NudityDetector NuGet-package is geïnstalleerd en beschikbaar.");
        }
    }

    public async Task<ImageModerationResult> ModerateAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);

        var tempFilePath = CreateTempFilePath(fileName);

        try
        {
            await using (var fileStream = File.Create(tempFilePath))
            {
                imageStream.Seek(0, SeekOrigin.Begin);
                await imageStream.CopyToAsync(fileStream, cancellationToken);
            }

            var rawResult = await InvokeClassifierAsync(tempFilePath, cancellationToken).ConfigureAwait(false);

            if (!TryExtractUnsafeScore(rawResult, out var unsafeScore))
            {
                logger.LogWarning("NudeNet classifier did not return an unsafe score for file {FileName}", fileName);
                return new ImageModerationResult(true, null);
            }

            var isApproved = unsafeScore < 0.5;
            var failureReason = isApproved
                ? null
                : string.Format(CultureInfo.InvariantCulture, "Afgekeurd door NudeNet (ongepaste score: {0:P0}).", unsafeScore);

            return new ImageModerationResult(isApproved, failureReason);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kon de profielfoto {FileName} niet modereren", fileName);
            return new ImageModerationResult(false, "Er trad een technische fout op tijdens het beoordelen van de foto.");
        }
        finally
        {
            DeleteTempFile(tempFilePath);
        }
    }

    private async Task<object?> InvokeClassifierAsync(string tempFilePath, CancellationToken cancellationToken)
    {
        var parameters = classificationMethod.GetParameters();
        var arguments = new object?[parameters.Length];
        var disposables = new List<IDisposable>(capacity: parameters.Length);

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.ParameterType == typeof(string))
            {
                arguments[i] = tempFilePath;
            }
            else if (parameter.ParameterType == typeof(CancellationToken))
            {
                arguments[i] = cancellationToken;
            }
            else if (typeof(Stream).IsAssignableFrom(parameter.ParameterType))
            {
                var stream = File.OpenRead(tempFilePath);
                arguments[i] = stream;
                disposables.Add(stream);
            }
            else if (parameter.ParameterType == typeof(byte[]))
            {
                arguments[i] = await File.ReadAllBytesAsync(tempFilePath, cancellationToken).ConfigureAwait(false);
            }
            else if (parameter.HasDefaultValue)
            {
                arguments[i] = parameter.DefaultValue;
            }
            else if (parameter.IsOptional)
            {
                arguments[i] = Type.Missing;
            }
            else
            {
                throw new InvalidOperationException($"De NudeNet classifier parameter '{parameter.Name}' met type '{parameter.ParameterType}' wordt niet ondersteund.");
            }
        }

        try
        {
            var rawResult = classificationMethod.Invoke(classifierInstance, arguments);

            if (rawResult is Task task)
            {
                await task.ConfigureAwait(false);
                return GetTaskResult(task);
            }

            return rawResult;
        }
        finally
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }

    private static object? GetTaskResult(Task task)
    {
        var taskType = task.GetType();
        return taskType.IsGenericType
            ? taskType.GetProperty("Result")?.GetValue(task)
            : null;
    }

    private static string CreateTempFilePath(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var tempFileName = $"nudenet_{Guid.NewGuid():N}{extension}";
        return Path.Combine(Path.GetTempPath(), tempFileName);
    }

    private static void DeleteTempFile(string tempFilePath)
    {
        try
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
        catch
        {
            // Swallow cleanup exceptions.
        }
    }

    private static bool TryCreateClassifier(
        out object classifierInstance,
        out MethodInfo classificationMethod,
        out IDisposable? classifierDisposable,
        out IAsyncDisposable? classifierAsyncDisposable)
    {
        classifierInstance = default!;
        classificationMethod = default!;
        classifierDisposable = null;
        classifierAsyncDisposable = null;

        var classifierType = ResolveClassifierType();
        if (classifierType is null)
        {
            return false;
        }

        classifierInstance = Activator.CreateInstance(classifierType)
            ?? throw new InvalidOperationException("Instantiëren van de NudeNet classifier is mislukt.");

        classifierDisposable = classifierInstance as IDisposable;
        classifierAsyncDisposable = classifierInstance as IAsyncDisposable;

        classificationMethod = ResolveClassificationMethod(classifierType);
        return true;
    }

    private static Type? ResolveClassifierType()
    {
        var candidateTypeNames = new[]
        {
            "NudityDetector.Classifier",
            "NudityDetector.NudeNetClassifier",
            "NudeNet.Classifier",
            "NudeNet.NudeNetClassifier"
        };

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var typeName in candidateTypeNames)
            {
                var type = assembly.GetType(typeName, throwOnError: false, ignoreCase: false);
                if (type is not null)
                {
                    return type;
                }
            }
        }

        try
        {
            var nudityAssembly = Assembly.Load("NudityDetector");
            if (nudityAssembly is not null)
            {
                foreach (var typeName in candidateTypeNames)
                {
                    var type = nudityAssembly.GetType(typeName, throwOnError: false, ignoreCase: false);
                    if (type is not null)
                    {
                        return type;
                    }
                }
            }
        }
        catch
        {
            // Ignored - we'll return null below.
        }

        return null;
    }

    private static MethodInfo ResolveClassificationMethod(Type classifierType)
    {
        static bool IsUsableMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            return parameters.Length >= 1 &&
                   parameters.All(p =>
                       p.ParameterType == typeof(string) ||
                       p.ParameterType == typeof(CancellationToken) ||
                       typeof(Stream).IsAssignableFrom(p.ParameterType) ||
                       p.ParameterType == typeof(byte[]) ||
                       p.HasDefaultValue ||
                       p.IsOptional);
        }

        var method = classifierType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => string.Equals(m.Name, "Classify", StringComparison.OrdinalIgnoreCase) && IsUsableMethod(m))
            ?? classifierType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => string.Equals(m.Name, "Predict", StringComparison.OrdinalIgnoreCase) && IsUsableMethod(m));

        if (method is null)
        {
            throw new InvalidOperationException("Kon geen geschikte methode vinden op de NudeNet classifier om afbeeldingen te beoordelen.");
        }

        return method;
    }

    private static bool TryExtractUnsafeScore(object? rawResult, out double unsafeScore)
    {
        unsafeScore = 0d;

        if (rawResult is null)
        {
            return false;
        }

        switch (rawResult)
        {
            case IDictionary<string, float> dictFloat when dictFloat.TryGetValue("unsafe", out var valueFloat):
                unsafeScore = valueFloat;
                return true;
            case IDictionary<string, double> dictDouble when dictDouble.TryGetValue("unsafe", out var valueDouble):
                unsafeScore = valueDouble;
                return true;
            case IDictionary dictionary:
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key is string key && string.Equals(key, "unsafe", StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryConvertToDouble(entry.Value, out unsafeScore))
                        {
                            return true;
                        }
                    }
                }

                break;
        }

        if (TryReadProperty(rawResult, "unsafe", out unsafeScore))
        {
            return true;
        }

        if (rawResult is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is null)
                {
                    continue;
                }

                var label = ReadStringProperty(item, "label") ?? ReadStringProperty(item, "category");
                if (label is null)
                {
                    continue;
                }

                if (!string.Equals(label, "unsafe", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(label, "nsfw", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (TryReadProperty(item, "score", out unsafeScore) || TryReadProperty(item, "probability", out unsafeScore))
                {
                    return true;
                }
            }
        }

        if (TryReadIndexer(rawResult, "unsafe", out unsafeScore))
        {
            return true;
        }

        return false;
    }

    private static bool TryReadProperty(object target, string propertyName, out double value)
    {
        var type = target.GetType();
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is null)
        {
            value = 0d;
            return false;
        }

        return TryConvertToDouble(property.GetValue(target), out value);
    }

    private static string? ReadStringProperty(object target, string propertyName)
    {
        var type = target.GetType();
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(target) as string;
    }

    private static bool TryReadIndexer(object target, string key, out double value)
    {
        var type = target.GetType();
        var indexer = type
            .GetDefaultMembers()
            .OfType<PropertyInfo>()
            .FirstOrDefault(p =>
            {
                var parameters = p.GetIndexParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
            });

        if (indexer is null)
        {
            value = 0d;
            return false;
        }

        var rawValue = indexer.GetValue(target, new object?[] { key });
        return TryConvertToDouble(rawValue, out value);
    }

    private static bool TryConvertToDouble(object? value, out double converted)
    {
        switch (value)
        {
            case null:
                converted = 0d;
                return false;
            case double d:
                converted = d;
                return true;
            case float f:
                converted = f;
                return true;
            case decimal m:
                converted = (double)m;
                return true;
            case int i:
                converted = i;
                return true;
            case long l:
                converted = l;
                return true;
            case string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                converted = parsed;
                return true;
            default:
                converted = 0d;
                return false;
        }
    }

    public void Dispose()
    {
        try
        {
            classifierAsyncDisposable?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch
        {
            // Ignore disposal exceptions coming from async disposable implementations.
        }

        classifierDisposable?.Dispose();

        GC.SuppressFinalize(this);
    }
}
