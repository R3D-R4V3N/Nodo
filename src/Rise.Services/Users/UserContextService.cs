using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.Users.Mapper;
using Rise.Shared.Identity;
using Rise.Shared.Users;

namespace Rise.Services.Users;
public class UserContextService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IUserContextService
{

    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;

    private const int PreferenceSelectionLimit = 5;
    private const int HobbySelectionLimit = 3;
    private const int ChatLineSelectionLimit = 5;

    private static readonly IReadOnlyDictionary<string, SentimentCategoryType> PreferenceCategoryById =
        new Dictionary<string, SentimentCategoryType>(StringComparer.OrdinalIgnoreCase)
        {
            ["travel-adventures"] = SentimentCategoryType.TravelAdventures,
            ["city-trips"] = SentimentCategoryType.CityTrips,
            ["beach-days"] = SentimentCategoryType.BeachDays,
            ["mountain-views"] = SentimentCategoryType.MountainViews,
            ["shopping-sprees"] = SentimentCategoryType.ShoppingSprees,
            ["market-visits"] = SentimentCategoryType.MarketVisits,
            ["cozy-cafes"] = SentimentCategoryType.CozyCafes,
            ["dining-out"] = SentimentCategoryType.DiningOut,
            ["street-food"] = SentimentCategoryType.StreetFood,
            ["new-flavours"] = SentimentCategoryType.NewFlavours,
            ["sweet-treats"] = SentimentCategoryType.SweetTreats,
            ["savoury-snacks"] = SentimentCategoryType.SavourySnacks,
            ["spicy-dishes"] = SentimentCategoryType.SpicyDishes,
            ["fresh-salads"] = SentimentCategoryType.FreshSalads,
            ["seasonal-soups"] = SentimentCategoryType.SeasonalSoups,
            ["fruity-moments"] = SentimentCategoryType.FruityMoments,
            ["chocolate-moments"] = SentimentCategoryType.ChocolateMoments,
            ["cheese-boards"] = SentimentCategoryType.CheeseBoards,
            ["coffee-breaks"] = SentimentCategoryType.CoffeeBreaks,
            ["tea-time"] = SentimentCategoryType.TeaTime,
            ["smoothie-bar"] = SentimentCategoryType.SmoothieBar,
            ["juice-stands"] = SentimentCategoryType.JuiceStands,
            ["breakfast-dates"] = SentimentCategoryType.BreakfastDates,
            ["brunch-plans"] = SentimentCategoryType.BrunchPlans,
            ["picnic-plans"] = SentimentCategoryType.PicnicPlans,
            ["food-trucks"] = SentimentCategoryType.FoodTrucks,
            ["farmers-markets"] = SentimentCategoryType.FarmersMarkets,
            ["road-trips"] = SentimentCategoryType.RoadTrips,
            ["train-journeys"] = SentimentCategoryType.TrainJourneys,
            ["ferry-rides"] = SentimentCategoryType.FerryRides,
            ["wellness-days"] = SentimentCategoryType.WellnessDays,
            ["spa-relax"] = SentimentCategoryType.SpaRelax,
            ["sauna-evenings"] = SentimentCategoryType.SaunaEvenings,
            ["cinema-nights"] = SentimentCategoryType.CinemaNights,
            ["series-marathons"] = SentimentCategoryType.SeriesMarathons,
            ["romantic-movies"] = SentimentCategoryType.RomanticMovies,
            ["action-movies"] = SentimentCategoryType.ActionMovies,
            ["horror-movies"] = SentimentCategoryType.HorrorMovies,
            ["documentaries"] = SentimentCategoryType.Documentaries,
            ["podcasts"] = SentimentCategoryType.Podcasts,
            ["radio-hits"] = SentimentCategoryType.RadioHits,
            ["live-concerts"] = SentimentCategoryType.LiveConcerts,
            ["music-festivals"] = SentimentCategoryType.MusicFestivals,
            ["dance-parties"] = SentimentCategoryType.DanceParties,
            ["quiet-evenings"] = SentimentCategoryType.QuietEvenings,
            ["candlelight-dinners"] = SentimentCategoryType.CandlelightDinners,
            ["sunset-watching"] = SentimentCategoryType.SunsetWatching,
            ["rainy-days"] = SentimentCategoryType.RainyDays,
            ["snowy-days"] = SentimentCategoryType.SnowyDays,
            ["amusement-parks"] = SentimentCategoryType.AmusementParks,
        };

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
            .Include(u => u.UserSettings)
                .ThenInclude(s => s.ChatTextLineSuggestions)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var email = (await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken)
            )?.Email
            ?? string.Empty;

        return Result.Success(new UserResponse.CurrentUser
        {
            User = UserMapper.ToCurrentUserDto(currentUser, email)
        });
    }

    public async Task<Result<UserResponse.CurrentUser>> UpdateProfileAsync(UserRequest.UpdateProfile request, CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentUser = await _dbContext.ApplicationUsers
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .Include(u => u.UserSettings)
                .ThenInclude(s => s.ChatTextLineSuggestions)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var identityUser = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);

        if (identityUser is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var trimmedName = request.Name.Trim();
        var (firstName, lastName) = SplitName(trimmedName);
        currentUser.FirstName = firstName;
        currentUser.LastName = lastName;

        currentUser.Biography = request.Bio.Trim();
        currentUser.AvatarUrl = request.AvatarUrl.Trim();

        var trimmedEmail = request.Email.Trim();
        var normalizedEmail = trimmedEmail.ToUpperInvariant();
        identityUser.Email = trimmedEmail;
        identityUser.NormalizedEmail = normalizedEmail;
        identityUser.UserName = trimmedEmail;
        identityUser.NormalizedUserName = normalizedEmail;

        var hobbiesResult = BuildHobbies(request.Hobbies);
        if (!hobbiesResult.IsSuccess)
        {
            return MapFailure(hobbiesResult);
        }

        currentUser.UpdateHobbies(hobbiesResult.Value);

        var sentimentsResult = await BuildSentimentsAsync(request, cancellationToken);
        if (!sentimentsResult.IsSuccess)
        {
            return MapFailure(sentimentsResult);
        }

        var updateSentimentsResult = currentUser.UpdateSentiments(sentimentsResult.Value);
        if (!updateSentimentsResult.IsSuccess)
        {
            return MapFailure(updateSentimentsResult);
        }

        var chatLineResult = UpdateChatLines(currentUser, request.DefaultChatLines);
        if (!chatLineResult.IsSuccess)
        {
            return MapFailure(chatLineResult);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updatedUser = UserMapper.ToCurrentUserDto(currentUser, identityUser.Email ?? string.Empty);

        return Result.Success(new UserResponse.CurrentUser
        {
            User = updatedUser
        });
    }

    private static (string FirstName, string LastName) SplitName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (string.Empty, string.Empty);
        }

        var trimmed = name.Trim();
        var indexOfSpace = trimmed.IndexOf(' ', StringComparison.Ordinal);
        if (indexOfSpace < 0)
        {
            return (trimmed, trimmed);
        }

        var first = trimmed[..indexOfSpace].Trim();
        var last = trimmed[(indexOfSpace + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(last))
        {
            last = first;
        }

        return (first, last);
    }

    private Result<List<UserHobby>> BuildHobbies(IEnumerable<string> hobbyIds)
    {
        var hobbies = new List<UserHobby>();
        var uniqueIds = hobbyIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(HobbySelectionLimit);

        foreach (var hobbyId in uniqueIds)
        {
            if (!Enum.TryParse<HobbyType>(hobbyId, true, out var hobbyType))
            {
                return Result.Invalid(new ValidationError(nameof(UserRequest.UpdateProfile.Hobbies), $"Onbekende hobby '{hobbyId}'."));
            }

            hobbies.Add(new UserHobby { Hobby = hobbyType });
        }

        return Result.Success(hobbies);
    }

    private async Task<Result<List<UserSentiment>>> BuildSentimentsAsync(UserRequest.UpdateProfile request, CancellationToken cancellationToken)
    {
        var likeCategoriesResult = ResolvePreferenceCategories(request.Likes, nameof(request.Likes));
        if (!likeCategoriesResult.IsSuccess)
        {
            return Result.Invalid(likeCategoriesResult.ValidationErrors.ToArray());
        }

        var dislikeCategoriesResult = ResolvePreferenceCategories(request.Dislikes, nameof(request.Dislikes));
        if (!dislikeCategoriesResult.IsSuccess)
        {
            return Result.Invalid(dislikeCategoriesResult.ValidationErrors.ToArray());
        }

        var likeCategories = likeCategoriesResult.Value;
        var dislikeCategories = dislikeCategoriesResult.Value;

        if (likeCategories.Any(dislikeCategories.Contains))
        {
            return Result.Conflict("Een voorkeur kan niet tegelijkertijd leuk en niet leuk zijn.");
        }

        if (likeCategories.Count == 0 && dislikeCategories.Count == 0)
        {
            return Result.Success(new List<UserSentiment>());
        }

        var sentiments = await _dbContext.Sentiments
            .Where(s =>
                (s.Type == SentimentType.Like && likeCategories.Contains(s.Category)) ||
                (s.Type == SentimentType.Dislike && dislikeCategories.Contains(s.Category)))
            .ToListAsync(cancellationToken);

        if (sentiments.Count != likeCategories.Count + dislikeCategories.Count)
        {
            return Result.Error("Kon niet alle voorkeuren laden.");
        }

        return Result.Success(sentiments);
    }

    private Result<List<SentimentCategoryType>> ResolvePreferenceCategories(IEnumerable<string> ids, string fieldName)
    {
        var categories = new List<SentimentCategoryType>();
        var uniqueIds = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(PreferenceSelectionLimit);

        foreach (var id in uniqueIds)
        {
            if (!PreferenceCategoryById.TryGetValue(id, out var category))
            {
                return Result.Invalid(new ValidationError(fieldName, $"Onbekende voorkeur '{id}'."));
            }

            categories.Add(category);
        }

        return Result.Success(categories);
    }

    private Result UpdateChatLines(ApplicationUser user, IEnumerable<string> chatLines)
    {
        if (user.UserSettings is null)
        {
            return Result.Error("Gebruikersinstellingen konden niet geladen worden.");
        }

        var normalizedLines = chatLines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(ChatLineSelectionLimit)
            .ToList();

        var existingLines = user.UserSettings.ChatTextLineSuggestions
            .Select(s => s.Text)
            .ToList();

        foreach (var existing in existingLines)
        {
            var removeResult = user.UserSettings.RemoveChatTextLine(existing);
            if (!removeResult.IsSuccess)
            {
                return removeResult;
            }
        }

        for (var index = 0; index < normalizedLines.Count; index++)
        {
            var addResult = user.UserSettings.AddChatTextLine(normalizedLines[index], index);
            if (!addResult.IsSuccess)
            {
                return addResult;
            }
        }

        return Result.Success();
    }

    private static Result<UserResponse.CurrentUser> MapFailure(Result result)
    {
        return result.Status switch
        {
            ResultStatus.Invalid => Result.Invalid(result.ValidationErrors?.ToArray() ?? Array.Empty<ValidationError>()),
            ResultStatus.Conflict => Result.Conflict(result.Errors?.ToArray() ?? Array.Empty<string>()),
            ResultStatus.Unauthorized => Result.Unauthorized(result.Errors?.FirstOrDefault()),
            _ => Result.Error(result.Errors?.ToArray() ?? Array.Empty<string>())
        };
    }

    private static Result<UserResponse.CurrentUser> MapFailure<T>(Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Invalid => Result.Invalid(result.ValidationErrors?.ToArray() ?? Array.Empty<ValidationError>()),
            ResultStatus.Conflict => Result.Conflict(result.Errors?.ToArray() ?? Array.Empty<string>()),
            ResultStatus.Unauthorized => Result.Unauthorized(result.Errors?.FirstOrDefault()),
            _ => Result.Error(result.Errors?.ToArray() ?? Array.Empty<string>())
        };
    }
}
