using Rise.Domain.Users.Sentiment;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;

internal static class SentimentMapper
{
    public static UserSentimentDto ToDto(UserSentiment interest)
    {
        var (DisplayName, Emoji) = TranslateCategoryType(interest.Category);

        return new UserSentimentDto()
        {
            Type = interest.Type.MapToDto(),
            Text = DisplayName,
            Emoji = Emoji
        };
    }

    private static (string DisplayName, string Emoji) TranslateCategoryType(SentimentCategoryType category) =>
        category switch
        {
            SentimentCategoryType.TravelAdventures => ("Reizen", "✈️"),
            SentimentCategoryType.CityTrips => ("Stedentrips", "🏙️"),
            SentimentCategoryType.BeachDays => ("Stranddagen", "🏖️"),
            SentimentCategoryType.MountainViews => ("Bergen bewonderen", "🏔️"),
            SentimentCategoryType.ShoppingSprees => ("Shoppen", "🛍️"),
            SentimentCategoryType.MarketVisits => ("Markten bezoeken", "🛒"),
            SentimentCategoryType.CozyCafes => ("Gezellige cafeetjes", "☕"),
            SentimentCategoryType.DiningOut => ("Uit eten gaan", "🍽️"),
            SentimentCategoryType.StreetFood => ("Straatvoedsel proeven", "🌮"),
            SentimentCategoryType.NewFlavours => ("Nieuwe smaken proberen", "🧂"),
            SentimentCategoryType.SweetTreats => ("Zoete desserts", "🍰"),
            SentimentCategoryType.SavourySnacks => ("Hartige snacks", "🥨"),
            SentimentCategoryType.SpicyDishes => ("Pittig eten", "🌶️"),
            SentimentCategoryType.FreshSalads => ("Frisse salades", "🥗"),
            SentimentCategoryType.SeasonalSoups => ("Seizoenssoepen", "🍲"),
            SentimentCategoryType.FruityMoments => ("Vers fruit", "🍓"),
            SentimentCategoryType.ChocolateMoments => ("Chocolade", "🍫"),
            SentimentCategoryType.CheeseBoards => ("Kaasplankjes", "🧀"),
            SentimentCategoryType.CoffeeBreaks => ("Koffie momenten", "☕"),
            SentimentCategoryType.TeaTime => ("Theepauzes", "🍵"),
            SentimentCategoryType.SmoothieBar => ("Smoothies", "🥤"),
            SentimentCategoryType.JuiceStands => ("Verse sappen", "🧃"),
            SentimentCategoryType.BreakfastDates => ("Uitgebreide ontbijtjes", "🥐"),
            SentimentCategoryType.BrunchPlans => ("Weekendbrunch", "🥞"),
            SentimentCategoryType.PicnicPlans => ("Picknicken", "🧺"),
            SentimentCategoryType.FoodTrucks => ("Foodtrucks", "🚚"),
            SentimentCategoryType.FarmersMarkets => ("Boerenmarkten", "🌻"),
            SentimentCategoryType.RoadTrips => ("Roadtrips", "🚗"),
            SentimentCategoryType.TrainJourneys => ("Treinreizen", "🚆"),
            SentimentCategoryType.FerryRides => ("Boottochtjes", "⛴️"),
            SentimentCategoryType.WellnessDays => ("Wellness dagen", "💆"),
            SentimentCategoryType.SpaRelax => ("Spa bezoeken", "🧖"),
            SentimentCategoryType.SaunaEvenings => ("Saunabezoek", "🧖‍♂️"),
            SentimentCategoryType.CinemaNights => ("Bioscoopavonden", "🎬"),
            SentimentCategoryType.SeriesMarathons => ("Series bingewatchen", "📺"),
            SentimentCategoryType.RomanticMovies => ("Romantische films", "💞"),
            SentimentCategoryType.ActionMovies => ("Actiefilms", "💥"),
            SentimentCategoryType.HorrorMovies => ("Horrorfilms", "👻"),
            SentimentCategoryType.Documentaries => ("Documentaires", "🎥"),
            SentimentCategoryType.Podcasts => ("Podcasts luisteren", "🎧"),
            SentimentCategoryType.RadioHits => ("Radiohits", "📻"),
            SentimentCategoryType.LiveConcerts => ("Live concerten", "🎶"),
            SentimentCategoryType.MusicFestivals => ("Muziekfestivals", "🎉"),
            SentimentCategoryType.DanceParties => ("Dansfeestjes", "🪩"),
            SentimentCategoryType.QuietEvenings => ("Rustige avonden thuis", "🛋️"),
            SentimentCategoryType.CandlelightDinners => ("Diner bij kaarslicht", "🕯️"),
            SentimentCategoryType.SunsetWatching => ("Zonsondergangen", "🌅"),
            SentimentCategoryType.RainyDays => ("Regenachtige dagen", "🌧️"),
            SentimentCategoryType.SnowyDays => ("Sneeuwdagen", "❄️"),
            SentimentCategoryType.AmusementParks => ("Pretparken", "🎢"),
            _ => throw new NotImplementedException(),
        };
}
