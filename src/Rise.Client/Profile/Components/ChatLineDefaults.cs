using System.Collections.Generic;
using Rise.Client.Profile.Models;

namespace Rise.Client.Profile.Components;

internal static class ChatLineDefaults
{
    public static readonly IReadOnlyList<PreferenceOption> Options = new List<PreferenceOption>
    {
        new("greeting-how-are-you", "Hoi! Hoe gaat het met je vandaag?"),
        new("weekend-plans", "Heb je leuke plannen voor dit weekend?"),
        new("weather-check", "Wat vind je van het weer vandaag?"),
        new("favorite-hobby", "Wat doe je het liefst in je vrije tijd?"),
        new("share-highlight", "Wat was het leukste dat je deze week meemaakte?"),
        new("coffee-invite", "Zin om binnenkort samen koffie te drinken?"),
        new("movie-talk", "Heb je onlangs nog een leuke film gezien?"),
        new("music-question", "Welke muziek luister je graag?"),
        new("book-recommendation", "Heb je nog een boekentip voor mij?"),
        new("food-question", "Wat eet jij het liefst als comfort food?"),
        new("walk-invite", "Zullen we binnenkort eens een wandeling maken?"),
        new("fun-fact", "Ik wil graag een leuk weetje horen over jou!"),
        new("gratitude", "Waar ben jij vandaag dankbaar voor?"),
        new("motivation", "Wat geeft jou energie op een drukke dag?"),
        new("relax-tip", "Hoe ontspan jij het liefst na een lange dag?"),
        new("game-question", "Speel je graag spelletjes?"),
        new("sport-chat", "Welke sport kijk of doe jij het liefst?"),
        new("travel-dream", "Welke plek wil je ooit nog bezoeken?"),
        new("memory-share", "Vertel eens over een mooie herinnering."),
        new("goal-question", "Waar kijk je deze maand het meest naar uit?"),
        new("support-offer", "Laat het me weten als ik iets voor je kan doen!"),
        new("photo-share", "Ik ben benieuwd naar je laatste foto, wil je die delen?"),
        new("daily-check-in", "Wat houdt je vandaag bezig?"),
        new("morning-message", "Goedemorgen! Heb je lekker geslapen?"),
        new("evening-message", "Slaap zacht straks, wat ga je nog doen vanavond?"),
        new("compliment", "Ik waardeer het echt om met jou te praten!"),
        new("laugh-question", "Waar heb je laatst hard om gelachen?"),
        new("learning", "Wat wil je graag nog leren?"),
        new("pet-talk", "Heb je huisdieren? Vertel eens!"),
        new("recipe-share", "Heb je een favoriet recept dat ik moet proberen?"),
    };
}
