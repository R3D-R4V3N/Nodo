using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Events;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Domain.Users.Settings;
using Rise.Shared.Identity;

namespace Rise.Persistence;

/// <summary>
/// Seeds the database.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="roleManager"></param>
/// <param name="userManager"></param>
public class DbSeeder(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
{
    private const string PasswordDefault = "Nodo.1";

    public async Task SeedAsync()
    {
        await RolesAsync();
        await SentimentsAsync();
        await HobbiesAsync();
        await OrganizationsAsync();
        await UsersAsync();
        await ConnectionsAsync();
        await MessagesAsync();
        await EventsAsync();
        //await ProductsAsync();
        //await ProjectsAsync();
    }

    private async Task RolesAsync()
    {
        if (dbContext.Roles.Any())
        {
            return;
        }

        await roleManager.CreateAsync(new IdentityRole("Administrator"));
        await roleManager.CreateAsync(new IdentityRole("Supervisor"));
        await roleManager.CreateAsync(new IdentityRole("User"));
    }

    private async Task SentimentsAsync()
    {
        if (dbContext.Sentiments.Any())
        {
            return;
        }

        var sentimentList = new List<UserSentiment>();

        foreach (var categoryType in Enum.GetValues<SentimentCategoryType>())
        {
            sentimentList.Add(
                new UserSentiment()
                {
                    Type = SentimentType.Like,
                    Category = categoryType,
                }
            );

            sentimentList.Add(
                new UserSentiment()
                {
                    Type = SentimentType.Dislike,
                    Category = categoryType,
                }
            );
        }

        dbContext.Sentiments.AddRange(sentimentList);
        await dbContext.SaveChangesAsync();
    }

    private async Task HobbiesAsync()
    {
        if (dbContext.Hobbies.Any())
        {
            return;
        }

        var hobbyList = new List<UserHobby>();

        foreach (var hobby in Enum.GetValues<HobbyType>())
        {
            hobbyList.Add(
                new UserHobby()
                {
                    Hobby = hobby,
                }
            );
        }

        dbContext.Hobbies.AddRange(hobbyList);
        await dbContext.SaveChangesAsync();
    }

    private async Task OrganizationsAsync()
    {
        if (dbContext.Organizations.Any())
        {
            dbContext.Organizations.RemoveRange(dbContext.Organizations);
            await dbContext.SaveChangesAsync();
        }

        var organizations = new List<Organization>
        {
            new("Gent", "Begeleiding en ontmoetingen in hartje Gent."),
            new("Antwerpen", "Creatieve uitvalsbasis dicht bij de Schelde."),
            new("Brugge", "Kleinschalige werking in de historische binnenstad."),
            new("Leuven", "Studentikoze energie met rustige ontmoetingsruimtes."),
            new("Mechelen", "Steunpunt vlak bij de Dijle met gezellige ateliers."),
            new("Hasselt", "Limburgse warmte met een focus op muziek en beweging."),
            new("Kortrijk", "Samenwerking met lokale sport- en cultuurhuizen."),
            new("Namur", "Meertalige begeleiding langs de Maas."),
            new("Liège", "Spontane groepsactiviteiten in het Luikse."),
        };

        dbContext.Organizations.AddRange(organizations);
        await dbContext.SaveChangesAsync();
    }


    private async Task UsersAsync()
    {
        dbContext.Messages.RemoveRange(dbContext.Messages);
        dbContext.Chats.RemoveRange(dbContext.Chats);
        dbContext.Events.RemoveRange(dbContext.Events);
        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.Supervisors.RemoveRange(dbContext.Supervisors);
        dbContext.Admins.RemoveRange(dbContext.Admins);
        await dbContext.SaveChangesAsync();

        var existingIdentities = await userManager.Users.ToListAsync();
        foreach (var identityUser in existingIdentities)
        {
            await userManager.DeleteAsync(identityUser);
        }

        await dbContext.Roles.ToListAsync();

        var organizationsByName = await dbContext.Organizations
            .ToDictionaryAsync(o => o.Name, StringComparer.OrdinalIgnoreCase);

        Organization GetOrganization(string name)
            => organizationsByName.TryGetValue(name, out var organization)
                ? organization
                : throw new InvalidOperationException($"Organisatie '{name}' werd niet gevonden.");

        var gent = GetOrganization("Gent");
        var antwerpen = GetOrganization("Antwerpen");
        var brugge = GetOrganization("Brugge");
        var leuven = GetOrganization("Leuven");
        var mechelen = GetOrganization("Mechelen");
        var hasselt = GetOrganization("Hasselt");
        var kortrijk = GetOrganization("Kortrijk");
        var namur = GetOrganization("Namur");
        var liege = GetOrganization("Liège");

        var sentimentByKey = dbContext.Sentiments
            .ToDictionary(s => (s.Type, s.Category));

        var hobbyByType = dbContext.Hobbies
            .ToDictionary(h => h.Hobby);

        IEnumerable<UserSentiment> Sentiments(params (SentimentType type, SentimentCategoryType category)[] entries)
            => entries.Select(e => sentimentByKey[e]).ToList();

        IEnumerable<UserHobby> Hobbies(params HobbyType[] hobbies)
            => hobbies.Select(h => hobbyByType[h]).ToList();

        User CreateUser(
            string firstName,
            string lastName,
            string biography,
            string avatarUrl,
            DateOnly birthDay,
            GenderType gender,
            Organization organization,
            Supervisor supervisor,
            HobbyType[] hobbies,
            SentimentCategoryType[] likes,
            SentimentCategoryType[] dislikes,
            string accountId)
        {
            var user = new User
            {
                AccountId = accountId,
                FirstName = FirstName.Create(firstName),
                LastName = LastName.Create(lastName),
                Biography = Biography.Create(biography),
                AvatarUrl = BlobUrl.Create(avatarUrl),
                BirthDay = BirthDay.Create(birthDay),
                Gender = gender,
                Organization = organization,
                Supervisor = supervisor,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            };

            user.UpdateHobbies(Hobbies(hobbies));
            user.UpdateSentiments(
                Sentiments(
                    likes.Select(l => (SentimentType.Like, l))
                        .Concat(dislikes.Select(d => (SentimentType.Dislike, d)))
                        .ToArray()));

            return user;
        }

        IdentityUser CreateIdentity(string email) => new()
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var begeleiderThibo = new Func<string, Supervisor>(accountId => new Supervisor
        {
            AccountId = accountId,
            FirstName = FirstName.Create("Thibo"),
            LastName = LastName.Create("Verbeke"),
            Biography = Biography.Create("Begeleider in Gent die structuur en humor combineert."),
            AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=crop&w=800&q=80"),
            BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-34))),
            Gender = GenderType.Man,
            Organization = gent,
            UserSettings = new UserSetting
            {
                FontSize = FontSize.Create(12),
                IsDarkMode = false,
            },
        });

        var begeleiders = new Dictionary<string, Func<string, Supervisor>>
        {
            ["emma.begeleider@nodo.chat"] = accountId => new Supervisor
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Emma"),
                LastName = LastName.Create("Van Pelt"),
                Biography = Biography.Create("Begeleider in Antwerpen met focus op zachte opstart."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1504595403659-9088ce801e29?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-32))),
                Gender = GenderType.Woman,
                Organization = antwerpen,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            },
            ["jonas.begeleider@nodo.chat"] = accountId => new Supervisor
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Jonas"),
                LastName = LastName.Create("Bauwens"),
                Biography = Biography.Create("Begeleider in Brugge, deelt graag wandelroutes."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1544723795-3fb6469f5b39?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-36))),
                Gender = GenderType.Man,
                Organization = brugge,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            },
            ["lotte.begeleider@nodo.chat"] = accountId => new Supervisor
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Lotte"),
                LastName = LastName.Create("Geens"),
                Biography = Biography.Create("Begeleider in Leuven die groepsmomenten organiseert."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-31))),
                Gender = GenderType.Woman,
                Organization = leuven,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            },
            ["ruben.begeleider@nodo.chat"] = accountId => new Supervisor
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Ruben"),
                LastName = LastName.Create("Vercammen"),
                Biography = Biography.Create("Begeleider in Mechelen die graag plannen tekent."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-33))),
                Gender = GenderType.Man,
                Organization = mechelen,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            },
            ["amira.begeleider@nodo.chat"] = accountId => new Supervisor
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Amira"),
                LastName = LastName.Create("Aydin"),
                Biography = Biography.Create("Begeleider in Hasselt met talent voor muziek."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-29))),
                Gender = GenderType.Woman,
                Organization = hasselt,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            },
            ["elise.begeleider@nodo.chat"] = accountId => new Supervisor
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Elise"),
                LastName = LastName.Create("Declercq"),
                Biography = Biography.Create("Begeleider in Kortrijk die sportactiviteiten opzet."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-30))),
                Gender = GenderType.Woman,
                Organization = kortrijk,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            },
            ["victor.begeleider@nodo.chat"] = accountId => new Supervisor
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Victor"),
                LastName = LastName.Create("Duchêne"),
                Biography = Biography.Create("Begeleider in Namur met aandacht voor meertaligheid."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-38))),
                Gender = GenderType.Man,
                Organization = namur,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            },
            ["chloe.begeleider@nodo.chat"] = accountId => new Supervisor
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Chloé"),
                LastName = LastName.Create("Renard"),
                Biography = Biography.Create("Begeleider in Liège met oog voor creatieve expressie."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-35))),
                Gender = GenderType.Woman,
                Organization = liege,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            },
        };

        var supervisorProfiles = new Dictionary<string, Supervisor>();
        Supervisor GetSupervisor(string email)
            => supervisorProfiles.TryGetValue(email, out var supervisor)
                ? supervisor
                : throw new InvalidOperationException($"Begeleider '{email}' werd niet gevonden.");

        var snelleBerichten = new[]
        {
            "Zullen we dit later even samen bekijken?",
            "Bedankt om dit te delen!",
            "Ik noteer het in mijn planner.",
            "Ik stuur je straks een update.",
            "Laten we het stap voor stap doen.",
            "Ik heb er zin in!",
        };

        var random = new Random();

        var accounts = new List<SeedAccount>
        {
            new("beheer@nodo.chat", AppRoles.Administrator, accountId => new Admin
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Beheer"),
                LastName = LastName.Create("Team"),
                Biography = Biography.Create("Adminaccount voor centrale ondersteuning."),
                AvatarUrl = BlobUrl.Create("https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80"),
                BirthDay = BirthDay.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(-40))),
                Gender = GenderType.X,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
            }),
            new("thibo.begeleider@nodo.chat", AppRoles.Supervisor, begeleiderThibo),
            new("emma.begeleider@nodo.chat", AppRoles.Supervisor, begeleiders["emma.begeleider@nodo.chat"]),
            new("jonas.begeleider@nodo.chat", AppRoles.Supervisor, begeleiders["jonas.begeleider@nodo.chat"]),
            new("lotte.begeleider@nodo.chat", AppRoles.Supervisor, begeleiders["lotte.begeleider@nodo.chat"]),
            new("ruben.begeleider@nodo.chat", AppRoles.Supervisor, begeleiders["ruben.begeleider@nodo.chat"]),
            new("amira.begeleider@nodo.chat", AppRoles.Supervisor, begeleiders["amira.begeleider@nodo.chat"]),
            new("elise.begeleider@nodo.chat", AppRoles.Supervisor, begeleiders["elise.begeleider@nodo.chat"]),
            new("victor.begeleider@nodo.chat", AppRoles.Supervisor, begeleiders["victor.begeleider@nodo.chat"]),
            new("chloe.begeleider@nodo.chat", AppRoles.Supervisor, begeleiders["chloe.begeleider@nodo.chat"]),
            new("demo@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Demo",
                "Account",
                "Standaard account om de app uit te proberen.",
                "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-27)),
                GenderType.X,
                gent,
                GetSupervisor("thibo.begeleider@nodo.chat"),
                new[] { HobbyType.Gaming, HobbyType.Reading, HobbyType.Swimming },
                new[] { SentimentCategoryType.CinemaNights, SentimentCategoryType.TrainJourneys, SentimentCategoryType.CozyCafes },
                new[] { SentimentCategoryType.SpicyDishes, SentimentCategoryType.HorrorMovies },
                accountId)),
            new("ayla@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Ayla",
                "Coppens",
                "Vertelt enthousiast over haar nieuwe recepten.",
                "https://images.unsplash.com/photo-1544723795-3fb6469f5b39?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                GenderType.Woman,
                antwerpen,
                GetSupervisor("emma.begeleider@nodo.chat"),
                new[] { HobbyType.Cooking, HobbyType.Baking, HobbyType.BoardGames },
                new[] { SentimentCategoryType.BrunchPlans, SentimentCategoryType.SweetTreats, SentimentCategoryType.CozyCafes },
                new[] { SentimentCategoryType.ActionMovies, SentimentCategoryType.SaunaEvenings },
                accountId)),
            new("pieter@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Pieter",
                "Verlinden",
                "Deelt graag foto's van zijn fietstochten.",
                "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-26)),
                GenderType.Man,
                gent,
                GetSupervisor("thibo.begeleider@nodo.chat"),
                new[] { HobbyType.Cycling, HobbyType.Photography, HobbyType.Camping },
                new[] { SentimentCategoryType.RoadTrips, SentimentCategoryType.SunsetWatching, SentimentCategoryType.LiveConcerts },
                new[] { SentimentCategoryType.MarketVisits, SentimentCategoryType.HorrorMovies },
                accountId)),
            new("jamila@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Jamila",
                "Benali",
                "Zoekt maatjes om samen yoga te doen.",
                "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
                GenderType.Woman,
                hasselt,
                GetSupervisor("amira.begeleider@nodo.chat"),
                new[] { HobbyType.Yoga, HobbyType.Reading, HobbyType.Pilates },
                new[] { SentimentCategoryType.WellnessDays, SentimentCategoryType.TeaTime, SentimentCategoryType.FruityMoments },
                new[] { SentimentCategoryType.SpicyDishes, SentimentCategoryType.AmusementParks },
                accountId)),
            new("hugo@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Hugo",
                "Claeys",
                "Is dol op oldschool games en puzzels.",
                "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
                GenderType.Man,
                brugge,
                GetSupervisor("jonas.begeleider@nodo.chat"),
                new[] { HobbyType.Gaming, HobbyType.Puzzles, HobbyType.Chess },
                new[] { SentimentCategoryType.SeriesMarathons, SentimentCategoryType.BoardGames, SentimentCategoryType.CinemaNights },
                new[] { SentimentCategoryType.SaunaEvenings, SentimentCategoryType.FreshSalads },
                accountId)),
            new("naomi@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Naomi",
                "Segers",
                "Schrijft korte verhalen en deelt ze graag.",
                "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                GenderType.Woman,
                leuven,
                GetSupervisor("lotte.begeleider@nodo.chat"),
                new[] { HobbyType.Writing, HobbyType.Dancing, HobbyType.Crafting },
                new[] { SentimentCategoryType.CozyCafes, SentimentCategoryType.ChocolateMoments, SentimentCategoryType.Podcasts },
                new[] { SentimentCategoryType.ActionMovies, SentimentCategoryType.BeachDays },
                accountId)),
            new("elias@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Elias",
                "Vandenbulcke",
                "Volgt kookles en test nieuwe smaken uit.",
                "https://images.unsplash.com/photo-1511367466-2500eceaa34e?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                GenderType.Man,
                mechelen,
                GetSupervisor("ruben.begeleider@nodo.chat"),
                new[] { HobbyType.Cooking, HobbyType.Gardening, HobbyType.BoardGames },
                new[] { SentimentCategoryType.NewFlavours, SentimentCategoryType.MarketVisits, SentimentCategoryType.FoodTrucks },
                new[] { SentimentCategoryType.HorrorMovies, SentimentCategoryType.RainyDays },
                accountId)),
            new("sofie@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Sofie",
                "Van Herck",
                "Deelt playlists en organiseert dansmomenten.",
                "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-21)),
                GenderType.Woman,
                antwerpen,
                GetSupervisor("emma.begeleider@nodo.chat"),
                new[] { HobbyType.Dancing, HobbyType.MusicMaking, HobbyType.Running },
                new[] { SentimentCategoryType.MusicFestivals, SentimentCategoryType.LiveConcerts, SentimentCategoryType.DanceParties },
                new[] { SentimentCategoryType.SnowyDays, SentimentCategoryType.SaunaEvenings },
                accountId)),
            new("bram@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Bram",
                "Lauwers",
                "Maakt graag houtprojecten en deelt foto's.",
                "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-29)),
                GenderType.Man,
                kortrijk,
                GetSupervisor("elise.begeleider@nodo.chat"),
                new[] { HobbyType.Woodworking, HobbyType.Camping, HobbyType.Fishing },
                new[] { SentimentCategoryType.MarketVisits, SentimentCategoryType.RoadTrips, SentimentCategoryType.PicnicPlans },
                new[] { SentimentCategoryType.ShoppingSprees, SentimentCategoryType.CinemaNights },
                accountId)),
            new("yara@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Yara",
                "Danneels",
                "Probeert elke maand een nieuw boek uit.",
                "https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                GenderType.Woman,
                liege,
                GetSupervisor("chloe.begeleider@nodo.chat"),
                new[] { HobbyType.Reading, HobbyType.Drawing, HobbyType.Photography },
                new[] { SentimentCategoryType.CozyCafes, SentimentCategoryType.CinemaNights, SentimentCategoryType.BreakfastDates },
                new[] { SentimentCategoryType.AmusementParks, SentimentCategoryType.SpicyDishes },
                accountId)),
            new("matteo@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Matteo",
                "Rossi",
                "Houdt van treinreizen en leert Nederlands.",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-27)),
                GenderType.Man,
                namur,
                GetSupervisor("victor.begeleider@nodo.chat"),
                new[] { HobbyType.Hiking, HobbyType.Football, HobbyType.BoardGames },
                new[] { SentimentCategoryType.TrainJourneys, SentimentCategoryType.CozyCafes, SentimentCategoryType.SeriesMarathons },
                new[] { SentimentCategoryType.HorrorMovies, SentimentCategoryType.SpicyDishes },
                accountId)),
            new("ines@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Ines",
                "Dubois",
                "Maakt keramiek en toont graag het proces.",
                "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                GenderType.Woman,
                brugge,
                GetSupervisor("jonas.begeleider@nodo.chat"),
                new[] { HobbyType.Pottery, HobbyType.Baking, HobbyType.Sewing },
                new[] { SentimentCategoryType.CozyCafes, SentimentCategoryType.SweetTreats, SentimentCategoryType.CandlelightDinners },
                new[] { SentimentCategoryType.ActionMovies, SentimentCategoryType.SnowyDays },
                accountId)),
            new("olivia@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Olivia",
                "Van Hoof",
                "Doet vrijwilligerswerk en plant picknicks.",
                "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
                GenderType.Woman,
                mechelen,
                GetSupervisor("ruben.begeleider@nodo.chat"),
                new[] { HobbyType.Gardening, HobbyType.Swimming, HobbyType.Crafting },
                new[] { SentimentCategoryType.PicnicPlans, SentimentCategoryType.FarmersMarkets, SentimentCategoryType.SeasonalSoups },
                new[] { SentimentCategoryType.HorrorMovies, SentimentCategoryType.SnowyDays },
                accountId)),
            new("celine@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Céline",
                "Marchal",
                "Leert programmeren en zoekt studiemaatjes.",
                "https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                GenderType.Woman,
                namur,
                GetSupervisor("victor.begeleider@nodo.chat"),
                new[] { HobbyType.Gaming, HobbyType.ModelBuilding, HobbyType.CardGames },
                new[] { SentimentCategoryType.Podcasts, SentimentCategoryType.SeriesMarathons, SentimentCategoryType.SmoothieBar },
                new[] { SentimentCategoryType.SpicyDishes, SentimentCategoryType.AmusementParks },
                accountId)),
            new("thomas@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Thomas",
                "Maes",
                "Spreekt graag over sport en digitale tools.",
                "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                GenderType.Man,
                kortrijk,
                GetSupervisor("elise.begeleider@nodo.chat"),
                new[] { HobbyType.Basketball, HobbyType.Running, HobbyType.BoardGames },
                new[] { SentimentCategoryType.CoffeeBreaks, SentimentCategoryType.ActionMovies, SentimentCategoryType.MusicFestivals },
                new[] { SentimentCategoryType.SaunaEvenings, SentimentCategoryType.SweetTreats },
                accountId)),
            new("louis@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Louis",
                "Dupont",
                "Houdt van fotografie en rustige wandelingen.",
                "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
                GenderType.Man,
                liege,
                GetSupervisor("chloe.begeleider@nodo.chat"),
                new[] { HobbyType.Photography, HobbyType.Hiking, HobbyType.Yoga },
                new[] { SentimentCategoryType.SunsetWatching, SentimentCategoryType.TeaTime, SentimentCategoryType.RoadTrips },
                new[] { SentimentCategoryType.HorrorMovies, SentimentCategoryType.SaunaEvenings },
                accountId)),
            new("ana@nodo.chat", AppRoles.User, accountId => CreateUser(
                "Ana",
                "Martins",
                "Oefent Nederlands en zoekt gesprekspartners.",
                "https://images.unsplash.com/photo-1511367466-2500eceaa34e?auto=format&fit=crop&w=800&q=80",
                DateOnly.FromDateTime(DateTime.Today.AddYears(-19)),
                GenderType.Woman,
                antwerpen,
                GetSupervisor("emma.begeleider@nodo.chat"),
                new[] { HobbyType.Singing, HobbyType.BoardGames, HobbyType.Knitting },
                new[] { SentimentCategoryType.CozyCafes, SentimentCategoryType.Podcasts, SentimentCategoryType.TrainJourneys },
                new[] { SentimentCategoryType.ActionMovies, SentimentCategoryType.SpicyDishes },
                accountId)),
        };

        var existingProfiles = new Dictionary<string, BaseUser>();

        foreach (var account in accounts)
        {
            var identity = await userManager.FindByEmailAsync(account.Email);

            if (identity is null)
            {
                identity = CreateIdentity(account.Email);
                var createResult = await userManager.CreateAsync(identity, PasswordDefault);
                if (!createResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Kon account '{account.Email}' niet aanmaken: {errorMessage}");
                }
            }

            if (!await userManager.IsInRoleAsync(identity, account.Role))
            {
                await userManager.AddToRoleAsync(identity, account.Role);
            }

            if (account.ProfileFactory is null)
            {
                continue;
            }

            var profile = account.ProfileFactory(identity.Id);
            if (profile is null)
            {
                continue;
            }

            foreach (var line in snelleBerichten.OrderBy(_ => random.Next()).Take(3))
            {
                profile.UserSettings.AddChatTextLine(line);
            }

            switch (profile)
            {
                case User userProfile:
                    var superChat = Chat.CreateSupervisorChat(userProfile, userProfile.Supervisor);
                    dbContext.Users.Add(userProfile);
                    dbContext.Chats.Add(superChat);
                    break;
                case Supervisor supervisorProfile:
                    dbContext.Supervisors.Add(supervisorProfile);
                    supervisorProfiles[account.Email] = supervisorProfile;
                    break;
                case Admin adminProfile:
                    dbContext.Admins.Add(adminProfile);
                    break;
            }

            existingProfiles[identity.Id] = profile;
        }

        await dbContext.SaveChangesAsync();

    }

    private async Task ConnectionsAsync()
    {
        var users = await dbContext
            .Users
            .Include(u => u.Connections)
            .ToListAsync();

        if (users.Count == 0)
            return;

        var hasConnections = await dbContext
            .Users
            .SelectMany(u => u.Connections)
            .AnyAsync();

        if (hasConnections)
            return;

        var demo = users.GetUser("Demo");
        var ayla = users.GetUser("Ayla");
        var pieter = users.GetUser("Pieter");
        var jamila = users.GetUser("Jamila");
        var hugo = users.GetUser("Hugo");
        var naomi = users.GetUser("Naomi");
        var elias = users.GetUser("Elias");
        var sofie = users.GetUser("Sofie");
        var bram = users.GetUser("Bram");
        var yara = users.GetUser("Yara");
        var matteo = users.GetUser("Matteo");
        var ines = users.GetUser("Ines");
        var olivia = users.GetUser("Olivia");
        var celine = users.GetUser("Céline");
        var thomas = users.GetUser("Thomas");
        var louis = users.GetUser("Louis");
        var ana = users.GetUser("Ana");

        MakeFriends(demo, ayla);
        MakeFriends(demo, pieter);
        MakeFriends(ayla, sofie);
        MakeFriends(pieter, jamila);
        MakeFriends(hugo, naomi);
        MakeFriends(elias, jamila);
        MakeFriends(sofie, ayla);
        MakeFriends(bram, olivia);
        MakeFriends(bram, thomas);
        MakeFriends(yara, matteo);
        MakeFriends(yara, ines);
        MakeFriends(celine, ana);
        MakeFriends(naomi, louis);
        MakeFriends(sofie, demo);

        SendFriendRequest(demo, yara);
        SendFriendRequest(elias, thomas);
        SendFriendRequest(olivia, sofie);
        SendFriendRequest(celine, jamila);
        SendFriendRequest(hugo, bram);

        await dbContext.SaveChangesAsync();

        static void MakeFriends(User userA, User userB)
        {
            userA.SendFriendRequest(userB);
            userB.AcceptFriendRequest(userA);
            Chat.CreatePrivateChat(userA, userB);
        }

        static void SendFriendRequest(User requester, User receiver)
        {
            requester.SendFriendRequest(receiver);
        }
    }

    private async Task MessagesAsync()
    {
        if (await dbContext.Messages.AnyAsync())
        {
            return;
        }

        var chats = await dbContext.Chats
            .Where(c => c.ChatType == ChatType.Private)
            .OrderBy(c => c.Id)
            .ToListAsync();

        if (chats.Count < 4)
        {
            return;
        }

        var individueleCheckIn = chats[0];
        var vrijdagGroep = chats[1];
        var creatieveHoek = chats[2];
        var technischeHulp = chats[3];

        individueleCheckIn.AddTextMessage("Hoi, ik wil even checken hoe je dag verlopen is.", individueleCheckIn.RandomUser());
        individueleCheckIn.AddTextMessage("Bedankt om het te delen, zullen we morgen een plan opstellen?", individueleCheckIn.RandomUser());
        individueleCheckIn.AddTextMessage("Ik heb de ademhalingsoefening klaarstaan als je wil.", individueleCheckIn.RandomUser());
        individueleCheckIn.AddTextMessage("Top, stuur hem maar door dan probeer ik hem vanavond.", individueleCheckIn.RandomUser());

        vrijdagGroep.AddTextMessage("Wie heeft er zin in de wandeling langs de Schelde?", vrijdagGroep.RandomUser());
        vrijdagGroep.AddTextMessage("Ik neem warme chocomelk mee!", vrijdagGroep.RandomUser());
        vrijdagGroep.AddTextMessage("Zal ik een kort spelletje voorbereiden voor onderweg?", vrijdagGroep.RandomUser());
        vrijdagGroep.AddTextMessage("Ja graag, dan hebben we meteen een ijsbreker.", vrijdagGroep.RandomUser());

        for (int i = 1; i <= 200; i++)
        {
            creatieveHoek.AddTextMessage($"Bericht {i}", creatieveHoek.RandomUser());
        }

        technischeHulp.AddTextMessage("Mijn tablet doet raar wanneer ik de spraakopnames open.", technischeHulp.RandomUser());
        technischeHulp.AddTextMessage("Heb je al geprobeerd om de app even opnieuw te starten?", technischeHulp.RandomUser());
        technischeHulp.AddTextMessage("Ja, maar ik twijfel of ik iets fout doe.", technischeHulp.RandomUser());
        technischeHulp.AddTextMessage("Ik kijk straks met je mee en stuur een korte handleiding door.", technischeHulp.RandomUser());


        await dbContext.SaveChangesAsync();
    }

    private async Task EventsAsync()
    {
        if (dbContext.Events.Any())
        {
            return;
        }

        var users = await dbContext
            .Users
            .ToListAsync();

        if (users.Count == 0)
            return;

        var demo = users.GetUser("Demo");
        var ayla = users.GetUser("Ayla");
        var pieter = users.GetUser("Pieter");
        var jamila = users.GetUser("Jamila");
        var hugo = users.GetUser("Hugo");
        var naomi = users.GetUser("Naomi");

        var events = new List<Event>
        {
            new Event
            {
                Name = "Avondwandeling Gentse Leien",
                Date = DateTime.Now.AddDays(6).Date.AddHours(19),
                Location = "9000 Gent",
                Price = 0.00,
                ImageUrl = "https://images.unsplash.com/photo-1523797467744-1b91a06c24db?auto=format&fit=crop&w=800&q=80",
                InterestedUsers = new List<User> { demo, pieter, ayla }
            },
            new Event
            {
                Name = "Filmavond in Leuven",
                Date = DateTime.Now.AddDays(12).Date.AddHours(20),
                Location = "3000 Leuven",
                Price = 4.50,
                ImageUrl = "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?auto=format&fit=crop&w=800&q=80",
                InterestedUsers = new List<User> { jamila, naomi, hugo, demo }
            },
            new Event
            {
                Name = "Keramiekatelier Kortrijk",
                Date = DateTime.Now.AddDays(20).Date.AddHours(14),
                Location = "8500 Kortrijk",
                Price = 9.00,
                ImageUrl = "https://images.unsplash.com/photo-1523419400524-fc1e0d1c1c5b?auto=format&fit=crop&w=800&q=80",
                InterestedUsers = new List<User> { jamila, pieter, naomi, ayla, demo }
            }
        };

        dbContext.Events.AddRange(events);
        await dbContext.SaveChangesAsync();
    }

    private sealed record SeedAccount(string Email, string Role, Func<string, BaseUser?>? ProfileFactory);
}

internal static class DbSeederExtensions
{
    public static BaseUser RandomUser(this Chat chat) => chat.Users.Count == 0
        ? throw new Exception()
        : chat.Users[new Random().Next(0, chat.Users.Count)];
    public static User GetUser(this List<User> users, string firstName)
        => users.First(u => u.FirstName.Value.Equals(firstName, StringComparison.Ordinal));
}