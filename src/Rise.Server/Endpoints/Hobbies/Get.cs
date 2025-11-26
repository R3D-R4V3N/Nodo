using Rise.Shared.Common;
using Rise.Shared.Hobbies;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Hobbies;

public class Get(IHobbyService hobbyService) : Endpoint<QueryRequest.SkipTake, Result<HobbyResponse.GetHobbies>>
{
    public override void Configure()
    {
        Get("/api/hobbies");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<HobbyResponse.GetHobbies>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        return hobbyService.GetHobbiesAsync(req, ct);
    }
}