using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Rise.Domain.Chats;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Shared.Identity;
using Rise.Domain.Organizations.Properties;

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
    
    private async Task UsersAsync()
    {
        if (dbContext.Users.Any())
        {
            return;
        }

        await dbContext.Roles.ToListAsync();

        Organization nodoAntwerpen;
        Organization nodoGent;
        Organization nodoBrussel;

        if (await dbContext.Organizations.AnyAsync())
        {
            var organizations = await dbContext.Organizations.ToListAsync();

            nodoAntwerpen = organizations.Single(o => o.Name.Value == "Nodo Antwerpen");
            nodoGent = organizations.Single(o => o.Name.Value == "Nodo Gent");
            nodoBrussel = organizations.Single(o => o.Name.Value == "Nodo Brussel");
        }
        else
        {
            nodoAntwerpen = new Organization
            {
                Name = (OrganizationName)"Nodo Antwerpen",
                Location = new OrganizationLocation(
                    name: "Antwerpen",
                    zipCode: "2000",
                    city: "Antwerpen")
            };

            nodoGent = new Organization
            {
                Name = (OrganizationName)"Nodo Gent",
                Location = new OrganizationLocation(
                    name: "Gent",
                    zipCode: "9000",
                    city: "Gent")
            };

            nodoBrussel = new Organization
            {
                Name = (OrganizationName)"Nodo Brussel",
                Location = new OrganizationLocation(
                    name: "Brussel",
                    zipCode: "1000",
                    city: "Brussel")
            };

            dbContext.Organizations.AddRange(nodoAntwerpen, nodoGent, nodoBrussel);
        }

        Organization[] organizationPool =
        [
            nodoAntwerpen,
            nodoGent,
            nodoBrussel
        ];

        var organizationIndex = 0;
        Organization NextOrganization() => organizationPool[organizationIndex++ % organizationPool.Length];

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
                new Supervisor()
                {
                    AccountId = supervisor.Id,
                    FirstName = FirstName.Create("Super"),
                    LastName = LastName.Create("Visor"),
                    Biography = Biography.Create("Here to help you."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                    Organization = nodoAntwerpen,
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(userAccount1, AppRoles.User,
                new User()
                {
                    AccountId = userAccount1.Id,
                    FirstName = FirstName.Create("John"),
                    LastName = LastName.Create("Doe"),
                    Biography = Biography.Create("Houdt van katten en rustige gesprekken."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(userAccount2, AppRoles.User,
                new User()
                {
                    AccountId = userAccount2.Id,
                    FirstName = FirstName.Create("Stacey"),
                    LastName = LastName.Create("Willington"),
                    Biography = Biography.Create("Deelt graag verhalen over haar hulphond."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1544723795-3fb6469f5b39?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-26)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(nodoAdmin, AppRoles.Administrator, null),
            new(supervisorEmma, AppRoles.Supervisor,
                new Supervisor()
                {
                    AccountId = supervisorEmma.Id,
                    FirstName = FirstName.Create("Emma"),
                    LastName = LastName.Create("Claes"),
                    Biography = Biography.Create("Coach voor dagelijkse structuur en zelfvertrouwen."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-35)),
                    Organization = nodoGent,
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(supervisorJonas, AppRoles.Supervisor,
                new Supervisor()
                {
                    AccountId = supervisorJonas.Id,
                    FirstName = FirstName.Create("Jonas"),
                    LastName =  LastName.Create("Van Lint"),
                    Biography = Biography.Create("Helpt bij plannen en houdt wekelijks groepsmomenten."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1531891437562-4301cf35b7e4?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-33)),
                    Organization = nodoAntwerpen,
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(supervisorElla, AppRoles.Supervisor,
                new Supervisor()
                {
                    AccountId = supervisorElla.Id,
                    FirstName = FirstName.Create("Ella"),
                    LastName =  LastName.Create("Vervoort"),
                    Biography = Biography.Create("Creatieve begeleider voor beeldende therapie."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-31)),
                    Organization = nodoBrussel,
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterNoor, AppRoles.User,
                new User()
                {
                    AccountId = chatterNoor.Id,
                    FirstName = FirstName.Create("Noor"),
                    LastName = LastName.Create("Vermeulen"),
                    Biography = Biography.Create("Praat graag over muziek en wil nieuwe vrienden maken."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterMilan, AppRoles.User,
                new User()
                {
                    AccountId = chatterMilan.Id,
                    FirstName = FirstName.Create("Milan"),
                    LastName = LastName.Create("Peeters"),
                    Biography = Biography.Create("Zoekt iemand om samen over games te praten."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterLina, AppRoles.User,
                new User()
                {
                    AccountId = chatterLina.Id,
                    FirstName = FirstName.Create("Lina"),
                    LastName = LastName.Create("Jacobs"),
                    Biography = Biography.Create("Vindt het fijn om vragen te kunnen stellen in een veilige omgeving."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1760733345250-6b2625fca116?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterKyandro, AppRoles.User,
                new User()
                {
                    AccountId = chatterKyandro.Id,
                    FirstName = FirstName.Create("Kyandro"),
                    LastName = LastName.Create("Voet"),
                    Biography = Biography.Create("Helpt vaak bij technische vragen en deelt programmeertips."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1760681555543-0a3c65fa10eb?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterJasper, AppRoles.User,
                new User()
                {
                    AccountId = chatterJasper.Id,
                    FirstName = FirstName.Create("Jasper"),
                    LastName = LastName.Create("Vermeersch"),
                    Biography = Biography.Create("Vindt het leuk om te discussiëren over technologie en innovatie."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1760625525477-f725e48f5a13?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterBjorn, AppRoles.User,
                new User()
                {
                    AccountId = chatterBjorn.Id,
                    FirstName = FirstName.Create("Bjorn"),
                    LastName = LastName.Create("Van Damme"),
                    Biography = Biography.Create("Praat graag over sport en houdt van teamwork."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1749521166410-9031d6ded805?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-27)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterThibo, AppRoles.User,
                new User()
                {
                    AccountId = chatterThibo.Id,
                    FirstName = FirstName.Create("Thibo"),
                    LastName = LastName.Create("De Smet"),
                    Biography = Biography.Create("Is nieuwsgierig en stelt vaak interessante vragen."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1760604278004-91a4d7b22447?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-21)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterSaar, AppRoles.User,
                new User()
                {
                    AccountId = chatterSaar.Id,
                    FirstName = FirstName.Create("Saar"),
                    LastName = LastName.Create("Vandenberg"),
                    Biography = Biography.Create("Deelt graag foto's van haar tekeningen."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1760497925596-a6462350c583?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterYassin, AppRoles.User,
                new User()
                {
                    AccountId = chatterYassin.Id,
                    FirstName = FirstName.Create("Yassin"),
                    LastName = LastName.Create("El Amrani"),
                    Biography = Biography.Create("Leert zelfstandig koken en zoekt tips van vrienden."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1760411069721-60d7c378b697?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterLotte, AppRoles.User,
                new User()
                {
                    AccountId = chatterLotte.Id,
                    FirstName = FirstName.Create("Lotte"),
                    LastName = LastName.Create("De Wilde"),
                    Biography = Biography.Create("Wordt blij van dansen en deelt positieve boodschappen."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1760086741328-c56df17e8272?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
            new(chatterAmina, AppRoles.User,
                new User()
                {
                    AccountId = chatterAmina.Id,
                    FirstName = FirstName.Create("Amina"),
                    LastName = LastName.Create("Karim"),
                    Biography = Biography.Create("Houdt van creatieve projecten en begeleidt graag groepsspelletjes."),
                    AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1739889399693-8a46b389473f?auto=format&fit=facearea&facepad=2.5&w=200&h=200&q=80"),
                    BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
                    Organization = NextOrganization(),
                    UserSettings = new UserSetting()
                    {
                        FontSize = FontSize.Create(12),
                        IsDarkMode = false,
                    }
                }),
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
        var users = await dbContext.Users
            .ToListAsync();

        if (users.Count == 0)
            return;

        var hasConnections = await dbContext.Users
            .SelectMany(u => u.Connections)
            .AnyAsync();

        if (hasConnections)
            return;

        foreach (var user in users)
        {
            await dbContext.Entry(user)
                .Collection(u => u.Connections)
                .LoadAsync();
        }

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
            userA.AddFriend(userB);
            userB.AddFriend(userA);
        }

        static void SendFriendRequest(User requester, User receiver)
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

        var users = await dbContext
            .Users
            .ToListAsync();

        if (users.Count == 0)
            return;

        foreach (var user in users)
        {
            await dbContext.Entry(user)
                .Collection(u => u.Connections)
                .LoadAsync();
        }

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
        => users.Single(u => u.FirstName.Value.Equals(firstName, StringComparison.Ordinal));
}