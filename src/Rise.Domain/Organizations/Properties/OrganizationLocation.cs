using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Rise.Domain.Common;

namespace Rise.Domain.Organizations.Properties;

public sealed class OrganizationLocation : ValueObject
{
    private string _name = default!;
    private string _zipCode = default!;
    private string? _city;
    private string? _street;

    private OrganizationLocation()
    {
    }

    public OrganizationLocation(
        string name,
        string zipCode,
        string? city = null,
        string? street = null)
    {
        Name = name;
        ZipCode = zipCode;
        City = city;
        Street = street;
    }

    public const int NAME_MAX_LENGTH = 200;
    public const int ZIPCODE_MAX_LENGTH = 32;
    public const int CITY_MAX_LENGTH = 200;
    public const int STREET_MAX_LENGTH = 200;

    public string Name
    {
        get => _name;
        set => _name = NormalizeRequired(value, NAME_MAX_LENGTH, nameof(Name));
    }

    public string ZipCode
    {
        get => _zipCode;
        set => _zipCode = NormalizeRequired(value, ZIPCODE_MAX_LENGTH, nameof(ZipCode));
    }

    public string? City
    {
        get => _city;
        set => _city = NormalizeOptional(value, CITY_MAX_LENGTH);
    }

    public string? Street
    {
        get => _street;
        set => _street = NormalizeOptional(value, STREET_MAX_LENGTH);
    }

    public override string ToString()
    {
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(Street))
        {
            segments.Add(Street!);
        }

        var citySegment = string.Join(' ', new[] { ZipCode, City }.Where(part => !string.IsNullOrWhiteSpace(part)));
        if (!string.IsNullOrWhiteSpace(citySegment))
        {
            segments.Add(citySegment);
        }

        segments.Add(Name);

        return string.Join(", ", segments);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return ZipCode;
        yield return City;
        yield return Street;
    }

    private static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        Guard.Against.NullOrWhiteSpace(value, parameterName);

        var trimmed = value.Trim();
        Guard.Against.InvalidInput(trimmed, parameterName, v => v.Length <= maxLength, $"{parameterName} is te lang.");

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        Guard.Against.InvalidInput(trimmed, nameof(value), v => v.Length <= maxLength, "Waarde is te lang.");

        return trimmed;
    }
}
