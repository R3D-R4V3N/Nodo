using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users.Hobbys;
using Rise.Persistence;
using Rise.Shared.Users;
using System;

namespace Rise.Services.Users.Mapper;

internal static class HobbyMapper
{
    public static HobbyDto.Get ToGetDto(UserHobby hobby)
    {
        return new HobbyDto.Get
        {
            Hobby = hobby.Hobby.ToDto(),
        };
    }

    public static async Task<Result<UserHobby>> ToDomainAsync(
        HobbyDto.EditProfile hobbyDto, 
        ApplicationDbContext dbContext, 
        CancellationToken ct)
    {
        if (hobbyDto is null)
        {
            return Result.Invalid(new ValidationError(nameof(HobbyDto), $"Lege hobby meegegeven."));
        }

        HobbyType hobby = hobbyDto.Hobby.ToDomain();

        var userHobby = await dbContext
            .Hobbies
            .FirstOrDefaultAsync(x => x.Hobby.Equals(hobby), ct);

        if (userHobby is null)
        {
            return Result.Conflict($"Onbekende hobby {hobby}");
        }

        return Result.Success(userHobby);
    }

    public static async Task<Result<List<UserHobby>>> ToDomainAsync(
        IEnumerable<HobbyDto.EditProfile> hobbyDtos, 
        ApplicationDbContext dbContext, 
        CancellationToken ct)
    {
        if (hobbyDtos is null)
        {
            return Result.Success(new List<UserHobby>());
        }

        var results = new List<UserHobby>();

        foreach (var hobbyDto in hobbyDtos)
        {
            var result = await ToDomainAsync(hobbyDto, dbContext, ct);

            if (!result.IsSuccess)
            {
                if (result.ValidationErrors.Any())
                { 
                    return Result.Invalid(result.ValidationErrors);
                }
                return Result.Conflict(result.Errors.ToArray());
            }

            results.Add(result.Value);
        }

        return Result.Success(results);
    }
}
