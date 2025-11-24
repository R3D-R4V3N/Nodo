using Rise.Domain.Users;
using Rise.Domain.Users.Hobbys;

namespace Rise.Persistence.Configurations.Users.Hobbies;
public class UserHobbyJoin
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int HobbyId { get; set; }
    public UserHobby Hobby { get; set; } = null!;
}
