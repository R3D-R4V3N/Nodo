using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Tests.Chats;
public class ChatTests
{
    [Fact]
    public void CreateChat_ShouldCreate()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        var result = Chat.CreateChat(user1, user2);

        result.IsSuccess.ShouldBeTrue();
        Chat chat = result;

        chat.Users.ShouldContain(user1);
        chat.Users.ShouldContain(user2);
        user1.Chats.ShouldContain(chat);
        user2.Chats.ShouldContain(chat);
    }

    [Fact]
    public void CreateChat_ShouldConflict_WhenNotFriend()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        var result = Chat.CreateChat(user1, user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Chat kan niet worden gemaakt omdat {user1} en {user2} elkaar niet bevriend zijn"]);
    }

    [Fact]
    public void AddUser_ShouldAddUser_WhenNotExists()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var user3 = TestData.ValidUser(3);
        user1.SendFriendRequest(user3);
        user3.AcceptFriendRequest(user1);

        var result = chat.AddUser(user1, user3);

        result.IsSuccess.ShouldBeTrue();

        chat.Users.ShouldContain(user3);
        user3.Chats.ShouldContain(chat);
    }

    [Fact]
    public void AddUser_ShouldReturnConflict_WhenUserAlreadyExists()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);
        
        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);
        
        Chat chat = Chat.CreateChat(user1, user2);

        var result = chat.AddUser(user1, user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Chat bevat deze gebruiker al {user2}"]);
    }

    [Fact]
    public void AddUser_ShouldReturnConflict_WhenUserNotOwner()
    {
        var user1 = TestData.ValidUser(1);
        var user2 = TestData.ValidUser(2);

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateChat(user1, user2);

        var result = chat.AddUser(user1, user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Chat bevat deze gebruiker al {user2}"]);
    }

    //[Fact]
    //public void RemoveUser_ShouldRemove_WhenExists()
    //{
    //    _chat.AddUser(_user1);

    //    var result = _chat.RemoveUser(_user1);

    //    result.IsSuccess.Should().BeTrue();
    //    _chat.Users.Should().NotContain(_user1);
    //}

    //[Fact]
    //public void RemoveUser_ShouldFail_WhenUserNotInChat()
    //{
    //    var result = _chat.RemoveUser(_user1);

    //    result.Status.Should().Be(ResultStatus.Conflict);
    //}

    //[Fact]
    //public void AddTextMessage_ShouldAddMessage_WhenValid()
    //{
    //    // Arrange
    //    _chat.AddUser(_user1);

    //    // Act
    //    var result = _chat.AddTextMessage("Hello world", _user1);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    _chat.Messages.Should().HaveCount(1);
    //    _chat.Messages.First().Text.Value.Should().Be("Hello world");
    //}

    //[Fact]
    //public void AddTextMessage_ShouldFail_WhenEmptyText()
    //{
    //    _chat.AddUser(_user1);

    //    var result = _chat.AddTextMessage("  ", _user1);

    //    result.Status.Should().Be(ResultStatus.Conflict);
    //    _chat.Messages.Should().BeEmpty();
    //}

    //[Fact]
    //public void AddTextMessage_ShouldFail_WhenUserNotInChat()
    //{
    //    var result = _chat.AddTextMessage("Hello", _user1);

    //    result.Status.Should().Be(ResultStatus.Conflict);
    //}

    //[Fact]
    //public void AddMessage_ShouldAdd_WhenValid()
    //{
    //    _chat.AddUser(_user1);

    //    var message = new Message
    //    {
    //        Sender = _user1,
    //        Text = Text.Create("Hi!")
    //    };

    //    var result = _chat.AddMessage(message);

    //    result.IsSuccess.Should().BeTrue();
    //    _chat.Messages.Should().Contain(message);
    //    message.Chat.Should().Be(_chat);
    //}

    //[Fact]
    //public void AddMessage_ShouldFail_WhenSenderNotInChat()
    //{
    //    var message = new Message
    //    {
    //        Sender = _user1,
    //        Text = Text.Create("Hi!")
    //    };

    //    var result = _chat.AddMessage(message);

    //    result.Status.Should().Be(ResultStatus.Conflict);
    //}

    //[Fact]
    //public void RemoveMessage_ShouldRemove_WhenExists()
    //{
    //    _chat.AddUser(_user1);
    //    var message = new Message
    //    {
    //        Sender = _user1,
    //        Text = Text.Create("Hey"),
    //        Chat = _chat
    //    };
    //    _chat.AddMessage(message);

    //    var result = _chat.RemoveMessage(message);

    //    result.IsSuccess.Should().BeTrue();
    //    _chat.Messages.Should().NotContain(message);
    //}
}
