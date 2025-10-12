using Destructurama.Attributed;

namespace Rise.Shared.Identity.Accounts;

/// <summary>
/// Represents a request structure for account-related operations, such as registration or authentication.
/// </summary>
public static partial class AccountRequest
{
    public class Register
    {
        /// <summary>
        /// The user's email address which acts as a user name.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// The user's password.
        /// </summary>
        [LogMasked]
        public string? Password { get; set; }

        /// <summary>
        /// The user's password.
        /// </summary>
        [LogMasked]
        public string? ConfirmPassword { get; set; }

        /// <summary>
        /// The user's first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// The identifier of the organization the user belongs to.
        /// </summary>
        public int OrganizationId { get; set; }

        // Other needed stuff here, like Role(s), Firstname, lastname etc.

        /// <summary>
        /// Provides validation rules for the Register class fields such as email and password.
        /// </summary>
        public class Validator : AbstractValidator<Register>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).NotEmpty();
                RuleFor(x => x.ConfirmPassword)
                    .Equal(x => x.Password)
                    .WithMessage("Passwords do not match.");
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .WithMessage("Gelieve je volledige naam in te geven.");
                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .WithMessage("Gelieve je volledige naam in te geven.");
                RuleFor(x => x.OrganizationId)
                    .GreaterThan(0)
                    .WithMessage("Kies een organisatie om je account te koppelen.");
            }
        }
    }
}