using Rise.Domain.Users.Sentiment;
using Rise.Shared.Sentiments;

namespace Rise.Services.Sentiments.Mapper;
public static class SentimentCategoryTypeMapper
{
    public static SentimentCategoryTypeDto ToDto(this SentimentCategoryType category)
        => (SentimentCategoryTypeDto)category;

    public static SentimentCategoryType ToDomain(this SentimentCategoryTypeDto dto)
        => (SentimentCategoryType)dto;
}
