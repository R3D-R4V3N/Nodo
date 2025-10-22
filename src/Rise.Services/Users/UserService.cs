using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.Users.Mapper;
using Rise.Shared.Identity;
using Rise.Shared.Users;

namespace Rise.Services.Users;
public class UserService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IUserService
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

        var currentUser = await _dbContext.ApplicationUsers
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var identityUser = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);

        var userEntry = _dbContext.Entry(currentUser);

        var userSettingsReference = userEntry.Reference<ApplicationUserSetting>("_userSettings");
        if (!userSettingsReference.IsLoaded)
        {
            await userSettingsReference.LoadAsync(cancellationToken);
        }

        if (userSettingsReference.TargetEntry is { } settingsEntry)
        {
            var suggestions = settingsEntry.Collection(s => s.ChatTextLineSuggestions);
            if (!suggestions.IsLoaded)
            {
                await suggestions.LoadAsync(cancellationToken);
            }
        }

        var interestsCollection = userEntry.Collection<UserInterest>("_interests");
        if (!interestsCollection.IsLoaded)
        {
            await interestsCollection.LoadAsync(cancellationToken);
        }

        var hobbiesCollection = userEntry.Collection<UserHobby>("_hobbies");
        if (!hobbiesCollection.IsLoaded)
        {
            await hobbiesCollection.LoadAsync(cancellationToken);
        }

        var email = identityUser?.Email ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, email)
        });
    }
}
