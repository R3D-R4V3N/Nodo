using System;
using Destructurama.Attributed;
using Rise.Shared.Users;
using Rise.Shared.Validators;

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
        public string? FirstName { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// The user's birth date.
        /// </summary>
        public DateOnly? BirthDate { get; set; }

        /// <summary>
        /// The user's gender.
        /// </summary>
        public GenderTypeDto Gender { get; set; } = GenderTypeDto.X;

        /// <summary>
        /// The user's profile photo in data URL format.
        /// </summary>
        public string? AvatarDataUrl { get; set; }

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
        /// Identifier of the selected organization.
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// Provides validation rules for the Register class fields such as email and password.
        /// </summary>
        public class Validator : AbstractValidator<Register>
        {
            public Validator(ValidatorRules rules)
            {
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .EmailAddress()
                    .MaximumLength(rules.MAX_EMAIL_LENGTH);
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .MaximumLength(rules.MAX_FIRSTNAME_LENGTH);
                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .MaximumLength(rules.MAX_LASTNAME_LENGTH);
                RuleFor(x => x.BirthDate)
                    .NotNull()
                    .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                    .WithMessage("Geboortedatum mag niet in de toekomst liggen.");
                RuleFor(x => x.Gender)
                    .IsInEnum();
                RuleFor(x => x.AvatarDataUrl)
                    .NotEmpty()
                    .MaximumLength(rules.MAX_AVATAR_URL_LENGTH);
                RuleFor(x => x.Password).NotEmpty();
                RuleFor(x => x.ConfirmPassword)
                    .Equal(x => x.Password)
                    .WithMessage("Passwords do not match.");
                RuleFor(x => x.OrganizationId)
                    .NotNull()
                    .GreaterThan(0)
                    .WithMessage("Kies een organisatie.");
            }
        }
    }
}
