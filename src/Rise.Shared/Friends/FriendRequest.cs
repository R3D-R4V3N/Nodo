namespace Rise.Shared.Friends;

public static partial class FriendRequest
{
    public class Add
    {
        public int FriendId { get; set; }

        public class Validator : AbstractValidator<Add>
        {
            public Validator()
            {
                RuleFor(x => x.FriendId).GreaterThan(0);
            }
        }
    }

    public class Remove
    {
        public int FriendId { get; set; }

        public class Validator : AbstractValidator<Remove>
        {
            public Validator()
            {
                RuleFor(x => x.FriendId).GreaterThan(0);
            }
        }
    }
}
