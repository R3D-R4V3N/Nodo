using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
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
        await UsersAsync();
        await ConnectionsAsync();
        await ChatsAsync();
        await MessagesAsync();
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

    private async Task UsersAsync()
    {
        if (dbContext.Users.Any())
        {
            return;
        }

        await dbContext.Roles.ToListAsync();

        IEnumerable<UserSentiment> CreateInterests()
        {
            var allSentiments = dbContext.Sentiments.ToList();

            var random = new Random();

            var groupedByCategory = allSentiments
                .GroupBy(s => s.Category)
                .Select(g => g.OrderBy(_ => random.Next()).First())
                .ToList();

            return groupedByCategory
                .OrderBy(_ => random.Next())
                .Take(5)
                .ToList();
        }

        static IEnumerable<UserHobby> CreateHobbies(params HobbyType[] hobbies)
            => hobbies.Select(x => new UserHobby { Hobby = x });

        static ApplicationUser CreateProfile(
            string accountId,
            string firstName,
            string lastName,
            string biography,
            string avatarUrl,
            DateOnly birthDay,
            UserType userType,
            IEnumerable<UserSentiment> interests,
            IEnumerable<UserHobby> hobbies)
        {
            var profile = new ApplicationUser(accountId)
            {
                FirstName = firstName,
                LastName = lastName,
                Biography = biography,
                Gender = "x",
                AvatarUrl = avatarUrl,
                BirthDay = birthDay,
                UserType = userType,
                UserSettings = new ApplicationUserSetting()
                {
                    FontSize = 12,
                    IsDarkMode = false,
                }
            };

            profile.UpdateSentiments(interests);
            profile.UpdateHobbies(hobbies);

            return profile;
        }

        IdentityUser CreateIdentity(string email) => new()
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var admin = CreateIdentity("admin@example.com");
        var supervisor = CreateIdentity("supervisor@example.com");
        var userAccount1 = CreateIdentity("user1@example.com");
        var userAccount2 = CreateIdentity("user2@example.com");
        var nodoAdmin = CreateIdentity("admin@nodo.chat");

        var supervisorEmma = CreateIdentity("emma.supervisor@nodo.chat");
        var supervisorJonas = CreateIdentity("jonas.supervisor@nodo.chat");
        var supervisorElla = CreateIdentity("ella.supervisor@nodo.chat");

        var chatterNoor = CreateIdentity("noor@nodo.chat");
        var chatterMilan = CreateIdentity("milan@nodo.chat");
        var chatterLina = CreateIdentity("lina@nodo.chat");
        var chatterKyandro = CreateIdentity("kyandro@nodo.chat");
        var chatterJasper = CreateIdentity("jasper@nodo.chat");
        var chatterBjorn = CreateIdentity("bjorn@nodo.chat");
        var chatterThibo = CreateIdentity("thibo@nodo.chat");
        var chatterSaar = CreateIdentity("saar@nodo.chat");
        var chatterYassin = CreateIdentity("yassin@nodo.chat");
        var chatterLotte = CreateIdentity("lotte@nodo.chat");
        var chatterAmina = CreateIdentity("amina@nodo.chat");

        var accounts = new List<SeedAccount>
        {
            new(admin, AppRoles.Administrator, null),
            new(supervisor, AppRoles.Supervisor,
                CreateProfile(
                    supervisor.Id,
                    "Super",
                    "Visor",
                    "Here to help you.",
                    "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                    UserType.Supervisor,
                    CreateInterests(),
                    CreateHobbies(HobbyType.Hiking, HobbyType.Painting, HobbyType.Reading))),
            new(userAccount1, AppRoles.User,
                CreateProfile(
                    userAccount1.Id,
                    "John",
                    "Doe",
                    "Houdt van katten en rustige gesprekken.",
                    "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
                    UserType.Regular,
                    CreateInterests(),
                    CreateHobbies(HobbyType.Gaming, HobbyType.BoardGames, HobbyType.ModelBuilding))),
            new(userAccount2, AppRoles.User,
                CreateProfile(
                    userAccount2.Id,
                    "Stacey",
                    "Willington",
                    "Deelt graag verhalen over haar hulphond.",
                    "https://images.unsplash.com/photo-1544723795-3fb6469f5b39?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-26)),
                    UserType.Regular,
                    CreateInterests(),
                    CreateHobbies(HobbyType.Hiking, HobbyType.Photography, HobbyType.Birdwatching))),
            new(nodoAdmin, AppRoles.Administrator, null),
            new(supervisorEmma, AppRoles.Supervisor,
                CreateProfile(
                    supervisorEmma.Id,
                    "Emma",
                    "Claes",
                    "Coach voor dagelijkse structuur en zelfvertrouwen.",
                    "https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-35)),
                    UserType.Supervisor,
                    CreateInterests(),
                    CreateHobbies(HobbyType.Gardening, HobbyType.Yoga, HobbyType.Painting))),
            new(supervisorJonas, AppRoles.Supervisor,
                CreateProfile(
                    supervisorJonas.Id,
                    "Jonas",
                    "Van Lint",
                    "Helpt bij plannen en houdt wekelijks groepsmomenten.",
                    "https://images.unsplash.com/photo-1531891437562-4301cf35b7e4?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-33)),
                    UserType.Supervisor,
                    CreateInterests(),
                    CreateHobbies(HobbyType.Football, HobbyType.Running, HobbyType.Hiking))),
            new(supervisorElla, AppRoles.Supervisor,
                CreateProfile(
                    supervisorElla.Id,
                    "Ella",
                    "Vervoort",
                    "Creatieve begeleider voor beeldende therapie.",
                    "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-31)),
                    UserType.Supervisor,
                    CreateInterests(),
                    CreateHobbies(HobbyType.Crafting, HobbyType.Painting, HobbyType.MusicMaking))),
            new(chatterNoor, AppRoles.User,
                CreateProfile(
                    chatterNoor.Id,
                    "Noor",
                    "Vermeulen",
                    "Praat graag over muziek en wil nieuwe vrienden maken.",
                    "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                    UserType.Regular,
                    CreateInterests(),
                    CreateHobbies(HobbyType.MusicMaking, HobbyType.Gaming, HobbyType.Dancing))),
            new(chatterMilan, AppRoles.User,
                CreateProfile(
                    chatterMilan.Id,
                    "Milan",
                    "Peeters",
                    "Zoekt iemand om samen over games te praten.",
                    "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                    UserType.Regular,
                    CreateInterests(),
                    CreateHobbies(HobbyType.Gaming, HobbyType.Skating, HobbyType.BoardGames))),
            new(chatterLina, AppRoles.User,
                new ApplicationUser(chatterLina.Id)
                {
                    FirstName = "Lina",
                    LastName = "Jacobs",
                    Biography = "Vindt het fijn om vragen te kunnen stellen in een veilige omgeving.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1760733345250-6b2625fca116?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
            new(chatterKyandro, AppRoles.User,
                new ApplicationUser(chatterKyandro.Id)
                {
                    FirstName = "Kyandro",
                    LastName = "Voet",
                    Biography = "Helpt vaak bij technische vragen en deelt programmeertips.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1760681555543-0a3c65fa10eb?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
            new(chatterJasper, AppRoles.User,
                new ApplicationUser(chatterJasper.Id)
                {
                    FirstName = "Jasper",
                    LastName = "Vermeersch",
                    Biography = "Vindt het leuk om te discussiëren over technologie en innovatie.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1760625525477-f725e48f5a13?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
            new(chatterBjorn, AppRoles.User,
                new ApplicationUser(chatterBjorn.Id)
                {
                    FirstName = "Bjorn",
                    LastName = "Van Damme",
                    Biography = "Praat graag over sport en houdt van teamwork.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1749521166410-9031d6ded805?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-27)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
            new(chatterThibo, AppRoles.User,
                new ApplicationUser(chatterThibo.Id)
                {
                    FirstName = "Thibo",
                    LastName = "De Smet",
                    Biography = "Is nieuwsgierig en stelt vaak interessante vragen.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1760604278004-91a4d7b22447?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-21)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
            new(chatterSaar, AppRoles.User,
                new ApplicationUser(chatterSaar.Id)
                {
                    FirstName = "Saar",
                    LastName = "Vandenberg",
                    Biography = "Deelt graag foto's van haar tekeningen.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1760497925596-a6462350c583?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
            new(chatterYassin, AppRoles.User,
                new ApplicationUser(chatterYassin.Id)
                {
                    FirstName = "Yassin",
                    LastName = "El Amrani",
                    Biography = "Leert zelfstandig koken en zoekt tips van vrienden.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1760411069721-60d7c378b697?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
            new(chatterLotte, AppRoles.User,
                new ApplicationUser(chatterLotte.Id)
                {
                    FirstName = "Lotte",
                    LastName = "De Wilde",
                    Biography = "Wordt blij van dansen en deelt positieve boodschappen.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1760086741328-c56df17e8272?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
            new(chatterAmina, AppRoles.User,
                new ApplicationUser(chatterAmina.Id)
                {
                    FirstName = "Amina",
                    LastName = "Karim",
                    Biography = "Houdt van creatieve projecten en begeleidt graag groepsspelletjes.",
                    Gender = "x",
                    AvatarUrl = "https://images.unsplash.com/photo-1739889399693-8a46b389473f?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
                    UserType = UserType.Regular,
                    UserSettings = new ApplicationUserSetting()
                    {
                        FontSize = 12,
                        IsDarkMode = false,
                    }
                }),
        };

        foreach (var account in accounts)
        {
            if (account.Profile is not { } profile)
            {
                continue;
            }

            if (!profile.Sentiments.Any())
            {
                profile.UpdateSentiments(CreateInterests());
            }

            if (!profile.Hobbies.Any())
            {
                profile.UpdateHobbies(CreateHobbies(HobbyType.Reading, HobbyType.BoardGames, HobbyType.Crafting));
            }
        }

        foreach (var (identity, role, profile) in accounts)
        {
            await userManager.CreateAsync(identity, PasswordDefault);
            await userManager.AddToRoleAsync(identity, role);

            if (profile is not null)
            {
                profile.UserSettings.AddChatTextLine("Kowabunga!");
                profile.UserSettings.AddChatTextLine("Hallo hoe gaat het?");
                dbContext.ApplicationUsers.Add(profile);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task ConnectionsAsync()
    {
        var users = await dbContext.ApplicationUsers
            .ToListAsync();

        if (users.Count == 0)
            return;

        var hasConnections = await dbContext.ApplicationUsers
            .SelectMany(u => EF.Property<IEnumerable<UserConnection>>(u, "_connections"))
            .AnyAsync();

        if (hasConnections)
            return;

        foreach (var user in users)
        {
            await dbContext.Entry(user)
                .Collection<UserConnection>("_connections")
                .LoadAsync();
        }

        ApplicationUser GetUser(string firstName) => users.Single(u => u.FirstName.Equals(firstName, StringComparison.Ordinal));

        var noor = GetUser("Noor");
        var milan = GetUser("Milan");
        var lina = GetUser("Lina");
        var kyandro = GetUser("Kyandro");
        var jasper = GetUser("Jasper");
        var bjorn = GetUser("Bjorn");
        var thibo = GetUser("Thibo");
        var saar = GetUser("Saar");
        var yassin = GetUser("Yassin");
        var lotte = GetUser("Lotte");
        var amina = GetUser("Amina");
        var john = GetUser("John");
        var stacey = GetUser("Stacey");

        // Bevestigde vriendschappen
        MakeFriends(noor, milan);
        MakeFriends(kyandro, jasper);
        MakeFriends(bjorn, thibo);
        MakeFriends(saar, yassin);
        MakeFriends(lotte, amina);

        // Openstaande verzoeken voor verschillende scenario's
        SendFriendRequest(noor, lina); // Noor wacht op antwoord van Lina
        SendFriendRequest(milan, saar); // Milan nodigt Saar uit in de gamegroep
        SendFriendRequest(john, bjorn); // John zoekt een sportbuddy
        SendFriendRequest(stacey, noor); // Stacey wil Noor beter leren kennen
        SendFriendRequest(amina, kyandro); // Amina zoekt tips voor een programmeerclub

        await dbContext.SaveChangesAsync();

        static void MakeFriends(ApplicationUser userA, ApplicationUser userB)
        {
            userA.AddFriend(userB);
            userB.AddFriend(userA);
        }

        static void SendFriendRequest(ApplicationUser requester, ApplicationUser receiver)
        {
            requester.AddFriend(receiver);
        }
    }

    private async Task ChatsAsync()
    {
        if (dbContext.Chats.Any())
        {
            return;
        }

        var supervisors = await dbContext.ApplicationUsers
            .Where(u => u.UserType == UserType.Supervisor)
            .ToListAsync();

        var chatUsers = await dbContext.ApplicationUsers
            .Where(u => u.UserType == UserType.Regular)
            .ToListAsync();

        if (supervisors.Count == 0 || chatUsers.Count < 3)
        {
            return;
        }

        var chatsToCreate = new List<Chat>
        {
            new(), // Individuele check-in
            new(), // Vrijdagavond groep
            new(), // Creatieve hoek
            new(), // Technische hulplijn
        };

        dbContext.Chats.AddRange(chatsToCreate);
        await dbContext.SaveChangesAsync();
    }

    private async Task MessagesAsync()
    {
        if (await dbContext.Messages.AnyAsync())
        {
            return;
        }

        var chats = await dbContext.Chats
            .OrderBy(c => c.Id)
            .ToListAsync();

        if (chats.Count < 4)
        {
            return;
        }
        var users = await dbContext.ApplicationUsers
            .ToDictionaryAsync(u => u.FirstName, StringComparer.Ordinal);

        var individueleCheckIn = chats[0];
        var vrijdagGroep = chats[1];
        var creatieveHoek = chats[2];
        var technischeHulp = chats[3];

        var noor = users["Noor"];
        var emma = users["Emma"];

        var milan = users["Milan"];
        var saar = users["Saar"];
        var yassin = users["Yassin"];
        var jonas = users["Jonas"];

        individueleCheckIn.AddUser(noor);
        individueleCheckIn.AddUser(emma);

        individueleCheckIn.AddTextMessage("Hoi Emma, ik ben een beetje zenuwachtig voor morgen.", noor);
        individueleCheckIn.AddTextMessage("Dat begrijp ik Noor, we bekijken samen hoe je het rustig kunt aanpakken.", emma);
        individueleCheckIn.AddTextMessage("Zal ik straks mijn checklist nog eens doornemen?", noor);
        individueleCheckIn.AddTextMessage("Ja, en ik stuur je zo meteen een ademhalingsoefening.", emma);

        vrijdagGroep.AddUser(milan);
        vrijdagGroep.AddUser(saar);
        vrijdagGroep.AddUser(yassin);
        vrijdagGroep.AddUser(jonas);

        vrijdagGroep.AddTextMessage("Wie doet er vrijdag mee met de online game-avond?", milan);
        vrijdagGroep.AddTextMessage("Ik! Zal ik snacks klaarzetten?", saar);
        vrijdagGroep.AddTextMessage("Wie doet er vrijdag mee met de online game-avond?", yassin);
        vrijdagGroep.AddTextMessage("Ik plan een korte check-in zodat iedereen zich welkom voelt.", jonas);

        creatieveHoek.AddUser(users["Lotte"]);
        creatieveHoek.AddUser(users["Amina"]);
        creatieveHoek.AddUser(users["Ella"]);

        creatieveHoek.AddTextMessage("Ik heb een nieuw schilderij gemaakt met felle kleuren!", users["Lotte"]);
        creatieveHoek.AddTextMessage("Oh wauw, kan je een foto delen?", users["Amina"]);
        creatieveHoek.AddTextMessage("Zeker! En misschien kunnen we volgende keer een collagemiddag houden?", users["Lotte"]);
        creatieveHoek.AddTextMessage("Topidee, ik zorg voor een stappenplan met eenvoudige materialen.", users["Ella"]);

        technischeHulp.AddUser(users["Jasper"]);
        technischeHulp.AddUser(users["Kyandro"]);
        technischeHulp.AddUser(users["Bjorn"]);

        technischeHulp.AddTextMessage("Mijn tablet doet raar wanneer ik de spraakopnames open.", users["Jasper"]);
        technischeHulp.AddTextMessage("Heb je al geprobeerd om de app even opnieuw te starten?", users["Kyandro"]);
        technischeHulp.AddTextMessage("Ja, maar ik twijfel of ik iets fout doe.", users["Jasper"]);
        technischeHulp.AddTextMessage("Ik kijk straks met je mee en stuur een korte handleiding door.", users["Bjorn"]);

        await dbContext.SaveChangesAsync();
    }

    private sealed record SeedAccount(IdentityUser Identity, string Role, ApplicationUser? Profile);
}
