using System;
using Destructurama.Attributed;
using FluentValidation;
using Rise.Shared.BlobStorage;
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
        public BlobDto.Create AvatarBlob { get; set; }

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
                    .WithMessage("Email mag niet leeg zijn")
                    .EmailAddress()
                    .WithMessage("Email moet een geldig formaat zijn")
                    .MaximumLength(rules.MAX_EMAIL_LENGTH)
                    .WithMessage($"Email heeft max {rules.MAX_EMAIL_LENGTH} karakters");
                RuleFor(x => x.FirstName)
                    .NotEmpty()
                    .WithMessage("Voornaam mag niet leeg zijn")
                    .MaximumLength(rules.MAX_FIRSTNAME_LENGTH)
                    .WithMessage($"Voornaam heeft max {rules.MAX_FIRSTNAME_LENGTH} karakters");
                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .WithMessage("Achternaam mag niet leeg zijn")
                    .MaximumLength(rules.MAX_LASTNAME_LENGTH)
                    .WithMessage($"Achternaam heeft max {rules.MAX_LASTNAME_LENGTH} karakters");
                RuleFor(x => x.BirthDate)
                    .NotNull()
                    .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                    .WithMessage("Geboortedatum mag niet in de toekomst liggen.");
                RuleFor(x => x.Gender)
                    .IsInEnum();
                RuleFor(x => x.AvatarBlob)
                    .NotNull()
                    .WithMessage("Avatar mag niet leeg zijn.");
                RuleFor(x => x.Password)
                    .NotEmpty()
                    .WithMessage("Wachtwoord mag niet leeg zijn.");
                RuleFor(x => x.ConfirmPassword)
                    .Equal(x => x.Password)
                    .WithMessage("Wachtwoorden komen niet overeen.");
                RuleFor(x => x.OrganizationId)
                    .NotNull()
                    .GreaterThan(0)
                    .WithMessage("Kies een organisatie.");
            }
        }
    }
}
