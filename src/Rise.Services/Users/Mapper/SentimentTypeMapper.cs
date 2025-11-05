using Rise.Domain.Users.Sentiment;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;
public static class SentimentTypeMapper
{
    public static SentimentTypeDto ToDto(this SentimentType category)
        => (SentimentTypeDto)category;

    public static SentimentType ToDomain(this SentimentTypeDto dto)
        => (SentimentType)dto;
}
