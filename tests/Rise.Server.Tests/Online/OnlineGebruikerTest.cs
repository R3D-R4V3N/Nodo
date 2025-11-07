using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Rise.Server.Hubs;
using Xunit;

namespace Rise.Server.Tests.Online;

public class OnlineGebruikerTest
{
    private Chathub CreateHubWithMocks(out Mock<IHubCallerClients> clientsMock, out Mock<HubCallerContext> contextMock)
    {
        clientsMock = new Mock<IHubCallerClients>();
        contextMock = new Mock<HubCallerContext>();

        var hub = new Chathub
        {
            Clients = clientsMock.Object,
            Context = contextMock.Object
        };

        return hub;
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldAddUserToOnlineUsers_AndNotifyClients()
    {
        // Reset static dictionary
        Chathub.ResetOnlineUsers();

        // Arrange
        var clientProxyMock = new Mock<IClientProxy>();
        var clientsMock = new Mock<IHubCallerClients>();
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

        var contextMock = new Mock<HubCallerContext>();
        contextMock.Setup(c => c.UserIdentifier).Returns("user1");

        var hub = new Chathub
        {
            Clients = clientsMock.Object,
            Context = contextMock.Object
        };

        // Act
        await hub.OnConnectedAsync();

        // Assert
        var onlineUsers = await hub.GetOnlineUsers();
        Assert.Contains("user1", onlineUsers);

        clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "UserStatusChanged",
                It.Is<object[]>(o => (string)o[0] == "user1" && (bool)o[1] == true),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldRemoveUserFromOnlineUsers_WhenLastConnection()
    {
        // Reset static dictionary
        Chathub.ResetOnlineUsers();

        // Arrange
        var clientProxyMock = new Mock<IClientProxy>();
        var clientsMock = new Mock<IHubCallerClients>();
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

        var contextMock = new Mock<HubCallerContext>();
        contextMock.Setup(c => c.UserIdentifier).Returns("user1");

        var hub = new Chathub
        {
            Clients = clientsMock.Object,
            Context = contextMock.Object
        };

        // Voeg de gebruiker handmatig toe
        await hub.OnConnectedAsync();

        // Act
        await hub.OnDisconnectedAsync(null);

        // Assert
        var onlineUsers = await hub.GetOnlineUsers();
        Assert.DoesNotContain("user1", onlineUsers);

        clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "UserStatusChanged",
                It.Is<object[]>(o => (string)o[0] == "user1" && (bool)o[1] == false),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetOnlineUsers_ShouldReturnAllConnectedUsers()
    {
        // Reset static dictionary
        Chathub.ResetOnlineUsers();

        // Arrange
        var hub = CreateHubWithMocks(out var clientsMock, out var contextMock);
        var clientProxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

        // Voeg meerdere gebruikers toe
        contextMock.Setup(c => c.UserIdentifier).Returns("user1");
        await hub.OnConnectedAsync();

        contextMock.Setup(c => c.UserIdentifier).Returns("user2");
        await hub.OnConnectedAsync();

        // Act
        var onlineUsers = await hub.GetOnlineUsers();

        // Assert
        Assert.Contains("user1", onlineUsers);
        Assert.Contains("user2", onlineUsers);
        Assert.Equal(2, onlineUsers.Count);
    }
}
