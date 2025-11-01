using System;
using System.Collections.Generic;
using Ardalis.Result;
using Rise.Domain.Common;

namespace Rise.Domain.Organizations.Properties;

public sealed class OrganizationName : ValueObject, IProperty<OrganizationName, string>
{
    private OrganizationName()
    {
    }

    public const int MAX_LENGTH = 200;

    public string Value { get; private set; } = default!;

    public static Result<OrganizationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Organisatienaam is leeg.");
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MAX_LENGTH)
        {
            return Result.Conflict("Organisatienaam is te lang.");
        }

        return Result.Success(new OrganizationName
        {
            Value = trimmedValue,
        });
    }

    public static implicit operator string(OrganizationName name) => name.Value;

    public static explicit operator OrganizationName(string value)
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
