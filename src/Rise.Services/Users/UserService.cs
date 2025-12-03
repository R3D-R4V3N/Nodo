using Microsoft.EntityFrameworkCore;
using Rise.Domain.Common;
using Rise.Domain.Common.ValueObjects;
using Rise.Persistence;
using Rise.Services.Hobbies.Mapper;
using Rise.Services.Identity;
using Rise.Services.Sentiments.Mapper;
using Rise.Services.Users.Mapper;
using Rise.Shared.Common;
using Rise.Shared.Hobbies;
using Rise.Shared.Sentiments;
using Rise.Shared.Users;

namespace Rise.Services.Users;

public class UserService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IUserService
{
    public async Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken ctx = default)
    {
        var loggedInUserId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.Users
            .SingleOrDefaultAsync(x => x.AccountId == loggedInUserId, ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("U heeft geen toegang om een gebruiker te verkrijgen.");

        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.NotFound();
        }

        var currentUser = await dbContext
            .Users
            .Include(u => u.Hobbies)
            .Include(u => u.Sentiments)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, ctx);

        if (currentUser is null)
        {
            return Result.NotFound("Meegegeven ID heeft geen geldig profiel.");
        }

        var email = (await dbContext.IdentityUsers
                        .SingleOrDefaultAsync(u => u.Id == accountId, ctx)
                    )?.Email
                    ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, email)
        });
    }

    public async Task<Result<UserResponse.CurrentUser>> UpdateUserAsync(
        string userToChangeAccountId,
        UserRequest.UpdateCurrentUser request,
        CancellationToken cancellationToken = default)
    {
        var loggedInUserId = sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(userToChangeAccountId) || string.IsNullOrWhiteSpace(loggedInUserId))
        {
            return Result.Unauthorized();
        }

        var userToChange = await dbContext
            .Users
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == userToChangeAccountId, cancellationToken);

        if (userToChange is null)
        {
            return Result.Unauthorized("Meegegeven id heeft geen geldig profiel.");
        }

        if (!userToChangeAccountId.Equals(loggedInUserId))
        {
            var loggedInUser = await dbContext
                .Supervisors
                .SingleOrDefaultAsync(u => u.AccountId == loggedInUserId, cancellationToken);

            if (loggedInUser is null)
            {
                return Result.Unauthorized();
            }
        }

        userToChange.FirstName = FirstName.Create(request.FirstName);
        userToChange.LastName = LastName.Create(request.LastName);
        userToChange.Biography = Biography.Create(request.Biography);
        userToChange.AvatarUrl = AvatarUrl.Create(request.AvatarUrl);
        userToChange.Gender = request.Gender.ToDomain();

        var hobbiesResult = await HobbyMapper.ToDomainAsync(request.Hobbies, dbContext, cancellationToken);

        if (!hobbiesResult.IsSuccess)
        {
            if (hobbiesResult.ValidationErrors.Any())
            {
                return Result.Invalid(hobbiesResult.ValidationErrors);
            }

            return Result.Conflict(hobbiesResult.Errors.ToArray());
        }

        var updateHobbiesResult = userToChange.UpdateHobbies(hobbiesResult.Value);
        if (!updateHobbiesResult.IsSuccess)
        {
            var updateError = updateHobbiesResult.Errors.FirstOrDefault() ?? "Kon de voorkeuren niet bijwerken.";
            return Result.Invalid(new ValidationError(nameof(request.Sentiments), updateError));
        }

        var sentimentsResult = await SentimentMapper.ToDomainAsync(request.Sentiments, dbContext, cancellationToken);
        if (!sentimentsResult.IsSuccess)
        {
            if (sentimentsResult.ValidationErrors.Any())
            {
                return Result.Invalid(sentimentsResult.ValidationErrors);
            }

            return Result.Conflict(sentimentsResult.Errors.ToArray());
        }

        var updateSentimentsResult = userToChange.UpdateSentiments(sentimentsResult.Value);
        if (!updateSentimentsResult.IsSuccess)
        {
            var updateError = updateSentimentsResult.Errors.FirstOrDefault() ?? "Kon de voorkeuren niet bijwerken.";
            return Result.Invalid(new ValidationError(nameof(request.Sentiments), updateError));
        }

        userToChange.UserSettings.RemoveChatTextLines();

        var chatLines = request.DefaultChatLines ?? [];
        foreach (var chatLine in chatLines)
        {
            var censored = WordFilter.Censor(chatLine);
            var addResult = userToChange.UserSettings.AddChatTextLine(censored);
            if (!addResult.IsSuccess)
            {
                if (addResult.ValidationErrors.Any())
                {
                    return Result.Invalid(addResult.ValidationErrors);
                }

                return Result.Conflict(addResult.Errors.ToArray());
            }
        }

        var identityUser = await dbContext
            .IdentityUsers
            .SingleOrDefaultAsync(u => u.Id == userToChangeAccountId, cancellationToken);

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

        await dbContext.SaveChangesAsync(cancellationToken);

        var email = identityUser?.Email ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(userToChange, email)
        });
    }

    public async Task<Result<UserResponse.CurrentUser>> UpdateProfileInfoAsync(
        string userToChangeAccountId,
        UserRequest.UpdateProfileInfo request,
        CancellationToken cancellationToken = default)
    {
        var loggedInUserId = sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(userToChangeAccountId) || string.IsNullOrWhiteSpace(loggedInUserId))
        {
            return Result.Unauthorized();
        }

        var userToChange = await dbContext
            .Users
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == userToChangeAccountId, cancellationToken);

        if (userToChange is null)
        {
            return Result.Unauthorized("Meegegeven id heeft geen geldig profiel.");
        }

        if (!userToChangeAccountId.Equals(loggedInUserId))
        {
            var loggedInUser = await dbContext
                .Supervisors
                .SingleOrDefaultAsync(u => u.AccountId == loggedInUserId, cancellationToken);

            if (loggedInUser is null)
            {
                return Result.Unauthorized();
            }
        }

        userToChange.FirstName = FirstName.Create(request.FirstName);
        userToChange.LastName = LastName.Create(request.LastName);
        userToChange.Biography = Biography.Create(request.Biography);
        userToChange.AvatarUrl = AvatarUrl.Create(request.AvatarUrl);
        userToChange.Gender = request.Gender.ToDomain();

        var identityUser = await dbContext
            .IdentityUsers
            .SingleOrDefaultAsync(u => u.Id == userToChangeAccountId, cancellationToken);

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

        await dbContext.SaveChangesAsync(cancellationToken);

        var email = identityUser?.Email ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(userToChange, email)
        });
    }

    public async Task<Result<UserResponse.CurrentUser>> UpdatePreferencesAsync(
        string userToChangeAccountId,
        UserRequest.UpdatePreferences request,
        CancellationToken cancellationToken = default)
    {
        var loggedInUserId = sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(userToChangeAccountId) || string.IsNullOrWhiteSpace(loggedInUserId))
        {
            return Result.Unauthorized();
        }

        var userToChange = await dbContext
            .Users
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == userToChangeAccountId, cancellationToken);

        if (userToChange is null)
        {
            return Result.Unauthorized("Meegegeven id heeft geen geldig profiel.");
        }

        if (!userToChangeAccountId.Equals(loggedInUserId))
        {
            var loggedInUser = await dbContext
                .Supervisors
                .SingleOrDefaultAsync(u => u.AccountId == loggedInUserId, cancellationToken);

            if (loggedInUser is null)
            {
                return Result.Unauthorized();
            }
        }

        var hobbiesResult = await HobbyMapper.ToDomainAsync(request.Hobbies, dbContext, cancellationToken);

        if (!hobbiesResult.IsSuccess)
        {
            if (hobbiesResult.ValidationErrors.Any())
            {
                return Result.Invalid(hobbiesResult.ValidationErrors);
            }

            return Result.Conflict(hobbiesResult.Errors.ToArray());
        }

        var updateHobbiesResult = userToChange.UpdateHobbies(hobbiesResult.Value);
        if (!updateHobbiesResult.IsSuccess)
        {
            var updateError = updateHobbiesResult.Errors.FirstOrDefault() ?? "Kon de voorkeuren niet bijwerken.";
            return Result.Invalid(new ValidationError(nameof(request.Sentiments), updateError));
        }

        var sentimentsResult = await SentimentMapper.ToDomainAsync(request.Sentiments, dbContext, cancellationToken);
        if (!sentimentsResult.IsSuccess)
        {
            if (sentimentsResult.ValidationErrors.Any())
            {
                return Result.Invalid(sentimentsResult.ValidationErrors);
            }

            return Result.Conflict(sentimentsResult.Errors.ToArray());
        }

        var updateSentimentsResult = userToChange.UpdateSentiments(sentimentsResult.Value);
        if (!updateSentimentsResult.IsSuccess)
        {
            var updateError = updateSentimentsResult.Errors.FirstOrDefault() ?? "Kon de voorkeuren niet bijwerken.";
            return Result.Invalid(new ValidationError(nameof(request.Sentiments), updateError));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var email = (await dbContext.IdentityUsers
                        .SingleOrDefaultAsync(u => u.Id == userToChangeAccountId, cancellationToken)
                    )?.Email
                    ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(userToChange, email)
        });
    }

    public async Task<Result<UserResponse.CurrentUser>> UpdateChatLinesAsync(
        string userToChangeAccountId,
        UserRequest.UpdateChatLines request,
        CancellationToken cancellationToken = default)
    {
        var loggedInUserId = sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(userToChangeAccountId) || string.IsNullOrWhiteSpace(loggedInUserId))
        {
            return Result.Unauthorized();
        }

        var userToChange = await dbContext
            .Users
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == userToChangeAccountId, cancellationToken);

        if (userToChange is null)
        {
            return Result.Unauthorized("Meegegeven id heeft geen geldig profiel.");
        }

        if (!userToChangeAccountId.Equals(loggedInUserId))
        {
            var loggedInUser = await dbContext
                .Supervisors
                .SingleOrDefaultAsync(u => u.AccountId == loggedInUserId, cancellationToken);

            if (loggedInUser is null)
            {
                return Result.Unauthorized();
            }
        }

        userToChange.UserSettings.RemoveChatTextLines();

        var chatLines = request.DefaultChatLines ?? [];
        foreach (var chatLine in chatLines)
        {
            var censored = WordFilter.Censor(chatLine);
            var addResult = userToChange.UserSettings.AddChatTextLine(censored);
            if (!addResult.IsSuccess)
            {
                if (addResult.ValidationErrors.Any())
                {
                    return Result.Invalid(addResult.ValidationErrors);
                }

                return Result.Conflict(addResult.Errors.ToArray());
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var email = (await dbContext.IdentityUsers
                        .SingleOrDefaultAsync(u => u.Id == userToChangeAccountId, cancellationToken)
                    )?.Email
                    ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(userToChange, email)
        });
    }
}
