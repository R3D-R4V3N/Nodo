using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Users
{
    public class ApplicationUser : Entity
    {
        /// <summary>
        /// Link to the <see cref="IdentityUser"/> account, so a Technician HAS A Account and not IS A <see cref="IdentityUser"/>./>
        /// </summary>
        public string AccountId { get; private set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Biography { get; set; }
        public required UserType UserType { get; set; }


        //// connections
        private readonly HashSet<ApplicationUser> friends = [];
        public IReadOnlyCollection<ApplicationUser> Friends => friends;

        private readonly HashSet<ApplicationUser> friendRequests = [];
        public IReadOnlyCollection<ApplicationUser> FriendRequests => friendRequests;

        //private readonly HashSet<ApplicationUser> blockedUsers = [];
        //public IReadOnlyList<ApplicationUser> BlockedUsers => blockedUsers.ToList().AsReadOnly();

        public ApplicationUser()
        {
        }

        public Result AddFriend(ApplicationUser friend)
        {
            if (!friendRequests.Contains(friend))
                return Result.Conflict($"Can't add {friend} without a request first");

            bool isAdded = friends.Add(friend);
            if (!isAdded)
                return Result.Conflict($"User is already friends with {friend}");

            friendRequests.Remove(friend);
            friend.friends.Add(this);

            return Result.Success();
        }

        public Result RemoveFriend(ApplicationUser friend)
        {
            bool isRemoved = friends.Remove(friend);
            if (!isRemoved)
                return Result.Conflict($"User wasn't friends with {friend}");

            friend.friends.Remove(this);

            return Result.Success();
        }
    }
}
