using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Shared.Identity;

namespace Rise.Persistence;
/// <summary>
/// Seeds the database
/// </summary>
/// <param name="dbContext"></param>
/// <param name="roleManager"></param>
/// <param name="userManager"></param>
public class DbSeeder(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
{
    const string PasswordDefault = "A1b2C3!";
    public async Task SeedAsync()
    {
        await RolesAsync();
        await UsersAsync();
        await ChatsAsync();
        await MessagesAsync();
    }

    private async Task RolesAsync()
    {
        if (dbContext.Roles.Any())
            return;

        await roleManager.CreateAsync(new IdentityRole(AppRoles.Administrator));
        await roleManager.CreateAsync(new IdentityRole(AppRoles.Supervisor));
        await roleManager.CreateAsync(new IdentityRole(AppRoles.ChatUser));
    }
    
    private async Task  UsersAsync()
    {
        if (dbContext.Users.Any())
            return;
        
        await dbContext.Roles.ToListAsync();

        var admin = new IdentityUser
        {
            UserName = "admin@chatapp.local",
            Email = "admin@chatapp.local",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(admin, PasswordDefault);

        var supervisorEmma = new IdentityUser
        {
            UserName = "emma.supervisor@chatapp.local",
            Email = "emma.supervisor@chatapp.local",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(supervisorEmma, PasswordDefault);

        var supervisorJonas = new IdentityUser
        {
            UserName = "jonas.supervisor@chatapp.local",
            Email = "jonas.supervisor@chatapp.local",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(supervisorJonas, PasswordDefault);

        var chatterNoor = new IdentityUser
        {
            UserName = "noor@chatapp.local",
            Email = "noor@chatapp.local",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(chatterNoor, PasswordDefault);

        var chatterMilan = new IdentityUser
        {
            UserName = "milan@chatapp.local",
            Email = "milan@chatapp.local",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(chatterMilan, PasswordDefault);

        var chatterLina = new IdentityUser
        {
            UserName = "lina@chatapp.local",
            Email = "lina@chatapp.local",
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(chatterLina, PasswordDefault);

        await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
        await userManager.AddToRoleAsync(supervisorEmma, AppRoles.Supervisor);
        await userManager.AddToRoleAsync(supervisorJonas, AppRoles.Supervisor);
        await userManager.AddToRoleAsync(chatterNoor, AppRoles.ChatUser);
        await userManager.AddToRoleAsync(chatterMilan, AppRoles.ChatUser);
        await userManager.AddToRoleAsync(chatterLina, AppRoles.ChatUser);

        var applicationUsers = new List<ApplicationUser>
        {
            new(supervisorEmma.Id, "Emma", "Begeleider", "Begeleider die jongeren ondersteunt tijdens het chatten.", UserType.Supervisor),
            new(supervisorJonas.Id, "Jonas", "Coach", "Houdt gesprekken in de gaten en helpt wanneer het even moeilijk wordt.", UserType.Supervisor),
            new(chatterNoor.Id, "Noor", "Vermeulen", "Praat graag over muziek en wil nieuwe vrienden maken.", UserType.Regular),
            new(chatterMilan.Id, "Milan", "Peeters", "Zoekt iemand om samen over games te praten.", UserType.Regular),
            new(chatterLina.Id, "Lina", "Jacobs", "Vindt het fijn om vragen te kunnen stellen in een veilige omgeving.", UserType.Regular)
        };

        dbContext.ApplicationUsers.AddRange(applicationUsers);

        await dbContext.SaveChangesAsync();
    }
    private async Task ChatsAsync()
    {
        if (dbContext.Chats.Any())
            return;

        var supervisors = await dbContext.ApplicationUsers
            .Where(u => u.UserType == UserType.Supervisor)
            .ToListAsync();

        var regularUsers = await dbContext.ApplicationUsers
            .Where(u => u.UserType == UserType.Regular)
            .ToListAsync();

        if (!supervisors.Any() || !regularUsers.Any())
            return;

        var firstChatter = regularUsers.First();
        var secondChatter = regularUsers.Skip(1).FirstOrDefault() ?? firstChatter;
        var primarySupervisor = supervisors.First();
        var backupSupervisor = supervisors.Skip(1).FirstOrDefault() ?? primarySupervisor;

        var chat1 = new Chat();
        var chat2 = new Chat();

        dbContext.Chats.AddRange(chat1, chat2);
        await dbContext.SaveChangesAsync();

        // voeg berichten toe
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
            return;

        var chats = await dbContext.Chats.ToListAsync();

        if (!chats.Any())
            return;

        var supervisors = await dbContext.ApplicationUsers
            .Where(u => u.UserType == UserType.Supervisor)
            .ToListAsync();

        var chatters = await dbContext.ApplicationUsers
            .Where(u => u.UserType == UserType.Regular)
            .ToListAsync();

        if (!supervisors.Any() || !chatters.Any())
            return;

        var firstChatter = chatters.First();
        var secondChatter = chatters.Skip(1).FirstOrDefault() ?? firstChatter;
        var primarySupervisor = supervisors.First();
        var backupSupervisor = supervisors.Skip(1).FirstOrDefault() ?? primarySupervisor;

        // voorbeeldberichten per chat
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