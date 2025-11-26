using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.Users.Mapper;
using Rise.Shared.Assets;
using Rise.Shared.Identity;
using Rise.Shared.Users;

namespace Rise.Services.Users;
public class UserContextService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IUserContextService
{

    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;

    public async Task<Result<UserResponse.CurrentUser>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var principal = _sessionContextProvider.User;

        if (principal is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        BaseUser? profile = principal switch
        {
            var p when p.IsInRole(AppRoles.User) => await _dbContext
                .Users
                .Include(u => u.Sentiments)
                .Include(u => u.Hobbies)
                .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken),

            var p when p.IsInRole(AppRoles.Supervisor) => await _dbContext
                .Supervisors
                .SingleOrDefaultAsync(s => s.AccountId == accountId, cancellationToken),

            var p when p.IsInRole(AppRoles.Administrator) => await _dbContext
                .Admins
                .SingleOrDefaultAsync(s => s.AccountId == accountId, cancellationToken),
            _ => null
        };

        if (profile is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var identityUser = await _dbContext
            .IdentityUsers
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);

        var email = identityUser?.Email ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = profile is User user
                ? user.ToCurrentUserDto(email)
                : profile.ToCurrentUserDto(email)
        });
    }
}
