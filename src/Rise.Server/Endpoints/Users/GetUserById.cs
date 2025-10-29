using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class GetUserById(IUserService userService) : Endpoint<string, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Get("/api/users/{accountId}");
        AllowAnonymous();
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(string accountId, CancellationToken ct)
    {
        // normaal moet dit automatisch werken via de param van de methode, maar dit werkt niet dus wordt dit handmatig opgehaald
        // Dit kan getest worden door de code hieronder uit commentaar te halen en dan zie je dat 1/2 de waarden null is
        var accountid = Route<string>("accountId");
        
        
        // var previous = Console.ForegroundColor;
        // Console.ForegroundColor = ConsoleColor.Green;
        // Console.WriteLine( "AccountId: "+ accountId);
        // Console.WriteLine( "Accountid: "+ accountid);
        // Console.ForegroundColor = previous; // restore
        

        
        // if (string.IsNullOrWhiteSpace(accountId))
        //     return Task.FromResult(Result.Error<UserResponse.CurrentUser>("Ongeldig account ID."));
        
        //var accountId = Query<string>("accountId");
        
        
       
        return userService.GetUserAsync(accountid, ct);
    }
}