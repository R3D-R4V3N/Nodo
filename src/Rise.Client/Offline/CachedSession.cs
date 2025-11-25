using Rise.Shared.Identity.Accounts;
using Rise.Shared.Users;

namespace Rise.Client.Offline;

public sealed record CachedSession
{
    public string? AccountId { get; init; }
    public AccountResponse.Info? AccountInfo { get; init; }
    public UserDto.CurrentUser? User { get; init; }
}
