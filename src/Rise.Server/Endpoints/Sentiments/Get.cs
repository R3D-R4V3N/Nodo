using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Sentiments;
using Rise.Shared.UserSentiments;

namespace Rise.Server.Endpoints.Sentiments;

public class Get(ISentimentsService sentimentsService) : Endpoint<QueryRequest.SkipTake, Result<SentimentResponse.GetSentiments>>
{
    public override void Configure()
    {
        Get("/api/sentiments");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<SentimentResponse.GetSentiments>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        return sentimentsService.GetSentimentsAsync(req, ct);
    }
}