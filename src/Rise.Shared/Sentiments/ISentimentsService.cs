using Rise.Shared.Sentiments;
using Rise.Shared.Common;

namespace Rise.Shared.UserSentiments
{
    public interface ISentimentsService
    {
        Task<Result<SentimentResponse.GetSentiments>> GetSentimentsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    }
}
