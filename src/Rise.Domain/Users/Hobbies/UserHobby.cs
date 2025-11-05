namespace Rise.Domain.Users.Hobbys;

public class UserHobby : Entity
{
    public required HobbyType Hobby { get; set; }
}
