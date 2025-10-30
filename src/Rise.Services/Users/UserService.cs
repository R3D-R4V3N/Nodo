using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Services.Users.Mapper;
using Rise.Shared.Users;

namespace Rise.Services.Users;

public class UserService(
    ApplicationDbContext dbContext) : IUserService
{

    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Error<UserResponse.CurrentUser>("Ongeldig account ID.");
        }

        var userProfile = await _dbContext.ApplicationUsers
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .Include(u => u.UserSettings.ChatTextLineSuggestions)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (userProfile is null)
        {
            return Result.NotFound("Gebruiker niet gevonden.");
        }

        var email = (await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken)
            )?.Email
            ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(userProfile, email)
        });
    }
}
