using Ardalis.GuardClauses;
using Ardalis.Result;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rise.Domain.Common;

namespace Rise.Domain.Users;

public class ApplicationUser : Entity
{
    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account so chatprofielen gekoppeld blijven aan hun login.
    /// </summary>
    public string AccountId { get; private set; }

        private string _firstName = string.Empty;
        public required string FirstName
        {
            get => _firstName;
            set => _firstName = Guard.Against.NullOrWhiteSpace(value);
        }
        private string _lastName = string.Empty;
        public required string LastName
        {
            get => _lastName;
            set => _lastName = Guard.Against.NullOrWhiteSpace(value);
        }
        private string _biography = string.Empty;
        public required string Biography
        {
            get => _biography;
            set => _biography = Guard.Against.NullOrWhiteSpace(value);
        }
        public required DateOnly BirthDay { get; set; }
        public required UserType UserType { get; set; }
    

        //// connections
        private readonly HashSet<UserConnection> _connections = [];
        public IReadOnlyCollection<UserConnection> Connections => _connections;
        public IEnumerable<UserConnection> Friends => _connections
            .Where(x => x.ConnectionType.Equals(UserConnectionType.Friend));
        public IEnumerable<UserConnection> FriendRequests => _connections
            .Where(x => 
                x.ConnectionType.Equals(UserConnectionType.RequestIncoming) 
                || x.ConnectionType.Equals(UserConnectionType.RequestOutgoing));
        public IEnumerable<UserConnection> BlockedUsers => _connections
            .Where(x => x.ConnectionType.Equals(UserConnectionType.Blocked));

        public ApplicationUser()
        {
        }

        public ApplicationUser(string accountId)
        {
            AccountId = Guard.Against.NullOrEmpty(accountId);
        }

        public Result<string> AddFriend(ApplicationUser friend)
        {
            bool isAdded = Friends
                .Any(x => x.Connection.Equals(friend));

            if (isAdded)
                return Result.Conflict($"User is already friends with {friend}");

            UserConnection? friendRequest = FriendRequests
                .FirstOrDefault(x => x.Connection.Equals(friend));

            if (friendRequest is null)
            {
                _connections.Add(
                    new UserConnection()
                    {
                        Connection = friend,
                        ConnectionType = UserConnectionType.RequestOutgoing
                    }
                );

                friend._connections.Add(
                    new UserConnection() 
                    { 
                        Connection = this,
                        ConnectionType = UserConnectionType.RequestIncoming 
                    }
                );

                return Result.Success($"User send a friend request to {friend}");
            }

            if (friendRequest.ConnectionType.Equals(UserConnectionType.RequestOutgoing))
            {
                return Result.Conflict($"User has already send a request to {friend}");
            }

            _connections.Add(new 
                UserConnection() 
                { 
                    Connection = friend, 
                    ConnectionType = UserConnectionType.Friend 
                }
            );

            _connections.Remove(
                new UserConnection()
                { 
                    Connection = friend, 
                    ConnectionType = UserConnectionType.RequestIncoming 
                }
            );

            friend._connections.Add(new
                UserConnection()
                {
                    Connection = this,
                    ConnectionType = UserConnectionType.Friend
                }
            );

            friend._connections.Remove(
                new UserConnection()
                {
                    Connection = this,
                    ConnectionType = UserConnectionType.RequestOutgoing
                }
            );

            return Result.Success($"User added {friend}");
        }

        public Result RemoveFriend(ApplicationUser friend)
        {
            _connections.Remove(
                new UserConnection()
                {
                    Connection = friend,
                    ConnectionType = UserConnectionType.Friend,
                }
            );

            friend._connections.Remove(
                new UserConnection()
                {
                    Connection = this,
                    ConnectionType = UserConnectionType.Friend
                }
            );

            return Result.Success();
        }
    }

