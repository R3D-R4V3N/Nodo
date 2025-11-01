using System;
using System.Collections.Generic;
using Ardalis.Result;
using Rise.Domain.Common;

namespace Rise.Domain.Organizations.Properties;

public sealed class OrganizationLocation : ValueObject, IProperty<OrganizationLocation, string>
{
    private OrganizationLocation()
    {
    }

    public const int MAX_LENGTH = 200;

    public string Value { get; private set; } = default!;

    public static Result<OrganizationLocation> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Organisatielocatie is leeg.");
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MAX_LENGTH)
        {
            return Result.Conflict("Organisatielocatie is te lang.");
        }

        return Result.Success(new OrganizationLocation
        {
            Value = trimmedValue,
        });
    }

    public static implicit operator string(OrganizationLocation location) => location.Value;

    public static explicit operator OrganizationLocation(string value)
    {
        var result = Create(value);
        if (!result.IsSuccess)
        {
            throw new ArgumentException(string.Join(',', result.Errors), nameof(value));
        }

        return result.Value;
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
