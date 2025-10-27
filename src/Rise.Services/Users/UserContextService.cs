using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.Users.Mapper;
using Rise.Shared.Identity;
using Rise.Shared.Users;

namespace Rise.Services.Users;
public class UserContextService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IUserContextService
{

    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;

    private static readonly Dictionary<string, SentimentCategoryType> SentimentCategoryLookup = Enum
        .GetValues<SentimentCategoryType>()
        .ToDictionary(
            category => BuildSentimentLookupKey(category.ToString()),
            category => category,
            StringComparer.OrdinalIgnoreCase);

    public async Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentUser = await _dbContext.ApplicationUsers
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var email = (await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken)
            )?.Email
            ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, email)
        });
    }

    public async Task<Result<UserResponse.CurrentUser>> UpdateCurrentUserAsync(
        UserRequest.UpdateCurrentUser request,
        CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentUser = await _dbContext.ApplicationUsers
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var trimmedName = request.Name?.Trim() ?? string.Empty;
        var (firstName, lastName) = SplitName(trimmedName, currentUser.FirstName, currentUser.LastName);

        if (firstName.Length > 100 || lastName.Length > 100)
        {
            return Result.Invalid(new ValidationError(nameof(request.Name), "Naam is te lang."));
        }

        currentUser.FirstName = firstName;
        currentUser.LastName = lastName;

        var biography = (request.Biography ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(biography))
        {
            return Result.Invalid(new ValidationError(nameof(request.Biography), "Bio mag niet leeg zijn."));
        }

        currentUser.Biography = biography;

        var avatarUrl = (request.AvatarUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            return Result.Invalid(new ValidationError(nameof(request.AvatarUrl), "Avatar mag niet leeg zijn."));
        }

        currentUser.AvatarUrl = avatarUrl;

        var hobbiesResult = BuildHobbies(request.HobbyIds);
        if (!hobbiesResult.IsSuccess)
        {
            if (hobbiesResult.ValidationErrors.Any())
            {
                return Result.Invalid(hobbiesResult.ValidationErrors.ToArray());
            }

            var hobbyError = hobbiesResult.Errors.FirstOrDefault() ?? "Ongeldige hobby selectie.";
            return Result.Invalid(new ValidationError(nameof(request.HobbyIds), hobbyError));
        }

        currentUser.UpdateHobbies(hobbiesResult.Value);

        var sentimentsResult = await BuildSentimentsAsync(request, cancellationToken);
        if (!sentimentsResult.IsSuccess)
        {
            if (sentimentsResult.ValidationErrors.Any())
            {
                return Result.Invalid(sentimentsResult.ValidationErrors.ToArray());
            }

            var sentimentsError = sentimentsResult.Errors.FirstOrDefault() ?? "Kon de voorkeuren niet verwerken.";
            return Result.Invalid(new ValidationError(nameof(request.Likes), sentimentsError));
        }

        var updateSentimentsResult = currentUser.UpdateSentiments(sentimentsResult.Value);
        if (!updateSentimentsResult.IsSuccess)
        {
            var updateError = updateSentimentsResult.Errors.FirstOrDefault() ?? "Kon de voorkeuren niet bijwerken.";
            return Result.Invalid(new ValidationError(nameof(request.Likes), updateError));
        }

        var settings = currentUser.UserSettings ?? new ApplicationUserSetting
        {
            FontSize = currentUser.UserSettings?.FontSize ?? 12,
            IsDarkMode = currentUser.UserSettings?.IsDarkMode ?? false,
        };

        currentUser.UserSettings = settings;

        var existingSuggestions = settings.ChatTextLineSuggestions.Select(s => s.Text).ToList();
        foreach (var existing in existingSuggestions)
        {
            var removeResult = settings.RemoveChatTextLine(existing);
            if (!removeResult.IsSuccess)
            {
                var removeError = removeResult.Errors.FirstOrDefault() ?? "Kon bestaande standaardzin niet verwijderen.";
                return Result.Invalid(new ValidationError(nameof(request.DefaultChatLines), removeError));
            }
        }

        var chatLines = (request.DefaultChatLines ?? [])
            .Select(line => line?.Trim() ?? string.Empty)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (var index = 0; index < chatLines.Count; index++)
        {
            var addResult = settings.AddChatTextLine(chatLines[index], index);
            if (!addResult.IsSuccess)
            {
                var chatLineError = addResult.Errors.FirstOrDefault() ?? "Kon de standaardzin niet toevoegen.";
                return Result.Invalid(new ValidationError(nameof(request.DefaultChatLines), chatLineError));
            }
        }

        var identityUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);
        if (identityUser is not null)
        {
            var trimmedEmail = request.Email?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedEmail))
            {
                return Result.Invalid(new ValidationError(nameof(request.Email), "E-mailadres mag niet leeg zijn."));
            }
            identityUser.Email = trimmedEmail;
            identityUser.UserName = trimmedEmail;
            var normalized = trimmedEmail.ToUpperInvariant();
            identityUser.NormalizedEmail = normalized;
            identityUser.NormalizedUserName = normalized;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var email = identityUser?.Email ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, email)
        });
    }

    private static Result<List<UserHobby>> BuildHobbies(IEnumerable<string>? hobbyIds)
    {
        var hobbies = new List<UserHobby>();
        if (hobbyIds is null)
        {
            return Result.Success(hobbies);
        }

        foreach (var rawId in hobbyIds)
        {
            if (string.IsNullOrWhiteSpace(rawId))
            {
                continue;
            }

            if (!Enum.TryParse(rawId, ignoreCase: true, out HobbyType hobby))
            {
                return Result.Invalid(new ValidationError(nameof(UserRequest.UpdateCurrentUser.HobbyIds), $"Onbekende hobby '{rawId}'."));
            }

            hobbies.Add(new UserHobby { Hobby = hobby });
        }

        return Result.Success(hobbies);
    }

    private async Task<Result<List<UserSentiment>>> BuildSentimentsAsync(
        UserRequest.UpdateCurrentUser request,
        CancellationToken cancellationToken)
    {
        if (!TryBuildCategories(request.Likes, nameof(request.Likes), out var likeCategories, out var likeError))
        {
            return Result.Invalid(likeError!);
        }

        if (!TryBuildCategories(request.Dislikes, nameof(request.Dislikes), out var dislikeCategories, out var dislikeError))
        {
            return Result.Invalid(dislikeError!);
        }

        var sentiments = new List<UserSentiment>();

        if (likeCategories.Count > 0)
        {
            var likeSentiments = await _dbContext.Sentiments
                .Where(s => s.Type == SentimentType.Like && likeCategories.Contains(s.Category))
                .ToListAsync(cancellationToken);

            sentiments.AddRange(likeSentiments);
        }

        if (dislikeCategories.Count > 0)
        {
            var dislikeSentiments = await _dbContext.Sentiments
                .Where(s => s.Type == SentimentType.Dislike && dislikeCategories.Contains(s.Category))
                .ToListAsync(cancellationToken);

            sentiments.AddRange(dislikeSentiments);
        }

        if (sentiments.Count != likeCategories.Count + dislikeCategories.Count)
        {
            return Result.Invalid(new ValidationError(nameof(request.Likes), "Kon niet alle interesses vinden."));
        }

        return Result.Success(sentiments);
    }

    private static bool TryBuildCategories(
        IEnumerable<string>? source,
        string fieldName,
        out HashSet<SentimentCategoryType> categories,
        out ValidationError? error)
    {
        categories = new HashSet<SentimentCategoryType>();
        error = null;

        if (source is null)
        {
            return true;
        }

        foreach (var value in source)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!TryResolveSentimentCategory(value, out var category))
            {
                error = new ValidationError(fieldName, $"Onbekende voorkeur '{value}'.");
                return false;
            }

            categories.Add(category);
        }

        return true;
    }

    private static bool TryResolveSentimentCategory(string value, out SentimentCategoryType category)
    {
        var normalized = value.Trim();
        if (SentimentCategoryLookup.TryGetValue(normalized, out category))
        {
            return true;
        }

        return Enum.TryParse(normalized, ignoreCase: true, out category);
    }

    private static (string FirstName, string LastName) SplitName(string name, string fallbackFirst, string fallbackLast)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (fallbackFirst, fallbackLast);
        }

        var parts = name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return (fallbackFirst, fallbackLast);
        }

        var firstName = parts[0];
        var lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : parts[0];

        return (firstName, lastName);
    }

    private static string BuildSentimentLookupKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length + 5);
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (char.IsUpper(character) && i > 0)
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}
