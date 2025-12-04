using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Users;
using Rise.Tests.Shared;
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
    public void CreatePrivateChat_ShouldCreate()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        var result = Chat.CreatePrivateChat(user1, user2);

        result.IsSuccess.ShouldBeTrue();
        Chat chat = result;

        chat.Users.ShouldContain(user1);
        chat.Users.ShouldContain(user2);
        chat.ChatType.ShouldBe(ChatType.Private);
        user1.Chats.ShouldContain(chat);
        user2.Chats.ShouldContain(chat);
    }

    [Fact]
    public void CreatePrivateChat_ShouldConflict_WhenNotFriend()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        var result = Chat.CreatePrivateChat(user1, user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Chat kan niet worden gemaakt omdat {user1} en {user2} elkaar niet bevriend zijn"]);
    }

    [Fact]
    public void CreateGroupChat_ShouldCreate()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();
        var user3 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        user1.SendFriendRequest(user3);
        user3.AcceptFriendRequest(user1);

        var result = Chat.CreateGroupChat(user1, user2, user3);

        result.IsSuccess.ShouldBeTrue();
        Chat chat = result;

        chat.Users.ShouldContain(user1);
        chat.Users.ShouldContain(user2);
        chat.Users.ShouldContain(user3);
        chat.ChatType.ShouldBe(ChatType.Group);
        user1.Chats.ShouldContain(chat);
        user2.Chats.ShouldContain(chat);
        user3.Chats.ShouldContain(chat);
    }

    [Fact]
    public void CreateGroupChat_ShouldConflict_WhenNotFriend()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        var result = Chat.CreateGroupChat(user1, user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Chat kan niet worden gemaakt omdat {user1} en {user2} elkaar niet bevriend zijn"]);
    }

    [Fact]
    public void AddUser_ShouldAddUser_WhenGroup()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);

        var user3 = DomainData.ValidUser();
        user1.SendFriendRequest(user3);
        user3.AcceptFriendRequest(user1);

        var result = chat.AddUser(user1, user3);

        result.IsSuccess.ShouldBeTrue();

        chat.Users.ShouldContain(user3);
        user3.Chats.ShouldContain(chat);
    }

    [Fact]
    public void AddUser_ShouldConflict_WhenPrivateChat()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreatePrivateChat(user1, user2);

        var user3 = DomainData.ValidUser();
        user1.SendFriendRequest(user3);
        user3.AcceptFriendRequest(user1);

        var result = chat.AddUser(user1, user3);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Er kunnen geen gebruikers toegevoegd worden aan een private chat"]);
    }

    [Fact]
    public void AddUser_ShouldReturnConflict_WhenUserAlreadyExists()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();
        
        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);
        
        Chat chat = Chat.CreateGroupChat(user1, user2);

        var result = chat.AddUser(user1, user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Chat bevat deze gebruiker al {user2}"]);
    }

    [Fact]
    public void AddUser_ShouldReturnConflict_WhenUserNotOwner()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);

        var result = chat.AddUser(user1, user2);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Chat bevat deze gebruiker al {user2}"]);
    }

    [Fact]
    public void RemoveUser_ShouldRemove_InGroupChat()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);

        var result = chat.RemoveUser(user1, user1);

        result.IsSuccess.ShouldBeTrue();
        chat.Users.ShouldNotContain(user1);
        user1.Chats.ShouldNotContain(chat);
    }

    [Fact]
    public void RemoveUser_ShouldConflict_WhenPrivateChat()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreatePrivateChat(user1, user2);

        var result = chat.RemoveUser(user1, user1);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Er kunnen geen gebruikers verwijderd worden van een private chat"]);
    }

    [Fact]
    public void RemoveUser_ShouldRemove_PassedOwnerIsSupervisor()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();
        var user3 = DomainData.ValidSupervisor();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);

        var result = chat.RemoveUser(user3, user1);

        result.IsSuccess.ShouldBeTrue();
        chat.Users.ShouldNotContain(user1);
        user1.Chats.ShouldNotContain(chat);
    }

    [Fact]
    public void RemoveUser_ShouldConflict_UserNotOwner()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();
        var user3 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);

        var result = chat.RemoveUser(user3, user1);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Meegegeven gebruiker: {user3} is geen chat eigenaar"]);
        chat.Users.ShouldContain(user1);
        user1.Chats.ShouldContain(chat);
    }

    [Fact]
    public void RemoveUser_ShouldConflict_UserNotPresent()
    {
        var user1 = DomainData.ValidUser();
        var user2 = DomainData.ValidUser();
        var user3 = DomainData.ValidUser();

        user1.SendFriendRequest(user2);
        user2.AcceptFriendRequest(user1);

        Chat chat = Chat.CreateGroupChat(user1, user2);

        var result = chat.RemoveUser(user1, user3);

        result.Status.ShouldBe(ResultStatus.Conflict);
        result.Errors.ShouldBe([$"Chat bevat {user3} niet"]);
        chat.Users.ShouldContain(user1);
        chat.Users.ShouldContain(user2);
        user1.Chats.ShouldContain(chat);
        user2.Chats.ShouldContain(chat);
    }
}
