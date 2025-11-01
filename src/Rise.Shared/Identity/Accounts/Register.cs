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
        /// The user's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The user's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the organization the user belongs to.
        /// </summary>
        public int? OrganizationId { get; set; }

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
        
        // Other needed stuff here, like Role(s), Firstname, lastname etc.

        /// <summary>
        /// Provides validation rules for the Register class fields such as email and password.
        /// </summary>
        public class Validator : AbstractValidator<Register>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .MaximumLength(100);
                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .MaximumLength(100);
                RuleFor(x => x.OrganizationId)
                    .NotNull()
                    .GreaterThan(0);
                RuleFor(x => x.Password).NotEmpty();
                RuleFor(x => x.ConfirmPassword)
                    .Equal(x => x.Password)
                    .WithMessage("Passwords do not match.");
            }
        }
    }
}