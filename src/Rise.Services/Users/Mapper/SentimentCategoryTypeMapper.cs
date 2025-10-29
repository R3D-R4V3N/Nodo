using Rise.Domain.Users.Sentiment;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;
public static class SentimentCategoryTypeMapper
{
    public static SentimentCategoryTypeDto ToDto(this SentimentCategoryType category)
        => (SentimentCategoryTypeDto)category;

    public static SentimentCategoryType ToDomain(this SentimentCategoryTypeDto dto)
        => (SentimentCategoryType)dto;
}
