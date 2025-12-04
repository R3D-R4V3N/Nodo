using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Tests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Tests.Users.Connections;

public class UserConnectionTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var user1 = DomainData.ValidUser(1);
        var user2 = DomainData.ValidUser(2);
        var connectionType = UserConnectionType.Friend;
        var createdAt = DateTime.UtcNow;

        var connection = new UserConnection
        {
            From = user1,
            To = user2,
            ConnectionType = connectionType,
            CreatedAt = createdAt,
        };

        connection.From.ShouldBe(user1);
        connection.To.ShouldBe(user2);
        connection.ConnectionType.ShouldBe(connectionType);
        connection.CreatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void TwoConnections_WithSameValues_ShouldBeEqual()
    {
        var user1 = DomainData.ValidUser(1);
        var user2 = DomainData.ValidUser(2);

        var c1 = new UserConnection
        {
            From = user1,
            To = user2,
            ConnectionType = UserConnectionType.Friend
        };
        var c2 = new UserConnection
        {
            From = user1,
            To = user2,
            ConnectionType = UserConnectionType.Friend
        };

        c1.ShouldBe(c2);
        c1.Equals(c2).ShouldBeTrue();
        c1.GetHashCode().ShouldBe(c2.GetHashCode());
    }

    [Fact]
    public void TwoConnections_WithDifferentUser_ShouldNotBeEqual()
    {
        var c1 = new UserConnection
        {
            From = DomainData.ValidUser(1),
            To = DomainData.ValidUser(2),
            ConnectionType = UserConnectionType.Friend
        };
        var c2 = new UserConnection
        {
            From = DomainData.ValidUser(2),
            To = DomainData.ValidUser(1),
            ConnectionType = UserConnectionType.Friend
        };

        c1.ShouldNotBe(c2);
    }

    [Fact]
    public void TwoConnections_WithDifferentConnectionType_ShouldNotBeEqual()
    {
        var user1 = DomainData.ValidUser(1);
        var user2 = DomainData.ValidUser(2);

        var c1 = new UserConnection
        {
            From = user1,
            To = user2,
            ConnectionType = UserConnectionType.Friend
        };
        var c2 = new UserConnection
        {
            From = user1,
            To = user2,
            ConnectionType = UserConnectionType.Blocked
        };

        c1.ShouldNotBe(c2);
    }
}