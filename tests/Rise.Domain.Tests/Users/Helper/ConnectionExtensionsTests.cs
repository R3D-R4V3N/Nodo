using Rise.Domain.Helper;
using Rise.Domain.Users.Connections;
using Rise.Tests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Tests.Users.Helper;

public class ConnectionExtensionsTests
{
    [Theory]
    [MemberData(nameof(AllConnectionTypes))]
    public void CreateConnectionWith_ShouldSetPropertiesCorrectly(UserConnectionType type)
    {
        var user1 = DomainData.ValidUser(1);
        var user2 = DomainData.ValidUser(2);

        var connection = user1.CreateConnectionWith(user2, type);

        connection.From.ShouldBe(user1);
        connection.To.ShouldBe(user2);
        connection.ConnectionType.ShouldBe(type);
    }

    public static IEnumerable<object[]> AllConnectionTypes() =>
        Enum.GetValues<UserConnectionType>()
            .Select(t => new object[] { t });
}
