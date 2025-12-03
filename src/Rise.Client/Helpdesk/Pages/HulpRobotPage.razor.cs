namespace Rise.Client.Helpdesk.Pages;

public partial class HulpRobotPage
{
    private readonly IEnumerable<string> HelpdeskSuggestions = new[]
    {
        "Ik voel me niet zo goed bij dit gesprek, wat kan ik doen?",
        "Hoe kan ik een gesprek beginnen in Nodo?",
        "Wat kan ik doen als ik me overweldigd voel?",
        "Kan ik mijn stemming ergens bijhouden?",
        "Hoe vind ik iemand om mee te praten?",
        "Hoe kan ik een noodsignaal versturen?",
        "Kan ik mijn gesprekken teruglezen?",
        "Hoe pas ik mijn profiel of voorkeuren aan?",
        "Kan ik iemand verwijderen als een gesprek niet fijn voelt?"
    };

    private readonly List<(string[] Keywords, string Response)> SmartResponses =
        new()
        {
            (new[] { "niet goed", "slecht", "gesprek", "ongemakkelijk", "overweldigd", "te veel", "paniek" },
                "Als een gesprek niet fijn voelt, is dat helemaal oké. Je hoeft niet verder te praten. In de chat staat een grote noodknop." +
                " Druk daarop. Dan stopt het gesprek meteen en kun je even tot rust komen."),

            (new[] { "gesprek", "beginnen", "starten" },
                "Een nieuw gesprek starten is makkelijk. Klik hierboven op het pijltje terug en klik dan op het vrienden icoontje." +
                " Daar zie je al je vrienden. Druk op het knopje ‘bericht’ naast de persoon met wie je wilt praten. Dan opent er automatisch een chat"),

            (new[] { "stemming", "gevoel", "profiel", "bijhouden" },
                "Klik op je profielfoto → bewerken om je stemming bij te houden."),

            (new[] { "vriend", "praten", "iemand vinden" },
                "Klik onderaan op het vrienden-icoontje en voeg vrienden toe."),

            (new[] { "nood", "alarm", "signaal" },
                "Je kan in de chat op de noodknop drukken."),

            (new[] { "gesprek", "chat", "teruglezen", "scroll" },
                "Scroll omhoog in de chat om oude berichten te zien."),

            (new[] { "profiel", "voorkeuren", "aanpassen" },
                "Onderin rechts staat je profielfoto. Tik daarop. Daarna klik je op ‘bewerken’. Hier kun je jouw naam," +
                " interesses, stemming en andere voorkeuren aanpassen. Zo blijft je profiel helemaal van jou"),

            (new[] { "verwijderen", "ongewenst", "niet fijn", "blok" },
                "Ga naar vriendenpagina en klik hier op de verwijderknop bij de persoon die irriteert.")
        };

    private string GetBotAnswer(string text)
    {
        text = text.ToLower();

        string bestResponse = "Ik begrijp je vraag niet helemaal. Gebruik een van de suggesties hierboven!";
        int maxMatches = 0;

        foreach (var (keywords, response) in SmartResponses)
        {
            int matches = keywords.Count(k => text.Contains(k.ToLower()));

            if (matches > maxMatches)
            {
                maxMatches = matches;
                bestResponse = response;
            }
        }

        return bestResponse;
    }

    private List<BotChatMessage> Messages = new();

    private async Task HandleSuggestion(string text)
    {
        AddUserMessage(text);
        await Task.Delay(300);
        AddBotMessage(GetBotAnswer(text));
    }

    private async Task HandleUserInput(string text)
    {
        AddUserMessage(text);
        await Task.Delay(300);
        AddBotMessage(GetBotAnswer(text));
    }

    private void AddUserMessage(string text)
    {
        Messages.Add(new BotChatMessage
        {
            Content = text,
            IsUser = true,
            AvatarUrl = "/images/default-user.png"
        });
    }

    private void AddBotMessage(string text)
    {
        Messages.Add(new BotChatMessage
        {
            Content = text,
            IsUser = false,
            AvatarUrl = "https://images.unsplash.com/photo-1659018966820-de07c94e0d01?q=80"
        });
    }

    public class BotChatMessage
    {
        public string Content { get; set; } = ""; // De tekst van het bericht
        public bool IsUser { get; set; } = false; // Is dit een bericht van de gebruiker of de bot?
        public string AvatarUrl { get; set; } = ""; // Avatar van de gebruiker of bot
    }
}