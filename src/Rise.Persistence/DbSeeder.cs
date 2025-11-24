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
        if (dbContext.Organizations.Any())
        {
            return;
        }

        var organizations = new List<Organization>
        {
            new("Nodo Centrum", "Ondersteuning vanuit het centrale team."),
            new("Community Noord", "Samen sterker in regio Noord."),
            new("Community Zuid", "Creatieve ontmoetingsplek voor iedereen."),
        };

        dbContext.Organizations.AddRange(organizations);
        await dbContext.SaveChangesAsync();
    }

    private async Task UsersAsync()
    {
        if (dbContext.Users.Any())
        {
            return;
        }

        await dbContext.Roles.ToListAsync();

        var organizationsByName = await dbContext.Organizations
            .ToDictionaryAsync(o => o.Name, StringComparer.OrdinalIgnoreCase);

        Organization GetOrganization(string name)
            => organizationsByName.TryGetValue(name, out var organization)
                ? organization
                : throw new InvalidOperationException($"Organisatie '{name}' werd niet gevonden.");

        var nodoCentrum = GetOrganization("Nodo Centrum");
        var communityNoord = GetOrganization("Community Noord");
        var communityZuid = GetOrganization("Community Zuid");

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

        var accounts = new List<SeedAccount>
        {
            new("admin@example.com", AppRoles.Administrator, null),
            new("supervisor@example.com", AppRoles.Supervisor, accountId => new Supervisor()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Super"),
                LastName = LastName.Create("Visor"),
                Biography = Biography.Create("Here to help you."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1761405378284-834f87bb9818?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = nodoCentrum
            }),
            new("user1@example.com", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("John"),
                LastName = LastName.Create("Doe"),
                Biography = Biography.Create("Houdt van katten en rustige gesprekken."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1499996860823-5214fcc65f8f?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=932"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = nodoCentrum
            }),
            new("user2@example.com", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Stacey"),
                LastName = LastName.Create("Willington"),
                Biography = Biography.Create("Deelt graag verhalen over haar hulphond."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1524504388940-b1c1722653e1?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-26)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = nodoCentrum
            }),
            new("admin@nodo.chat", AppRoles.Administrator, null),
            new("emma.supervisor@nodo.chat", AppRoles.Supervisor, accountId => new Supervisor()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Emma"),
                LastName = LastName.Create("Claes"),
                Biography = Biography.Create("Coach voor dagelijkse structuur en zelfvertrouwen."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1639149888905-fb39731f2e6c?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-35)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityNoord
            }),
            new("jonas.supervisor@nodo.chat", AppRoles.Supervisor, accountId => new Supervisor()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Jonas"),
                LastName = LastName.Create("Van Lint"),
                Biography = Biography.Create("Helpt bij plannen en houdt wekelijks groepsmomenten."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1544005313-94ddf0286df2?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MjZ8fGF2YXRhcnxlbnwwfHwwfHx8MA%3D%3D&auto=format&fit=crop&q=60&w=700"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-33)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityNoord
            }),
            new("ella.supervisor@nodo.chat", AppRoles.Supervisor, accountId => new Supervisor()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Ella"),
                LastName = LastName.Create("Vervoort"),
                Biography = Biography.Create("Creatieve begeleider voor beeldende therapie."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-31)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityZuid
            }),
            new("noor@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Noor"),
                LastName = LastName.Create("Vermeulen"),
                Biography = Biography.Create("Praat graag over muziek en wil nieuwe vrienden maken."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=928"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityZuid
            }),
            new("milan@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Milan"),
                LastName = LastName.Create("Peeters"),
                Biography = Biography.Create("Zoekt iemand om samen over games te praten."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1580489944761-15a19d654956?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=922"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityZuid
            }),
            new("lina@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Lina"),
                LastName = LastName.Create("Jacobs"),
                Biography = Biography.Create("Vindt het fijn om vragen te kunnen stellen in een veilige omgeving."),
                AvatarUrl = AvatarUrl.Create("https://plus.unsplash.com/premium_photo-1687832254672-bf177d8819df?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = nodoCentrum
            }),
            new("kyandro@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Kyandro"),
                LastName = LastName.Create("Voet"),
                Biography = Biography.Create("Helpt vaak bij technische vragen en deelt programmeertips."),
                AvatarUrl = AvatarUrl.Create("https://plus.unsplash.com/premium_photo-1664536392896-cd1743f9c02c?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = nodoCentrum
            }),
            new("jasper@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Jasper"),
                LastName = LastName.Create("Vermeersch"),
                Biography = Biography.Create("Vindt het leuk om te discussiëren over technologie en innovatie."),
                AvatarUrl = AvatarUrl.Create("https://plus.unsplash.com/premium_photo-1664536392896-cd1743f9c02c?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = nodoCentrum
            }),
            new("bjorn@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Bjorn"),
                LastName = LastName.Create("Van Damme"),
                Biography = Biography.Create("Praat graag over sport en houdt van teamwork."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1704726135027-9c6f034cfa41?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=770"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-27)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityNoord
            }),
            new("thibo@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Thibo"),
                LastName = LastName.Create("De Smet"),
                Biography = Biography.Create("Is nieuwsgierig en stelt vaak interessante vragen."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=774"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-21)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityNoord
            }),
            new("saar@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Saar"),
                LastName = LastName.Create("Vandenberg"),
                Biography = Biography.Create("Deelt graag foto's van haar tekeningen."),
                AvatarUrl = AvatarUrl.Create("https://images.unsplash.com/photo-1704726135027-9c6f034cfa41?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=770"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-24)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityNoord
            }),
            new("yassin@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Yassin"),
                LastName = LastName.Create("El Amrani"),
                Biography = Biography.Create("Leert zelfstandig koken en zoekt tips van vrienden."),
                AvatarUrl = AvatarUrl.Create("https://plus.unsplash.com/premium_photo-1690587673708-d6ba8a1579a5?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=758"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityZuid
            }),
            new("lotte@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Lotte"),
                LastName = LastName.Create("De Wilde"),
                Biography = Biography.Create("Wordt blij van dansen en deelt positieve boodschappen."),
                AvatarUrl = AvatarUrl.Create("https://plus.unsplash.com/premium_photo-1708271598591-4a84ef3b8dde?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=870"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-23)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityZuid
            }),
            new("amina@nodo.chat", AppRoles.User, accountId => new User()
            {
                AccountId = accountId,
                FirstName = FirstName.Create("Amina"),
                LastName = LastName.Create("Karim"),
                Biography = Biography.Create("Houdt van creatieve projecten en begeleidt graag groepsspelletjes."),
                AvatarUrl = AvatarUrl.Create("https://plus.unsplash.com/premium_photo-1708271598591-4a84ef3b8dde?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=870"),
                BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-22)),
                Gender = GenderType.X,
                UserSettings = new UserSetting()
                {
                    FontSize = FontSize.Create(12),
                    IsDarkMode = false,
                },
                Organization = communityZuid
            }),
        };

        var existingProfiles = await dbContext.Set<BaseUser>()
            .AsNoTracking()
            .ToDictionaryAsync(u => u.AccountId);

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

            if (existingProfiles.ContainsKey(identity.Id))
            {
                continue;
            }

            var profile = account.ProfileFactory(identity.Id);
            if (profile is null)
            {
                continue;
            }

            profile.UserSettings.AddChatTextLine("Kowabunga!");
            profile.UserSettings.AddChatTextLine("Hallo hoe gaat het?");

            switch (profile)
            {
                case User userProfile:
                    userProfile.UpdateSentiments(CreateSentiments());
                    userProfile.UpdateHobbies(CreateHobbies(dbContext, HobbyType.Reading, HobbyType.BoardGames, HobbyType.Crafting));
                    dbContext.Users.Add(userProfile);
                    break;
                case Supervisor supervisorProfile:
                    dbContext.Supervisors.Add(supervisorProfile);
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