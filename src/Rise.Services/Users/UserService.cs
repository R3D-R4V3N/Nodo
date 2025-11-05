using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users.Properties;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.Users.Mapper;
using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Users;

namespace Rise.Services.Users;

public class UserService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IUserService
{

    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;

    public async Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken cancellationToken = default)
    {
        // if (string.IsNullOrWhiteSpace(accountId))
        // {
        //     return Result.Unauthorized();
        // }

        var previous = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("AccountId: " + accountId);


        var currentUser = await _dbContext.Users.Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        Console.WriteLine("Account" + currentUser);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        Console.ForegroundColor = previous; // restore

        var email = (await _dbContext.Users
                        .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken)
                    )?.Email
                    ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, email)
        });
    }

    public async Task<Result<UserResponse.CurrentUser>> UpdateCurrentUserAsync(
        UserRequest.UpdateCurrentUser request,
        CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentUser = await _dbContext.ApplicationUsers
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        currentUser.FirstName = FirstName.Create(request.FirstName);
        currentUser.LastName = LastName.Create(request.LastName);
        currentUser.Biography = Biography.Create(request.Biography);
        currentUser.AvatarUrl = AvatarUrl.Create(request.AvatarUrl);
        currentUser.Gender = request.Gender.ToDomain();

        // can use IAsyncEnumerable but a pain to work with
        var hobbiesResult = await HobbyMapper.ToDomainAsync(request.Hobbies, _dbContext, cancellationToken);

        if (!hobbiesResult.IsSuccess)
        {
            // not really a fan of this but dont know a better way atm
            if (hobbiesResult.ValidationErrors.Any())
            {
                return Result.Invalid(hobbiesResult.ValidationErrors);
            }

            return Result.Conflict(hobbiesResult.Errors.ToArray());
        }

        currentUser.UpdateHobbies(hobbiesResult.Value);

        var sentimentsResult = await SentimentMapper.ToDomainAsync(request.Sentiments, _dbContext, cancellationToken);
        if (!sentimentsResult.IsSuccess)
        {
            if (sentimentsResult.ValidationErrors.Any())
            {
                return Result.Invalid(sentimentsResult.ValidationErrors);
            }

            return Result.Conflict(sentimentsResult.Errors.ToArray());
        }

        var updateSentimentsResult = currentUser.UpdateSentiments(sentimentsResult.Value);
        if (!updateSentimentsResult.IsSuccess)
        {
            var updateError = updateSentimentsResult.Errors.FirstOrDefault() ?? "Kon de voorkeuren niet bijwerken.";
            return Result.Invalid(new ValidationError(nameof(request.Sentiments), updateError));
        }

        // can also add an update function
        // this is small enough where it doesnt really matter
        currentUser.UserSettings.RemoveChatTextLines();

        var chatLines = request.DefaultChatLines ?? [];
        foreach (var chatLine in chatLines)
        {
            var censored = WordFilter.Censor(chatLine);
            var addResult = currentUser.UserSettings.AddChatTextLine(censored);
            if (!addResult.IsSuccess)
            {
                if (addResult.ValidationErrors.Any())
                {
                    return Result.Invalid(addResult.ValidationErrors);
                }

                return Result.Conflict(addResult.Errors.ToArray());
            }
        }

        var identityUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);
        if (identityUser is not null)
        {
            var trimmedEmail = request.Email?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedEmail))
            {
                return Result.Invalid(new ValidationError(nameof(request.Email), "E-mailadres mag niet leeg zijn."));
            }
            identityUser.Email = trimmedEmail;
            identityUser.UserName = trimmedEmail;
            var normalized = trimmedEmail.ToUpperInvariant();
            identityUser.NormalizedEmail = normalized;
            identityUser.NormalizedUserName = normalized;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var email = identityUser?.Email ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, email)
        });
    }
}