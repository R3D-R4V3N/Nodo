using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Rise.Domain.Common;

namespace Rise.Domain.Users;

public static class UserInterestConstants
{
    public const string DefaultGender = "x";

    public static readonly HashSet<string> AllowedGenders =
        new(StringComparer.OrdinalIgnoreCase) { "man", "vrouw", "x" };
}

public class UserInterest : ValueObject
{
    public UserInterest(string interestId)
    {
        InterestId = Guard.Against.NullOrWhiteSpace(interestId).Trim().ToLowerInvariant();
    }

    public string InterestId { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return InterestId;
    }
}
