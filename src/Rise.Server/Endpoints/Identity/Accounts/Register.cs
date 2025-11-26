using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Registrations;
using Rise.Persistence;
using Rise.Shared.Assets;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.RegistrationRequests;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
/// Register a new user,
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="userManager"></param>
/// <param name="dbContext"></param>
/// <param name="passwordHasher"></param>
public class Register(
    IRegistrationRequestService registrationService
    ) : Endpoint<AccountRequest.Register, Result>
{
    public override void Configure()
    {
        Post("/api/identity/accounts/register");
        AllowAnonymous(); // Open for all at the moment, but you can restrict it to only admins.
                          // Roles(AppRoles.Administrator);
    }

    public override async Task<Result> ExecuteAsync(AccountRequest.Register req, CancellationToken ctx)
    {
        return await registrationService.CreateAsync(req);
    }
}
