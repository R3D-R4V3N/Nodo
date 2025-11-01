using Rise.Domain.Users.Connections;

namespace Rise.Domain.Tests.Users.Connections;

public class UserConnectionTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var user = TestData.ValidUser(1);
        var connectionType = UserConnectionType.Friend;
        var createdAt = DateTime.UtcNow;

        var connection = new UserConnection
        {
            Connection = user,
            ConnectionType = connectionType,
            CreatedAt = createdAt,
        };

        connection.Connection.ShouldBe(user);
        connection.ConnectionType.ShouldBe(connectionType);
        connection.CreatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void TwoConnections_WithSameValues_ShouldBeEqual()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(1);

        var c1 = new UserConnection
        {
            Connection = user1,
            ConnectionType = UserConnectionType.Friend
        };
        var c2 = new UserConnection
        {
            Connection = user2,
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
            Connection = TestData.ValidUser(1),
            ConnectionType = UserConnectionType.Friend
        };
        var c2 = new UserConnection
        {
            Connection = TestData.ValidUser(2),
            ConnectionType = UserConnectionType.Friend
        };

        c1.ShouldNotBe(c2);
    }

    [Fact]
    public void TwoConnections_WithDifferentConnectionType_ShouldNotBeEqual()
    {
        var user = TestData.ValidUser(1);

        var c1 = new UserConnection
        {
            Connection = user,
            ConnectionType = UserConnectionType.Friend
        };
        var c2 = new UserConnection
        {
            Connection = user,
            ConnectionType = UserConnectionType.Blocked
        };

        c1.ShouldNotBe(c2);
    }

    [Fact]
    public void CreatedAt_ShouldBeSetToUtcNow_ByDefault()
    {
        var before = DateTime.UtcNow;

        var connection = new UserConnection
        {
            Connection = TestData.ValidUser(1),
            ConnectionType = UserConnectionType.Friend
        };

        var after = DateTime.UtcNow;

        connection.CreatedAt.ShouldBeInRange(before, after);
    }
}