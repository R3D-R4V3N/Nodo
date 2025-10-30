using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
using Rise.Shared.Identity;
using Rise.Shared.Users;
using System;
using System.Linq;

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

        static IEnumerable<UserHobby> CreateHobbies(
            ApplicationDbContext dbContext, 
            params HobbyType[] hobbies)
        {
            var dbHobbies = dbContext.Hobbies
                                     .Where(uh => hobbies.Contains(uh.Hobby))
                                     .ToList();

            return dbHobbies;
        }

        static ApplicationUser CreateProfile(
            string accountId,
            string firstName,
            string lastName,
            string biography,
            string avatarUrl,
            string gender,
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
                AvatarUrl = avatarUrl,
                Gender = gender,
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
                    "https://images.unsplash.com/photo-1761405378284-834f87bb9818?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928",
                    "x",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                    UserType.Supervisor,
                    CreateInterests(),
                    CreateHobbies(dbContext, HobbyType.Hiking, HobbyType.Painting, HobbyType.Reading))),
            new(userAccount1, AppRoles.User,
                CreateProfile(
                    userAccount1.Id,
                    "John",
                    "Doe",
                    "Houdt van katten en rustige gesprekken.",
                    "https://images.unsplash.com/photo-1499996860823-5214fcc65f8f?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=932",
                    "man",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
                    UserType.Regular,
                    CreateInterests(),
                    CreateHobbies(dbContext, HobbyType.Gaming, HobbyType.BoardGames, HobbyType.ModelBuilding))),
            new(userAccount2, AppRoles.User,
                CreateProfile(
                    userAccount2.Id,
                    "Stacey",
                    "Willington",
                    "Deelt graag verhalen over haar hulphond.",
                    "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
                    "vrouw",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-26)),
                    UserType.Regular,
                    CreateInterests(),
                    CreateHobbies(dbContext, HobbyType.Hiking, HobbyType.Photography, HobbyType.Birdwatching))),
            new(nodoAdmin, AppRoles.Administrator, null),
            new(supervisorEmma, AppRoles.Supervisor,
                CreateProfile(
                    supervisorEmma.Id,
                    "Emma",
                    "Claes",
                    "Coach voor dagelijkse structuur en zelfvertrouwen.",
                    "https://images.unsplash.com/photo-1544005313-94ddf0286df2?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MjZ8fGF2YXRhcnxlbnwwfHwwfHx8MA%3D%3D&auto=format&fit=crop&q=60&w=700",
                    "vrouw",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-35)),
                    UserType.Supervisor,
                    CreateInterests(),
                    CreateHobbies(dbContext, HobbyType.Gardening, HobbyType.Yoga, HobbyType.Painting))),
            new(supervisorJonas, AppRoles.Supervisor,
                CreateProfile(
                    supervisorJonas.Id,
                    "Jonas",
                    "Van Lint",
                    "Helpt bij plannen en houdt wekelijks groepsmomenten.",
                    "https://images.unsplash.com/photo-1639149888905-fb39731f2e6c?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928",
                    "man",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-33)),
                    UserType.Supervisor,
                    CreateInterests(),
                    CreateHobbies(dbContext, HobbyType.Football, HobbyType.Running, HobbyType.Hiking))),
            new(supervisorElla, AppRoles.Supervisor,
                CreateProfile(
                    supervisorElla.Id,
                    "Ella",
                    "Vervoort",
                    "Creatieve begeleider voor beeldende therapie.",
                    "https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928",
                    "vrouw",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-31)),
                    UserType.Supervisor,
                    CreateInterests(),
                    CreateHobbies(dbContext, HobbyType.Crafting, HobbyType.Painting, HobbyType.MusicMaking))),
            new(chatterNoor, AppRoles.User,
                CreateProfile(
                    chatterNoor.Id,
                    "Noor",
                    "Vermeulen",
                    "Praat graag over muziek en wil nieuwe vrienden maken.",
                    "https://images.unsplash.com/photo-1580489944761-15a19d654956?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=922",
                    "vrouw",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                    UserType.Regular,
                    CreateInterests(),
                    CreateHobbies(dbContext, HobbyType.MusicMaking, HobbyType.Gaming, HobbyType.Dancing))),
            new(chatterMilan, AppRoles.User,
                CreateProfile(
                    chatterMilan.Id,
                    "Milan",
                    "Peeters",
                    "Zoekt iemand om samen over games te praten.",
                    "https://images.unsplash.com/photo-1587397845856-e6cf49176c70?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
                    "man",
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                    UserType.Regular,
                    CreateInterests(),
                    CreateHobbies(dbContext, HobbyType.Gaming, HobbyType.Skating, HobbyType.BoardGames))),
            new(chatterLina, AppRoles.User,
                new ApplicationUser(chatterLina.Id)
                {
                    FirstName = "Lina",
                    LastName = "Jacobs",
                    Biography = "Vindt het fijn om vragen te kunnen stellen in een veilige omgeving.",
                    AvatarUrl = "https://plus.unsplash.com/premium_photo-1687832254672-bf177d8819df?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
                    Gender = "vrouw",
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
                    AvatarUrl = "https://plus.unsplash.com/premium_photo-1664536392896-cd1743f9c02c?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
                    Gender = "man",
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
                    AvatarUrl = "https://plus.unsplash.com/premium_photo-1671656349218-5218444643d8?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
                    Gender = "man",
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
                    AvatarUrl = "https://images.unsplash.com/photo-1704726135027-9c6f034cfa41?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=770",
                    Gender = "man",
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
                    AvatarUrl = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
                    Gender = "man",
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
                    AvatarUrl = "https://images.unsplash.com/photo-1760497925596-a6462350c583?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80",
                    Gender = "vrouw",
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
                    AvatarUrl = "https://images.unsplash.com/photo-1704726135027-9c6f034cfa41?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=770",
                    Gender = "man",
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
                    AvatarUrl = "https://plus.unsplash.com/premium_photo-1690587673708-d6ba8a1579a5?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=758",
                    Gender = "vrouw",
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
                    AvatarUrl = "https://plus.unsplash.com/premium_photo-1708271598591-4a84ef3b8dde?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=870",
                    Gender = "vrouw",
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
                profile.UpdateHobbies(CreateHobbies(dbContext, HobbyType.Reading, HobbyType.BoardGames, HobbyType.Crafting));
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
            .SelectMany(u => u.Connections)
            .AnyAsync();

        if (hasConnections)
            return;

        foreach (var user in users)
        {
            await dbContext.Entry(user)
                .Collection<UserConnection>("Connections")
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
            new(), // profielKlikTest
            new(), //profielklikgrouptest
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
        var profielKliktest = chats[4];
        var profielKlikGroupTest = chats[5];

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

        profielKliktest.AddUser(users["Thibo"]);
        profielKliktest.AddUser(users["Kyandro"]);
        
        profielKliktest.AddTextMessage("Klik op mijn profiel!", users["Kyandro"]);
        profielKliktest.AddTextMessage("Doe het", users["Kyandro"]);
        
        profielKlikGroupTest.AddUser(users["Thibo"]);
        profielKlikGroupTest.AddUser(users["Kyandro"]);
        profielKlikGroupTest.AddUser(users["Bjorn"]);
        profielKlikGroupTest.AddUser(users["Jasper"]);
        
        profielKlikGroupTest.AddTextMessage("Dit is een groupschat", users["Kyandro"]);
        profielKlikGroupTest.AddTextMessage("Met alle 4", users["Thibo"]);
        profielKlikGroupTest.AddTextMessage("Iedereen kan hier een bericht in sturen.", users["Bjorn"]);
        profielKlikGroupTest.AddTextMessage("Iedereen kan hier een bericht in sturen.", users["Jasper"]);
        
        await dbContext.SaveChangesAsync();
    }

    private sealed record SeedAccount(IdentityUser Identity, string Role, ApplicationUser? Profile);
}
