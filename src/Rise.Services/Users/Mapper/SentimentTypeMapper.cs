using Rise.Domain.Users.Sentiment;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;
public static class SentimentTypeMapper
{
    public static SentimentTypeDto MapToDto(this SentimentType type)
    => type switch
    {
        SentimentType.Like => SentimentTypeDto.Like,
        SentimentType.Dislike => SentimentTypeDto.Dislike,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static SentimentType MapToDomain(this SentimentTypeDto type)
        => type switch
        {
            SentimentTypeDto.Like => SentimentType.Like,
            SentimentTypeDto.Dislike => SentimentType.Dislike,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}
