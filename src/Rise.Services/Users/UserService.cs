using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.Users.Mapper;
using Rise.Shared.Identity;
using Rise.Shared.Users;
using System.Linq;

namespace Rise.Services.Users;
public class UserService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IUserService
{
    private const int MaxPreferences = 5;
    private const int MaxHobbies = 3;

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

        await LoadProfileAsync(currentUser, cancellationToken);
        var email = await GetEmailAsync(accountId, cancellationToken);

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
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        await LoadProfileAsync(currentUser, cancellationToken);

        UpdateBasicDetails(currentUser, request);
        UpdateInterests(currentUser, request);
        UpdateHobbies(currentUser, request);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await LoadProfileAsync(currentUser, cancellationToken);
        var email = await GetEmailAsync(accountId, cancellationToken);

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, email)
        });
    }

    private async Task<string> GetEmailAsync(string accountId, CancellationToken cancellationToken)
    {
        var identityUser = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);

        return identityUser?.Email ?? string.Empty;
    }

    private async Task LoadProfileAsync(ApplicationUser currentUser, CancellationToken cancellationToken)
    {
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
    }

    private static void UpdateBasicDetails(ApplicationUser currentUser, UserRequest.UpdateCurrentUser request)
    {
        var normalizedName = request.Name?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            var parts = normalizedName
                .Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length >= 1)
            {
                currentUser.FirstName = parts[0];
            }

            if (parts.Length >= 2)
            {
                currentUser.LastName = parts[1];
            }
        }

        var biography = request.Biography?.Trim();
        if (!string.IsNullOrWhiteSpace(biography))
        {
            currentUser.Biography = biography;
        }

        var avatarUrl = request.AvatarUrl?.Trim();
        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            currentUser.AvatarUrl = avatarUrl;
        }
    }

    private static void UpdateInterests(ApplicationUser currentUser, UserRequest.UpdateCurrentUser request)
    {
        var interests = new List<UserInterest>();

        if (request.Likes is not null)
        {
            foreach (var like in request.Likes.Take(MaxPreferences))
            {
                var normalized = NormalizePreference(like);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    interests.Add(UserInterest.Create("Like", normalized, null));
                }
            }
        }

        if (request.Dislikes is not null)
        {
            foreach (var dislike in request.Dislikes.Take(MaxPreferences))
            {
                var normalized = NormalizePreference(dislike);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    interests.Add(UserInterest.Create("Dislike", null, normalized));
                }
            }
        }

        currentUser.UpdateInterests(interests);
    }

    private static void UpdateHobbies(ApplicationUser currentUser, UserRequest.UpdateCurrentUser request)
    {
        var hobbies = new List<UserHobby>();

        if (request.HobbyIds is not null)
        {
            foreach (var hobbyId in request.HobbyIds.Take(MaxHobbies))
            {
                if (Enum.TryParse<HobbyType>(hobbyId, true, out var hobby))
                {
                    hobbies.Add(UserHobby.Create(hobby));
                }
            }
        }

        currentUser.UpdateHobbies(hobbies);
    }

    private static string NormalizePreference(string value)
        => value?.Trim() ?? string.Empty;
}
