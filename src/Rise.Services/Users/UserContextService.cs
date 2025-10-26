using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ardalis.Result;
using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.Users.Mapper;
using Rise.Shared.Identity;
using Rise.Shared.Users;
using Rise.Shared.Common;

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

        var currentUser = await _dbContext.ApplicationUsers
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        await LoadUserSettingsAsync(currentUser, cancellationToken);

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
        if (request is null)
        {
            return Result.Invalid(new ValidationError(nameof(request), "Geen gegevens ontvangen."));
        }

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

        var identityUser = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);

        if (identityUser is null)
        {
            return Result.Error("Het gekoppelde account kon niet worden gevonden.");
        }

        var validationErrors = new List<ValidationError>();

        var trimmedName = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            validationErrors.Add(new ValidationError(nameof(request.Name), "Naam is verplicht."));
        }

        var trimmedEmail = request.Email?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedEmail))
        {
            validationErrors.Add(new ValidationError(nameof(request.Email), "E-mailadres is verplicht."));
        }

        var trimmedAvatar = request.AvatarUrl?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(trimmedAvatar) && trimmedAvatar.Length > 250)
        {
            validationErrors.Add(new ValidationError(nameof(request.AvatarUrl), "De avatar-URL is te lang (maximaal 250 tekens)."));
        }

        if (validationErrors.Count > 0)
        {
            return Result.Invalid(validationErrors.ToArray());
        }

        var (firstName, lastName) = SplitName(trimmedName);
        currentUser.FirstName = firstName;
        currentUser.LastName = lastName;

        var biography = string.IsNullOrWhiteSpace(request.Biography)
            ? "Nog geen bio beschikbaar."
            : request.Biography.Trim();
        currentUser.Biography = biography;
        currentUser.Gender = NormalizeGender(request.Gender);

        if (!string.IsNullOrWhiteSpace(trimmedAvatar))
        {
            currentUser.AvatarUrl = trimmedAvatar;
        }

        identityUser.Email = trimmedEmail;
        identityUser.UserName = trimmedEmail;
        identityUser.NormalizedEmail = trimmedEmail.ToUpperInvariant();
        identityUser.NormalizedUserName = trimmedEmail.ToUpperInvariant();

        var hobbies = MapHobbies(request.HobbyIds);
        currentUser.UpdateHobbies(hobbies);

        var likeCategories = MapPreferenceIds(request.LikePreferenceIds);
        var dislikeCategories = MapPreferenceIds(request.DislikePreferenceIds);

        var sentiments = await _dbContext.Sentiments
            .Where(s =>
                (s.Type == SentimentType.Like && likeCategories.Contains(s.Category)) ||
                (s.Type == SentimentType.Dislike && dislikeCategories.Contains(s.Category)))
            .ToListAsync(cancellationToken);

        var updateSentimentsResult = currentUser.UpdateSentiments(sentiments);
        if (!updateSentimentsResult.IsSuccess)
        {
            return Result.Invalid(
                updateSentimentsResult.Errors
                    .Select(error => new ValidationError(
                        nameof(UserRequest.UpdateCurrentUser.LikePreferenceIds),
                        error))
                    .ToArray());
        }

        var settings = await LoadUserSettingsAsync(currentUser, cancellationToken)
            ?? new ApplicationUserSetting
            {
                FontSize = 12,
                IsDarkMode = false
            };

        currentUser.UserSettings = settings;

        foreach (var existing in settings.ChatTextLineSuggestions.Select(s => s.Text).ToList())
        {
            settings.RemoveChatTextLine(existing);
        }

        var sanitizedChatLines = SanitizeChatLines(request.DefaultChatLines);
        var index = 0;
        foreach (var chatLine in sanitizedChatLines)
        {
            var addResult = settings.AddChatTextLine(chatLine, index++);
            if (!addResult.IsSuccess)
            {
                return Result.Invalid(
                    addResult.Errors
                        .Select(error => new ValidationError(
                            nameof(UserRequest.UpdateCurrentUser.DefaultChatLines),
                            error))
                        .ToArray());
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updatedEmail = identityUser.Email ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, updatedEmail)
        });
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return ("Onbekend", "Onbekend");
        }

        if (parts.Length == 1)
        {
            return (parts[0], parts[0]);
        }

        var firstName = parts[0];
        var lastName = string.Join(" ", parts.Skip(1));
        return (firstName, lastName);
    }

    private static string NormalizeGender(string gender)
    {
        var normalized = gender?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "man" => "man",
            "vrouw" => "vrouw",
            _ => "x"
        };
    }

    private static IEnumerable<UserHobby> MapHobbies(IReadOnlyCollection<string> hobbyIds)
    {
        if (hobbyIds is null || hobbyIds.Count == 0)
        {
            return Array.Empty<UserHobby>();
        }

        return hobbyIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => Enum.TryParse<HobbyType>(id, true, out var hobbyType) ? hobbyType : (HobbyType?)null)
            .Where(type => type.HasValue)
            .Distinct()
            .Take(3)
            .Select(type => new UserHobby { Hobby = type!.Value })
            .ToList();
    }

    private static HashSet<SentimentCategoryType> MapPreferenceIds(IReadOnlyCollection<string> ids)
    {
        var categories = new HashSet<SentimentCategoryType>();
        if (ids is null)
        {
            return categories;
        }

        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            var pascal = string.Join(string.Empty,
                id
                    .Split('-', StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

            if (Enum.TryParse<SentimentCategoryType>(pascal, true, out var category))
            {
                categories.Add(category);
                if (categories.Count >= 5)
                {
                    break;
                }
            }
        }

        return categories;
    }

    private static List<string> SanitizeChatLines(IReadOnlyCollection<string> lines)
    {
        if (lines is null || lines.Count == 0)
        {
            return new List<string>();
        }

        var result = new List<string>(lines.Count);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cleaned = WordFilter.Censor(line.Trim());
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                continue;
            }

            if (!result.Contains(cleaned, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(cleaned);
                if (result.Count >= 5)
                {
                    break;
                }
            }
        }

        return result;
    }

    private async Task<ApplicationUserSetting?> LoadUserSettingsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var settingsEntry = _dbContext.Entry(user).Reference<ApplicationUserSetting>("_userSettings");
        await settingsEntry.LoadAsync(cancellationToken);

        var settings = settingsEntry.CurrentValue;
        if (settings is not null)
        {
            await _dbContext.Entry(settings)
                .Collection(s => s.ChatTextLineSuggestions)
                .LoadAsync(cancellationToken);
            user.UserSettings = settings;
        }

        return settings;
    }
}
