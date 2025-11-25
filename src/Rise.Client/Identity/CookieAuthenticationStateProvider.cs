using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Client.Offline;
using Rise.Shared.Identity.Accounts;

namespace Rise.Client.Identity
{
    /// <summary>
    /// Handles state for cookie-based auth.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the auth provider.
    /// </remarks>
    /// <param name="httpClientFactory">Factory to retrieve auth client.</param>
    public class CookieAuthenticationStateProvider(IHttpClientFactory httpClientFactory, CacheStoreService cacheStoreService, OfflineQueueService offlineQueueService): AuthenticationStateProvider, IAccountManager
    {
        /// <summary>
        /// Special auth client.
        /// </summary>
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("SecureApi");
        private readonly CacheStoreService _cacheStoreService = cacheStoreService;
        private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
        
        /// <summary>
        /// Authentication state.
        /// </summary>
        private bool authenticated = false;

        private readonly ClaimsPrincipal unauthenticated = new(new ClaimsIdentity());
        private static readonly TimeSpan SessionCacheTtl = TimeSpan.FromHours(12);

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <param name="request">The registration request.</param>
        /// <returns>The result serialized to a <see cref="Result"/>.
        /// </returns>
        public async Task<Result> RegisterAsync(AccountRequest.Register request)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/identity/accounts/register", request);

                var result = await response.Content.ReadFromJsonAsync<Result>();

                if (result is not null)
                {
                    return result;
                }

                return Result.Error("Kon het serverantwoord niet verwerken.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not register user.");
                return Result.Error("An unknown error prevented registration from succeeding.");
            }
         }

        /// <summary>
        /// User login.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The result of the login request serialized to a <see cref="FormResult"/>.</returns>
        public async Task<Result> LoginAsync(string email, string password)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/identity/accounts/login", new AccountRequest.Login
                {
                    Email = email,
                    Password = password,
                });
            
                var result = await response.Content.ReadFromJsonAsync<Result>();
                if (response.IsSuccessStatusCode)
                {
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                }

                return result!;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not login user.");
            }

            return Result.Error("Invalid email and/or password.");
        }

        /// <summary>
        /// Get authentication state.
        /// </summary>
        /// <remarks>
        /// Called by Blazor anytime and authentication-based decision needs to be made, then cached
        /// until the changed state notification is raised.
        /// </remarks>
        /// <returns>The authentication state asynchronous request.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            authenticated = false;
            // default to not authenticated
            var user = unauthenticated;

            var cachedSession = await _cacheStoreService.GetAuthSessionAsync(SessionCacheTtl);
            var isOnline = await _offlineQueueService.IsOnlineAsync();

            try
            {
                var result = await httpClient.GetFromJsonAsync<Result<AccountResponse.Info>>("/api/identity/accounts/info");

                if (result!.IsSuccess)
                {
                    user = BuildPrincipal(result.Value);
                    authenticated = true;

                    await _cacheStoreService.UpsertAuthSessionAsync(new CachedSession
                    {
                        AccountInfo = result.Value,
                        AccountId = cachedSession?.AccountId,
                        User = cachedSession?.User
                    });
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // noop: laat user = unauthenticated
            }
            // andere HTTP-fouten wél loggen
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Could not GetAuthenticationStateAsync.");
                if (!isOnline && cachedSession?.AccountInfo is not null)
                {
                    user = BuildPrincipal(cachedSession.AccountInfo);
                    authenticated = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not GetAuthenticationStateAsync.");
            }

            if (!authenticated && cachedSession?.AccountInfo is not null)
            {
                user = BuildPrincipal(cachedSession.AccountInfo);
                authenticated = true;
            }

            return new AuthenticationState(user);
        }

        public async Task LogoutAsync()
        {
            await httpClient.PostAsync("/api/identity/accounts/logout", null);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<bool> CheckAuthenticatedAsync()
        {
            await GetAuthenticationStateAsync();
            return authenticated;
        }

        private static ClaimsPrincipal BuildPrincipal(AccountResponse.Info info)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, info.Email),
                new(ClaimTypes.Email, info.Email)
            };

            claims.AddRange(
                info.Claims
                    .Where(c => c.Key is not (ClaimTypes.Name or ClaimTypes.Email or ClaimTypes.Role))
                    .Select(c => new Claim(c.Key, c.Value))
            );

            claims.AddRange(info.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
            return new ClaimsPrincipal(identity);
        }
    }
}
