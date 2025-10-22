using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Assets;
using Rise.Shared.Identity;
using Rise.Shared.Profile;

namespace Rise.Services.Profile;

public class ProfileService(
    ApplicationDbContext dbContext,
    UserManager<IdentityUser> userManager,
    ISessionContextProvider sessionContextProvider) : IProfileService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;

    public async Task<Result<ProfileResponse.Envelope>> GetAsync(CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var profile = await _dbContext.ApplicationUsers
            .Include(u => u.Interests)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (profile is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var identity = await _userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);

        if (identity is null)
        {
            return Result.Unauthorized("Kon het Identity-account niet ophalen.");
        }

        return Result.Success(CreateEnvelope(profile, identity.Email ?? string.Empty));
    }

    public async Task<Result<ProfileResponse.Envelope>> UpdateAsync(ProfileRequest.UpdateProfile request, CancellationToken cancellationToken = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var profile = await _dbContext.ApplicationUsers
            .Include(u => u.Interests)
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);

        if (profile is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var identity = await _userManager.Users
            .SingleOrDefaultAsync(u => u.Id == accountId, cancellationToken);

        if (identity is null)
        {
            return Result.Unauthorized("Kon het Identity-account niet ophalen.");
        }

        var sanitizedFirstName = request.FirstName?.Trim();
        if (string.IsNullOrWhiteSpace(sanitizedFirstName))
        {
            return Result.Invalid(new ValidationError(nameof(request.FirstName), "Voornaam is verplicht."));
        }

        var sanitizedLastName = request.LastName?.Trim();
        if (string.IsNullOrWhiteSpace(sanitizedLastName))
        {
            return Result.Invalid(new ValidationError(nameof(request.LastName), "Achternaam is verplicht."));
        }

        var biography = request.Biography?.Trim();
        if (string.IsNullOrWhiteSpace(biography))
        {
            return Result.Invalid(new ValidationError(nameof(request.Biography), "Bio mag niet leeg zijn."));
        }

        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Invalid(new ValidationError(nameof(request.Email), "E-mail is verplicht."));
        }

        var gender = request.Gender?.Trim().ToLowerInvariant() ?? UserInterestConstants.DefaultGender;
        if (!UserInterestConstants.AllowedGenders.Contains(gender))
        {
            return Result.Invalid(new ValidationError(nameof(request.Gender), "Ongeldige genderwaarde."));
        }

        var avatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
            ? profile.AvatarUrl
            : request.AvatarUrl.Trim();

        var requestedInterests = (request.Interests ?? Array.Empty<string>())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (requestedInterests.Count > ProfileCatalog.MaxInterestCount)
        {
            return Result.Invalid(new ValidationError(nameof(request.Interests), $"Je kan maximaal {ProfileCatalog.MaxInterestCount} interesses selecteren."));
        }

        if (requestedInterests.Any(id => !ProfileCatalog.IsValidInterest(id)))
        {
            return Result.Invalid(new ValidationError(nameof(request.Interests), "Bevat ongeldige interesses."));
        }

        profile.FirstName = sanitizedFirstName;
        profile.LastName = sanitizedLastName;
        profile.Biography = biography;
        profile.Gender = gender;
        profile.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl)
            ? (string.IsNullOrWhiteSpace(profile.AvatarUrl) ? DefaultImages.Profile : profile.AvatarUrl)
            : avatarUrl;
        profile.SetInterests(requestedInterests);

        identity.Email = email;
        identity.NormalizedEmail = email.ToUpperInvariant();
        identity.UserName = email;
        identity.NormalizedUserName = email.ToUpperInvariant();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(CreateEnvelope(profile, identity.Email ?? email));
    }

    private static ProfileResponse.Envelope CreateEnvelope(ApplicationUser profile, string email)
        => new()
        {
            Profile = new ProfileResponse.Profile
            {
                Id = profile.Id,
                AccountId = profile.AccountId,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Email = email,
                Biography = profile.Biography,
                Gender = profile.Gender,
                AvatarUrl = profile.AvatarUrl,
                MemberSince = profile.CreatedAt,
                Interests = profile.Interests.Select(i => i.InterestId).ToList()
            },
            AvailableInterests = ProfileCatalog.Interests,
            MaxInterestCount = ProfileCatalog.MaxInterestCount
        };
}
