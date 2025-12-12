using Destructurama.Attributed;
using Rise.Shared.Validators;

namespace Rise.Shared.Identity.Accounts;

public static partial class AccountRequest
{
    public class Login
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
        /// The optional two-factor authenticator code. This may be required for users who have enabled two-factor authentication.
        /// This is not required if a <see cref="TwoFactorRecoveryCode"/> is sent.
        /// </summary>
        public string? TwoFactorCode { get; set; }

        /// <summary>
        /// An optional two-factor recovery code from <see cref="TwoFactorResponse.RecoveryCodes"/>.
        /// This is required for users who have enabled two-factor authentication but lost access to their <see cref="TwoFactorCode"/>.
        /// </summary>
        public string? TwoFactorRecoveryCode { get; set; }

        /// <summary>
        /// Provides validation rules for the <see cref="Login"/> class.
        /// The validator ensures that required properties are present and conform to specific formats.
        /// </summary>
        public class Validator : AbstractValidator<Login>
        {
            public Validator(ValidatorRules rules)
            {
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithMessage("Email mag niet leeg zijn")
                    .EmailAddress()
                    .WithMessage("Email is niet in een geldig formaat")
                    .MaximumLength(rules.MAX_EMAIL_LENGTH)
                    .WithMessage($"Email heeft max {rules.MAX_EMAIL_LENGTH} karakters");
                RuleFor(x => x.Password)
                    .NotEmpty()
                    .WithMessage("Wachtwoord mag niet leeg zijn");
            }
        }
    }
}