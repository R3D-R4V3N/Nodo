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
    /// <param name="sessionCacheService">Persisted cache for auth payloads.</param>
    /// <param name="offlineQueueService">Connectivity helper.</param>
    public class CookieAuthenticationStateProvider(
        IHttpClientFactory httpClientFactory,
        SessionCacheService sessionCacheService,
        OfflineQueueService offlineQueueService) : AuthenticationStateProvider, IAccountManager
    {
        /// <summary>
        /// Special auth client.
        /// </summary>
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("SecureApi");
        private readonly SessionCacheService _sessionCache = sessionCacheService;
        private readonly OfflineQueueService _offlineQueueService = offlineQueueService;

        /// <summary>
        /// Authentication state.
        /// </summary>
        private bool authenticated = false;

        /// <summary>
        /// Default principal for anonymous (not authenticated) users.
        /// </summary>
        private readonly ClaimsPrincipal unauthenticated = new(new ClaimsIdentity());

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
            AccountResponse.Info? cachedAuth = null;

            try
            {
                cachedAuth = await _sessionCache.GetCachedAuthInfoAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not read cached authentication info.");
            }

            try
            {
                var isOnline = await _offlineQueueService.IsOnlineAsync();
                if (!isOnline && cachedAuth is not null)
                {
                    user = BuildPrincipal(cachedAuth);
                    authenticated = true;
                    return new AuthenticationState(user);
                }

                var result = await httpClient.GetFromJsonAsync<Result<AccountResponse.Info>>("/api/identity/accounts/info");

                if (result?.IsSuccess == true && result.Value is not null)
                {
                    await _sessionCache.CacheAuthInfoAsync(result.Value);
                    user = BuildPrincipal(result.Value);
                    authenticated = true;
                    return new AuthenticationState(user);
                }

                if (cachedAuth is not null)
                {
                    user = BuildPrincipal(cachedAuth);
                    authenticated = true;
                    return new AuthenticationState(user);
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
                if (cachedAuth is not null)
                {
                    user = BuildPrincipal(cachedAuth);
                    authenticated = true;
                    return new AuthenticationState(user);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not GetAuthenticationStateAsync.");
                if (cachedAuth is not null)
                {
                    user = BuildPrincipal(cachedAuth);
                    authenticated = true;
                    return new AuthenticationState(user);
                }
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

        private static ClaimsPrincipal BuildPrincipal(AccountResponse.Info authInfo)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, authInfo.Email),
                new(ClaimTypes.Email, authInfo.Email)
            };

            claims.AddRange(
                authInfo.Claims
                    .Where(c => c.Key is not (ClaimTypes.Name or ClaimTypes.Email or ClaimTypes.Role))
                    .Select(c => new Claim(c.Key, c.Value))
            );

            claims.AddRange(authInfo.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
            return new ClaimsPrincipal(identity);
        }
    }
}
