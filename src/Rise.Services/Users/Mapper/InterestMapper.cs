using Rise.Domain.Users;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;

internal static class InterestMapper
{
    public static UserInterestDto ToDto(UserInterest interest) => new()
    {
        Type = interest.Type,
        Like = interest.Like,
        Dislike = interest.Dislike,
    };
}
