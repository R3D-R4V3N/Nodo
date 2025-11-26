namespace Rise.Shared.Sentiments;

public static class SentimentDto
{
    public record Get
    {
        public int Id { get; init; }
        public SentimentTypeDto Type { get; init; }
        public SentimentCategoryTypeDto Category { get; init; }
        public string Text => TranslateEnumToText(Category).Name;
        public string Emoji => TranslateEnumToText(Category).Emoji;
    }

    // TODO: should prob use ids
    public record EditProfile
    {
        public SentimentTypeDto Type { get; init; }
        public SentimentCategoryTypeDto Category { get; init; }
    }
    public static (string Name, string Emoji) TranslateEnumToText(SentimentCategoryTypeDto sentiment) => sentiment switch
    {
        SentimentCategoryTypeDto.TravelAdventures => ("Reizen", "✈️"),
        SentimentCategoryTypeDto.CityTrips => ("Stedentrips", "🏙️"),
        SentimentCategoryTypeDto.BeachDays => ("Stranddagen", "🏖️"),
        SentimentCategoryTypeDto.MountainViews => ("Bergen bewonderen", "🏔️"),
        SentimentCategoryTypeDto.ShoppingSprees => ("Shoppen", "🛍️"),
        SentimentCategoryTypeDto.MarketVisits => ("Markten bezoeken", "🛒"),
        SentimentCategoryTypeDto.CozyCafes => ("Gezellige cafeetjes", "☕"),
        SentimentCategoryTypeDto.DiningOut => ("Uit eten gaan", "🍽️"),
        SentimentCategoryTypeDto.StreetFood => ("Straatvoedsel proeven", "🌮"),
        SentimentCategoryTypeDto.NewFlavours => ("Nieuwe smaken proberen", "🧂"),
        SentimentCategoryTypeDto.SweetTreats => ("Zoete desserts", "🍰"),
        SentimentCategoryTypeDto.SavourySnacks => ("Hartige snacks", "🥨"),
        SentimentCategoryTypeDto.SpicyDishes => ("Pittig eten", "🌶️"),
        SentimentCategoryTypeDto.FreshSalads => ("Frisse salades", "🥗"),
        SentimentCategoryTypeDto.SeasonalSoups => ("Seizoenssoepen", "🍲"),
        SentimentCategoryTypeDto.FruityMoments => ("Vers fruit", "🍓"),
        SentimentCategoryTypeDto.ChocolateMoments => ("Chocolade", "🍫"),
        SentimentCategoryTypeDto.CheeseBoards => ("Kaasplankjes", "🧀"),
        SentimentCategoryTypeDto.CoffeeBreaks => ("Koffie momenten", "☕"),
        SentimentCategoryTypeDto.TeaTime => ("Theepauzes", "🍵"),
        SentimentCategoryTypeDto.SmoothieBar => ("Smoothies", "🥤"),
        SentimentCategoryTypeDto.JuiceStands => ("Verse sappen", "🧃"),
        SentimentCategoryTypeDto.BreakfastDates => ("Uitgebreide ontbijtjes", "🥐"),
        SentimentCategoryTypeDto.BrunchPlans => ("Weekendbrunch", "🥞"),
        SentimentCategoryTypeDto.PicnicPlans => ("Picknicken", "🧺"),
        SentimentCategoryTypeDto.FoodTrucks => ("Foodtrucks", "🚚"),
        SentimentCategoryTypeDto.FarmersMarkets => ("Boerenmarkten", "🌻"),
        SentimentCategoryTypeDto.RoadTrips => ("Roadtrips", "🚗"),
        SentimentCategoryTypeDto.TrainJourneys => ("Treinreizen", "🚆"),
        SentimentCategoryTypeDto.FerryRides => ("Boottochtjes", "⛴️"),
        SentimentCategoryTypeDto.WellnessDays => ("Wellness dagen", "💆"),
        SentimentCategoryTypeDto.SpaRelax => ("Spa bezoeken", "🧖"),
        SentimentCategoryTypeDto.SaunaEvenings => ("Saunabezoek", "🧖‍♂️"),
        SentimentCategoryTypeDto.CinemaNights => ("Bioscoopavonden", "🎬"),
        SentimentCategoryTypeDto.SeriesMarathons => ("Series bingewatchen", "📺"),
        SentimentCategoryTypeDto.RomanticMovies => ("Romantische films", "💞"),
        SentimentCategoryTypeDto.ActionMovies => ("Actiefilms", "💥"),
        SentimentCategoryTypeDto.HorrorMovies => ("Horrorfilms", "👻"),
        SentimentCategoryTypeDto.Documentaries => ("Documentaires", "🎥"),
        SentimentCategoryTypeDto.Podcasts => ("Podcasts luisteren", "🎧"),
        SentimentCategoryTypeDto.RadioHits => ("Radiohits", "📻"),
        SentimentCategoryTypeDto.LiveConcerts => ("Live concerten", "🎶"),
        SentimentCategoryTypeDto.MusicFestivals => ("Muziekfestivals", "🎉"),
        SentimentCategoryTypeDto.DanceParties => ("Dansfeestjes", "🪩"),
        SentimentCategoryTypeDto.QuietEvenings => ("Rustige avonden thuis", "🛋️"),
        SentimentCategoryTypeDto.CandlelightDinners => ("Diner bij kaarslicht", "🕯️"),
        SentimentCategoryTypeDto.SunsetWatching => ("Zonsondergangen", "🌅"),
        SentimentCategoryTypeDto.RainyDays => ("Regenachtige dagen", "🌧️"),
        SentimentCategoryTypeDto.SnowyDays => ("Sneeuwdagen", "❄️"),
        SentimentCategoryTypeDto.AmusementParks => ("Pretparken", "🎢"),
        _ => throw new ArgumentOutOfRangeException(nameof(sentiment), sentiment, "No translation configured for sentiment type."),
    };
}
