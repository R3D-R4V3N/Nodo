using FluentValidation;
using Rise.Shared.Validators;

namespace Rise.Shared.Users;

public static partial class UserRequest
{
    public class UpdatePersonalInfo
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public GenderTypeDto Gender { get; set; }
    }

    public class UpdatePersonalInfoValidator : AbstractValidator<UpdatePersonalInfo>
    {
        public UpdatePersonalInfoValidator(ValidatorRules rules)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(rules.MAX_FIRSTNAME_LENGTH)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Voornaam mag niet leeg zijn.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(rules.MAX_LASTNAME_LENGTH)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Achternaam mag niet leeg zijn.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(rules.MAX_EMAIL_LENGTH);

            RuleFor(x => x.Biography)
                .NotEmpty()
                .MaximumLength(rules.MAX_BIOGRAPHY_LENGTH)
                .Must(bio => !string.IsNullOrWhiteSpace(bio))
                .WithMessage("Bio mag niet leeg zijn.");

            RuleFor(x => x.AvatarUrl)
                .NotEmpty()
                .MaximumLength(rules.MAX_AVATAR_URL_LENGTH)
                .Must(url => !string.IsNullOrWhiteSpace(url))
                .WithMessage("Avatar mag niet leeg zijn.");
        }
    }
}
