using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Users;
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
        
        var Kyandro = new IdentityUser
        {
            UserName = "kyandro@nodo.chat",
            Email = "kyandro@nodo.chat",
            EmailConfirmed = true,
        };
        
        await userManager.CreateAsync(Kyandro, PasswordDefault);
        
        var Jasper = new IdentityUser
        {
            UserName = "jasper@nodo.chat",
            Email = "jasper@nodo.chat",
            EmailConfirmed = true,
        };
        
        await userManager.CreateAsync(Jasper, PasswordDefault);
        
        var Bjorn = new IdentityUser
        {
            UserName = "bjorn@nodo.chat",
            Email = "bjorn@nodo.chat",
            EmailConfirmed = true,
        };
        
        await userManager.CreateAsync(Bjorn, PasswordDefault);
        
        var Thibo = new IdentityUser
        {
            UserName = "thibo@nodo.chat",
            Email = "thibo@nodo.chat",
            EmailConfirmed = true,
        };
        
        await userManager.CreateAsync(Thibo, PasswordDefault);
        
        var admin = new IdentityUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(admin, PasswordDefault);
        
        var supervisor = new IdentityUser
        {
            UserName = "supervisor@example.com",
            Email = "supervisor@example.com",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(supervisor, PasswordDefault);
        
        var userAccount1 = new IdentityUser
        {
            UserName = "user1@example.com",
            Email = "user1@example.com",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(userAccount1, PasswordDefault);
        
        var userAccount2 = new IdentityUser
        {
            UserName = "user2@example.com",
            Email = "user2@example.com",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(userAccount2, PasswordDefault);
                
        var user = new IdentityUser
        {
            UserName = "admin@nodo.chat",
            Email = "admin@nodo.chat",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(user, PasswordDefault);
        
        await userManager.AddToRoleAsync(admin, "Administrator");
        await userManager.AddToRoleAsync(supervisor, "Supervisor");
        await userManager.AddToRoleAsync(userAccount1, "User");
        await userManager.AddToRoleAsync(userAccount2, "User");

        dbContext.ApplicationUsers.AddRange(
            new ApplicationUser(userAccount1.Id)
            {
                FirstName = "John",
                LastName = "Doe",
                Biography = "I like cats, meow.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
            new ApplicationUser(userAccount2.Id)
            {
                FirstName = "Stacey",
                LastName = "Willington",
                Biography = "I like dogs, ruff.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
            new ApplicationUser(supervisor.Id)
            {
                FirstName = "Super",
                LastName = "Visor",
                Biography = "Here to help you.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                UserType = UserType.Supervisor,
            }
        );

        var supervisorEmma = new IdentityUser
        {
            UserName = "emma.supervisor@nodo.chat",
            Email = "emma.supervisor@nodo.chat",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(supervisorEmma, PasswordDefault);

        var supervisorJonas = new IdentityUser
        {
            UserName = "jonas.supervisor@nodo.chat",
            Email = "jonas.supervisor@nodo.chat",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(supervisorJonas, PasswordDefault);

        var chatterNoor = new IdentityUser
        {
            UserName = "noor@nodo.chat",
            Email = "noor@nodo.chat",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(chatterNoor, PasswordDefault);

        var chatterMilan = new IdentityUser
        {
            UserName = "milan@nodo.chat",
            Email = "milan@nodo.chat",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(chatterMilan, PasswordDefault);

        var chatterLina = new IdentityUser
        {
            UserName = "lina@nodo.chat",
            Email = "lina@nodo.chat",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(chatterLina, PasswordDefault);

        await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
        await userManager.AddToRoleAsync(supervisorEmma, AppRoles.Supervisor);
        await userManager.AddToRoleAsync(supervisorJonas, AppRoles.Supervisor);
        await userManager.AddToRoleAsync(chatterNoor, AppRoles.User);
        await userManager.AddToRoleAsync(chatterMilan, AppRoles.User);
        await userManager.AddToRoleAsync(chatterLina, AppRoles.User);
        await userManager.AddToRoleAsync(Kyandro, AppRoles.User);
        await userManager.AddToRoleAsync(Jasper, AppRoles.User);
        await userManager.AddToRoleAsync(Bjorn, AppRoles.User);
        await userManager.AddToRoleAsync(Thibo, AppRoles.User);



        var applicationUsers = new List<ApplicationUser>
        {
            new ApplicationUser(chatterNoor.Id)
            {
                FirstName = "Noor",
                LastName = "Vermeulen",
                Biography = "Praat graag over muziek en wil nieuwe vrienden maken.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
            new ApplicationUser(chatterMilan.Id)
            {
                FirstName = "Milan",
                LastName = "Peeters",
                Biography = "Zoekt iemand om samen over games te praten.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
            new ApplicationUser(chatterLina.Id)
            {
                FirstName = "Lina",
                LastName = "Jacobs",
                Biography = "Vindt het fijn om vragen te kunnen stellen in een veilige omgeving.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
            new ApplicationUser(Kyandro.Id)
            {
                FirstName = "Kyandro",
                LastName = "Voet",
                Biography = "Is geïnteresseerd in softwareontwikkeling en helpt vaak bij technische vragen.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
            new ApplicationUser(Jasper.Id)
            {
                FirstName = "Jasper",
                LastName = "Vermeersch",
                Biography = "Vindt het leuk om te discussiëren over technologie en innovatie.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
            new ApplicationUser(Bjorn.Id)
            {
                FirstName = "Bjorn",
                LastName = "Van Damme",
                Biography = "Praat graag over sport en houdt van teamwork.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
            new ApplicationUser(Thibo.Id)
            {
                FirstName = "Thibo",
                LastName = "De Smet",
                Biography = "Is nieuwsgierig en stelt vaak interessante vragen.",
                BirthDay = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                UserType = UserType.Regular,
            },
        };

        dbContext.ApplicationUsers.AddRange(applicationUsers);

        await dbContext.SaveChangesAsync();
    }

    private async Task ConnectionsAsync()
    {
        var users = await dbContext.ApplicationUsers.ToListAsync();

        if (users.Count == 0)
            return;

        if (users[0].Connections.Count > 0)
            return;

        users[0].AddFriend(users[1]);
        users[1].AddFriend(users[2]);
        users[2].AddFriend(users[1]);

        await dbContext.SaveChangesAsync();
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

        if (supervisors.Count == 0 || chatUsers.Count == 0)
        {
            return;
        }

        var firstChatter = chatUsers.First();
        var secondChatter = chatUsers.Skip(1).FirstOrDefault() ?? firstChatter;
        var primarySupervisor = supervisors.First();
        var backupSupervisor = supervisors.Skip(1).FirstOrDefault() ?? primarySupervisor;

        var chat1 = new Chat();
        var chat2 = new Chat();

        dbContext.Chats.AddRange(chat1, chat2);
        await dbContext.SaveChangesAsync();

        var messages = new List<Message>
        {
            new Message { Inhoud = "Hoi Emma, ik ben een beetje zenuwachtig voor morgen.", ChatId = chat1.Id, SenderId = firstChatter.Id },
            new Message { Inhoud = "Dat begrijp ik Noor, we bekijken samen hoe je het rustig kunt aanpakken.", ChatId = chat1.Id, SenderId = primarySupervisor.Id },
            new Message { Inhoud = "Ik heb vandaag een leuke foto van mijn hond gemaakt!", ChatId = chat2.Id, SenderId = secondChatter.Id },
            new Message { Inhoud = "Wat leuk! Wil je hem straks in de groepschat delen?", ChatId = chat2.Id, SenderId = backupSupervisor.Id },
        };

        dbContext.Messages.AddRange(messages);
        await dbContext.SaveChangesAsync();
    }
    
    private async Task MessagesAsync()
    {
        if (dbContext.Messages.Any())
        {
            return;
        }

        var supervisors = await dbContext.ApplicationUsers
            .Where(u => u.UserType == UserType.Supervisor)
            .ToListAsync();

        var chatUsers = await dbContext.ApplicationUsers
            .Where(u => u.UserType == UserType.Regular)
            .ToListAsync();

        if (!supervisors.Any() || !chatUsers.Any())
        {
            return;
        }

        var firstChatter = chatUsers.First();
        var secondChatter = chatUsers.Skip(1).FirstOrDefault() ?? firstChatter;
        var primarySupervisor = supervisors.First();
        var backupSupervisor = supervisors.Skip(1).FirstOrDefault() ?? primarySupervisor;


        var chats = await dbContext.Chats.ToListAsync();

        if (chats.Count == 0)
        {
            return;
        }


        var messages = new List<Message>
        {
            new Message { Inhoud = "Hoe voelde je je na het gesprek van gisteren?", ChatId = chats[0].Id, SenderId = primarySupervisor.Id },
            new Message { Inhoud = "Veel beter, bedankt om te luisteren!", ChatId = chats[0].Id, SenderId = firstChatter.Id },
            new Message { Inhoud = "Zullen we vrijdag samen online tekenen?", ChatId = chats[^1].Id, SenderId = secondChatter.Id },
            new Message { Inhoud = "Leuk idee! Ik stuur straks een uitnodiging.", ChatId = chats[^1].Id, SenderId = backupSupervisor.Id }
        };

        dbContext.Messages.AddRange(messages);
        await dbContext.SaveChangesAsync();
    }
}
