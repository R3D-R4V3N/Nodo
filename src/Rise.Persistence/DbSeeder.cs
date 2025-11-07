using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Sentiment;
using Rise.Domain.Users.Settings;
using Rise.Shared.Identity;
using Rise.Shared.Users;
using System;
using System.Collections.Generic;
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
        await OrganizationsAsync();
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

    private async Task OrganizationsAsync()
    {
        var desiredOrganizations = new List<Organization>
        {
            new("Dagcentrum Atlas", "Begeleidt jongeren dagelijks richting zelfstandigheid."),
            new("Begeleidingsdienst Horizon", "Biedt begeleiding op maat voor jongvolwassenen."),
            new("Steunpunt Noord", "Organisatie met focus op creatieve en sociale trajecten."),
        };

        var existingNames = await dbContext.Organizations
            .Select(o => o.Name)
            .ToListAsync();

        var toAdd = desiredOrganizations
            .Where(o => !existingNames.Contains(o.Name))
            .ToList();

        if (toAdd.Count == 0)
        {
            return;
        }

        dbContext.Organizations.AddRange(toAdd);
        await dbContext.SaveChangesAsync();
    }

    private async Task UsersAsync()
    {
        if (dbContext.Users.Any())
        {
            return;
        }

        await dbContext.Roles.ToListAsync();

        IEnumerable<UserSentiment> CreateSentiments()
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

        var organizations = await dbContext.Organizations.ToListAsync();

        Organization GetOrganization(string name) => organizations
            .Single(o => o.Name == name);

        var atlas = GetOrganization("Dagcentrum Atlas");
        var horizon = GetOrganization("Begeleidingsdienst Horizon");
        var noord = GetOrganization("Steunpunt Noord");

        Supervisor CreateSupervisorProfile(IdentityUser account, string firstName, string lastName, string biography, string avatarUrl, DateOnly birthDay, Organization organization)
        {
            var profile = new Supervisor
            {
                AccountId = account.Id,
                FirstName = FirstName.Create(firstName),
                LastName = LastName.Create(lastName),
                Biography = Biography.Create(biography),
                AvatarUrl = AvatarUrl.Create(avatarUrl),
                BirthDay = birthDay,
                Gender = GenderType.X,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                }
            };

            profile.AssignToOrganization(organization);

            return profile;
        }

        User CreateUserProfile(IdentityUser account, string firstName, string lastName, string biography, string avatarUrl, DateOnly birthDay, Organization organization, Supervisor supervisorProfile)
        {
            var profile = new User
            {
                AccountId = account.Id,
                FirstName = FirstName.Create(firstName),
                LastName = LastName.Create(lastName),
                Biography = Biography.Create(biography),
                AvatarUrl = AvatarUrl.Create(avatarUrl),
                BirthDay = birthDay,
                Gender = GenderType.X,
                UserSettings = new UserSetting
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                }
            };

            profile.AssignToOrganization(organization);
            var supervisorResult = profile.AssignSupervisor(supervisorProfile);
            if (!supervisorResult.IsSuccess)
            {
                throw new InvalidOperationException(string.Join("; ", supervisorResult.Errors));
            }

            return profile;
        }

        var supervisorProfile = CreateSupervisorProfile(
            supervisor,
            "Super",
            "Visor",
            "Here to help you.",
            "https://images.unsplash.com/photo-1761405378284-834f87bb9818?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            atlas);

        var supervisorEmmaProfile = CreateSupervisorProfile(
            supervisorEmma,
            "Emma",
            "Claes",
            "Coach voor dagelijkse structuur en zelfvertrouwen.",
            "https://images.unsplash.com/photo-1639149888905-fb39731f2e6c?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-35)),
            atlas);

        var supervisorJonasProfile = CreateSupervisorProfile(
            supervisorJonas,
            "Jonas",
            "Van Lint",
            "Helpt bij plannen en houdt wekelijks groepsmomenten.",
            "https://images.unsplash.com/photo-1544005313-94ddf0286df2?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MjZ8fGF2YXRhcnxlbnwwfHwwfHx8MA%3D%3D&auto=format&fit=crop&q=60&w=700",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-33)),
            horizon);

        var supervisorEllaProfile = CreateSupervisorProfile(
            supervisorElla,
            "Ella",
            "Vervoort",
            "Creatieve begeleider voor beeldende therapie.",
            "https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-31)),
            noord);

        var johnProfile = CreateUserProfile(
            userAccount1,
            "John",
            "Doe",
            "Houdt van katten en rustige gesprekken.",
            "https://images.unsplash.com/photo-1499996860823-5214fcc65f8f?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=932",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
            atlas,
            supervisorEmmaProfile);

        var staceyProfile = CreateUserProfile(
            userAccount2,
            "Stacey",
            "Willington",
            "Deelt graag verhalen over haar hulphond.",
            "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-26)),
            horizon,
            supervisorJonasProfile);

        var noorProfile = CreateUserProfile(
            chatterNoor,
            "Noor",
            "Vermeulen",
            "Praat graag over muziek en wil nieuwe vrienden maken.",
            "https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
            atlas,
            supervisorEmmaProfile);

        var milanProfile = CreateUserProfile(
            chatterMilan,
            "Milan",
            "Peeters",
            "Zoekt iemand om samen over games te praten.",
            "https://images.unsplash.com/photo-1580489944761-15a19d654956?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=922",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
            atlas,
            supervisorProfile);

        var linaProfile = CreateUserProfile(
            chatterLina,
            "Lina",
            "Jacobs",
            "Vindt het fijn om vragen te kunnen stellen in een veilige omgeving.",
            "https://plus.unsplash.com/premium_photo-1687832254672-bf177d8819df?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
            atlas,
            supervisorProfile);

        var kyandroProfile = CreateUserProfile(
            chatterKyandro,
            "Kyandro",
            "Voet",
            "Helpt vaak bij technische vragen en deelt programmeertips.",
            "https://plus.unsplash.com/premium_photo-1664536392896-cd1743f9c02c?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            horizon,
            supervisorJonasProfile);

        var jasperProfile = CreateUserProfile(
            chatterJasper,
            "Jasper",
            "Vermeersch",
            "Vindt het leuk om te discussiëren over technologie en innovatie.",
            "https://plus.unsplash.com/premium_photo-1664536392896-cd1743f9c02c?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
            horizon,
            supervisorJonasProfile);

        var bjornProfile = CreateUserProfile(
            chatterBjorn,
            "Bjorn",
            "Van Damme",
            "Praat graag over sport en houdt van teamwork.",
            "https://images.unsplash.com/photo-1704726135027-9c6f034cfa41?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=770",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-27)),
            atlas,
            supervisorProfile);

        var thiboProfile = CreateUserProfile(
            chatterThibo,
            "Thibo",
            "De Smet",
            "Is nieuwsgierig en stelt vaak interessante vragen.",
            "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-21)),
            atlas,
            supervisorEmmaProfile);

        var saarProfile = CreateUserProfile(
            chatterSaar,
            "Saar",
            "Vandenberg",
            "Deelt graag foto's van haar tekeningen.",
            "https://images.unsplash.com/photo-1704726135027-9c6f034cfa41?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=770",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
            noord,
            supervisorEllaProfile);

        var yassinProfile = CreateUserProfile(
            chatterYassin,
            "Yassin",
            "El Amrani",
            "Leert zelfstandig koken en zoekt tips van vrienden.",
            "https://plus.unsplash.com/premium_photo-1690587673708-d6ba8a1579a5?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=758",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            horizon,
            supervisorJonasProfile);

        var lotteProfile = CreateUserProfile(
            chatterLotte,
            "Lotte",
            "De Wilde",
            "Wordt blij van dansen en deelt positieve boodschappen.",
            "https://plus.unsplash.com/premium_photo-1708271598591-4a84ef3b8dde?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=870",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
            noord,
            supervisorEllaProfile);

        var aminaProfile = CreateUserProfile(
            chatterAmina,
            "Amina",
            "Karim",
            "Houdt van creatieve projecten en begeleidt graag groepsspelletjes.",
            "https://plus.unsplash.com/premium_photo-1708271598591-4a84ef3b8dde?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=870",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
            horizon,
            supervisorJonasProfile);

        var accounts = new List<SeedAccount>
        {
            new(admin, AppRoles.Administrator, null),
            new(supervisor, AppRoles.Supervisor, supervisorProfile),
            new(userAccount1, AppRoles.User, johnProfile),
            new(userAccount2, AppRoles.User, staceyProfile),
            new(nodoAdmin, AppRoles.Administrator, null),
            new(supervisorEmma, AppRoles.Supervisor, supervisorEmmaProfile),
            new(supervisorJonas, AppRoles.Supervisor, supervisorJonasProfile),
            new(supervisorElla, AppRoles.Supervisor, supervisorEllaProfile),
            new(chatterNoor, AppRoles.User, noorProfile),
            new(chatterMilan, AppRoles.User, milanProfile),
            new(chatterLina, AppRoles.User, linaProfile),
            new(chatterKyandro, AppRoles.User, kyandroProfile),
            new(chatterJasper, AppRoles.User, jasperProfile),
            new(chatterBjorn, AppRoles.User, bjornProfile),
            new(chatterThibo, AppRoles.User, thiboProfile),
            new(chatterSaar, AppRoles.User, saarProfile),
            new(chatterYassin, AppRoles.User, yassinProfile),
            new(chatterLotte, AppRoles.User, lotteProfile),
            new(chatterAmina, AppRoles.User, aminaProfile),
        };

        foreach (var (identity, role, profile) in accounts)
        {
            await userManager.CreateAsync(identity, PasswordDefault);
            await userManager.AddToRoleAsync(identity, role);

            if (profile is not null)
            {
                profile.UserSettings.AddChatTextLine("Kowabunga!");
                profile.UserSettings.AddChatTextLine("Hallo hoe gaat het?");
                if (profile is User _user)
                {
                    _user.UpdateSentiments(CreateSentiments());
                    _user.UpdateHobbies(CreateHobbies(dbContext, HobbyType.Reading, HobbyType.BoardGames, HobbyType.Crafting));
                    dbContext.Users.Add(_user);
                }
                else if (profile is Supervisor _supervisor)
                {
                    dbContext.Supervisors.Add(_supervisor);
                }
            }
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

        var noor = users.GetUser("Noor");
        var milan = users.GetUser("Milan");
        var lina = users.GetUser("Lina");
        var kyandro = users.GetUser("Kyandro");
        var jasper = users.GetUser("Jasper");
        var bjorn = users.GetUser("Bjorn");
        var thibo = users.GetUser("Thibo");
        var saar = users.GetUser("Saar");
        var yassin = users.GetUser("Yassin");
        var lotte = users.GetUser("Lotte");
        var amina = users.GetUser("Amina");
        var john = users.GetUser("John");
        var stacey = users.GetUser("Stacey");

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

        static void MakeFriends(User userA, User userB)
        {
            userA.SendFriendRequest(userB);
            userB.AcceptFriendRequest(userA);
        }

        static void SendFriendRequest(User requester, User receiver)
        {
            requester.SendFriendRequest(receiver);
        }
    }

    private async Task ChatsAsync()
    {
        if (dbContext.Chats.Any())
        {
            return;
        }

        var users = await dbContext
            .Users
            .Include(u => u.Connections)
            .ToListAsync();

        if (users.Count == 0)
            return;

        var noor = users.GetUser("Noor");
        var milan = users.GetUser("Milan");
        var lina = users.GetUser("Lina");
        var kyandro = users.GetUser("Kyandro");
        var jasper = users.GetUser("Jasper");
        var bjorn = users.GetUser("Bjorn");
        var thibo = users.GetUser("Thibo");
        var saar = users.GetUser("Saar");
        var yassin = users.GetUser("Yassin");
        var lotte = users.GetUser("Lotte");
        var amina = users.GetUser("Amina");
        var john = users.GetUser("John");
        var stacey = users.GetUser("Stacey");

        var chatsToCreate = new List<Chat>
        {
            Chat.CreateChat(noor, milan), // individueleCheckIn
            Chat.CreateChat(kyandro, jasper), // vrijdagGroep
            Chat.CreateChat(bjorn, thibo), // creatieveHoek
            Chat.CreateChat(lotte, amina), // technischeHulp
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

        var individueleCheckIn = chats[0];
        var vrijdagGroep = chats[1];
        var creatieveHoek = chats[2];
        var technischeHulp = chats[3];

        individueleCheckIn.AddTextMessage("Hoi Emma, ik ben een beetje zenuwachtig voor morgen.", individueleCheckIn.RandomUser());
        individueleCheckIn.AddTextMessage("Dat begrijp ik Noor, we bekijken samen hoe je het rustig kunt aanpakken.", individueleCheckIn.RandomUser());
        individueleCheckIn.AddTextMessage("Zal ik straks mijn checklist nog eens doornemen?", individueleCheckIn.RandomUser());
        individueleCheckIn.AddTextMessage("Ja, en ik stuur je zo meteen een ademhalingsoefening.", individueleCheckIn.RandomUser());

        vrijdagGroep.AddTextMessage("Wie doet er vrijdag mee met de online game-avond?", vrijdagGroep.RandomUser());
        vrijdagGroep.AddTextMessage("Ik! Zal ik snacks klaarzetten?", vrijdagGroep.RandomUser());
        vrijdagGroep.AddTextMessage("Wie doet er vrijdag mee met de online game-avond?", vrijdagGroep.RandomUser());
        vrijdagGroep.AddTextMessage("Ik plan een korte check-in zodat iedereen zich welkom voelt.", vrijdagGroep.RandomUser());

        creatieveHoek.AddTextMessage("Ik heb een nieuw schilderij gemaakt met felle kleuren!", creatieveHoek.RandomUser());
        creatieveHoek.AddTextMessage("Oh wauw, kan je een foto delen?", creatieveHoek.RandomUser());
        creatieveHoek.AddTextMessage("Zeker! En misschien kunnen we volgende keer een collagemiddag houden?", creatieveHoek.RandomUser());
        creatieveHoek.AddTextMessage("Topidee, ik zorg voor een stappenplan met eenvoudige materialen.", creatieveHoek.RandomUser());

        technischeHulp.AddTextMessage("Mijn tablet doet raar wanneer ik de spraakopnames open.", technischeHulp.RandomUser());
        technischeHulp.AddTextMessage("Heb je al geprobeerd om de app even opnieuw te starten?", technischeHulp.RandomUser());
        technischeHulp.AddTextMessage("Ja, maar ik twijfel of ik iets fout doe.", technischeHulp.RandomUser());
        technischeHulp.AddTextMessage("Ik kijk straks met je mee en stuur een korte handleiding door.", technischeHulp.RandomUser());


        await dbContext.SaveChangesAsync();
    }

    private sealed record SeedAccount(IdentityUser Identity, string Role, BaseUser? Profile);
}

internal static class DbSeederExtensions
{
    public static BaseUser RandomUser(this Chat chat) => chat.Users.Count == 0
        ? throw new Exception()
        : chat.Users[new Random().Next(0, chat.Users.Count)];
    public static User GetUser(this List<User> users, string firstName)
        => users.First(u => u.FirstName.Value.Equals(firstName, StringComparison.Ordinal));
}