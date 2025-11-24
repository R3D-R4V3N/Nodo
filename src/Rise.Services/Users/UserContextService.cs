using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
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

        BaseUser? profile = await _dbContext
            .Users
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (profile is null)
        {
            profile = await _dbContext
                .Supervisors
                .SingleOrDefaultAsync(s => s.AccountId == accountId, cancellationToken);
        }

        var identityUser = await _dbContext
            .IdentityUsers
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);

        var email = identityUser?.Email ?? string.Empty;

        if (profile is not null)
        {
            return Result.Success(new UserResponse.CurrentUser
            {
                User = profile is User user
                    ? user.ToCurrentUserDto(email)
                    : profile.ToCurrentUserDto(email)
            });
        }

        var principal = _sessionContextProvider.User;

        if (principal is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        if (!principal.IsInRole(AppRoles.Supervisor) && !principal.IsInRole(AppRoles.Administrator))
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var (firstName, lastName) = ExtractName(principal, email);

        var fallbackUser = new UserDto.CurrentUser
        {
            Id = 0,
            FirstName = firstName,
            LastName = lastName,
            AccountId = accountId,
            AvatarUrl = DefaultImages.GetProfile(email),
            Email = email,
            Biography = "Profielinformatie is nog niet ingesteld.",
            Gender = GenderTypeDto.X,
            BirthDay = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt = DateTime.UtcNow,
            Interests = [],
            Hobbies = [],
            DefaultChatLines = []
        };

        return Result.Success(new UserResponse.CurrentUser
        {
            User = fallbackUser
        });
    }

    private static (string FirstName, string LastName) ExtractName(ClaimsPrincipal principal, string email)
    {
        var firstName = principal.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = principal.FindFirst(ClaimTypes.Surname)?.Value;

        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
        {
            return (firstName, lastName);
        }

        var fallback = email.Split('@').FirstOrDefault() ?? string.Empty;
        var parts = fallback
            .Split(['.', '-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length >= 2)
        {
            firstName ??= parts[0];
            lastName ??= string.Join(' ', parts.Skip(1));
        }
        else if (parts.Length == 1)
        {
            firstName ??= parts[0];
            lastName ??= "Account";
        }

        firstName ??= "Supervisor";
        lastName ??= principal.IsInRole(AppRoles.Administrator) ? "Administrator" : "Account";

        return (firstName, lastName);
    }
}
