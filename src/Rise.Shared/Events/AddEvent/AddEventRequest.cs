using FluentValidation;

namespace Rise.Shared.Events;

public static partial class EventRequest
{

public class AddEventRequest
{
    public String Name { get; set; }
    public DateTime Date { get; set; }
    public String Location { get; set; }
    public Double Price { get; set; }
    public String ImageUrl { get; set; }

    public class Validator : AbstractValidator<AddEventRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Naam is verplicht.");

            RuleFor(x => x.Location)
                .NotEmpty()
                .WithMessage("Locatie is verplicht.");

            RuleFor(x => x.Date)
                .GreaterThan(DateTime.Now)
                .WithMessage("Datum moet in de toekomst liggen.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Prijs kan niet negatief zijn.");

            RuleFor(x => x.ImageUrl)
                .Must(url => !string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage("Voeg een geldige URL toe of laat dit veld leeg.");
        }
    }
}
}
