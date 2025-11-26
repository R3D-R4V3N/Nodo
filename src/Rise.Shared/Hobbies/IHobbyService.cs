using Rise.Shared.Common;

namespace Rise.Shared.Hobbies;

public interface IHobbyService
{
    Task<Result<HobbyResponse.GetHobbies>> GetHobbiesAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
}
