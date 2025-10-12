namespace Rise.Client.Identity
{
    /// <summary>
    /// Account management services.
    /// </summary>
    public interface IAccountManager
    {
        /// <summary>
        /// Login service.
        /// </summary>
        /// <param name="email">User's email.</param>
        /// <param name="password">User's password.</param>
        /// <returns>The result of the request serialized to <see cref="FormResult"/>.</returns>
        public Task<Result> LoginAsync(string email, string password);

        /// <summary>
        /// Log out the logged in user.
        /// </summary>
        /// <returns>The asynchronous task.</returns>
        public Task LogoutAsync();

        /// <summary>
        /// Registration service.
        /// </summary>
        /// <param name="email">User's email.</param>
        /// <param name="password">User's password.</param>
        /// <param name="confirmPassword">Confirmation of the user's password.</param>
        /// <param name="firstName">User's first name.</param>
        /// <param name="lastName">User's last name.</param>
        /// <param name="organizationId">Identifier of the selected organization.</param>
        /// <returns>The result of the request serialized to <see cref="FormResult"/>.</returns>
        public Task<Result> RegisterAsync(string email, string password, string confirmPassword, string firstName, string lastName, int organizationId);

        public Task<bool> CheckAuthenticatedAsync();
    }
}
