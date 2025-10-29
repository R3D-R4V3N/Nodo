using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Chats;
using System.Security.Claims;
using Rise.Shared.Assets;
using Rise.Shared.Users;

namespace Rise.Client.Home.Pages;

public partial class Homepage
{
    [CascadingParameter]
    public UserDto.CurrentUser? CurrentUser { get; set; }
    
    private readonly List<ChatDto.GetChats> _chats = new();
    private List<ChatDto.GetChats> _filteredChats => string.IsNullOrWhiteSpace(_searchTerm)
        ? _chats
        : _chats.Where(c => GetChatTitle(c)
            .Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();

    private bool _isLoading = true;
    private string? _loadError;
    private string? _searchTerm;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            NavigationManager.NavigateTo("/login", true);
            return;
        }

        var result = await ChatService.GetAllAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            _chats.Clear();
            _chats.AddRange(result.Value.Chats ?? []);
        }
        else
        {
            _loadError = result.Errors.FirstOrDefault() ?? "De chats konden niet geladen worden.";
        }

        _isLoading = false;
    }

    private void NavigateToChat(ChatDto.GetChats chat) 
        => NavigationManager.NavigateTo($"/chat/{chat.ChatId}");

    private string GetChatTitle(ChatDto.GetChats chat)
    {
        var chatUserNames = chat?
            .Users
            .Where(x => x.AccountId != CurrentUser!.AccountId)
            .Select(x => x.Name)
            .ToList() ?? [$"Chat {chat?.ChatId}"];

        return string.Join(", ", chatUserNames);
    }

    private static string GetLastMessagePreview(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        if (last is null || string.IsNullOrWhiteSpace(last.Content)) 
            return "Nog geen berichten";

        var preview = last.Content;

        return preview.Length <= 80 
            ? preview : 
            string.Concat(preview.AsSpan(0, 80), "�");
    }

    private static string GetLastActivity(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        return last?.Timestamp?.ToString("HH:mm") ?? "-";
    }

    private string DetermineGreetingNameFromIdentity()
    {
        return CurrentUser!.Name;
    }

   private string GetAvatarUrl(ChatDto.GetChats chat)
   {
       // Zoek de eerste gebruiker die niet de huidige gebruiker is
       var otherUser = chat?
           .Users?
           .FirstOrDefault(u => u.Id != CurrentUser.Id);

       return otherUser?.AvatarUrl ?? DefaultImages.Profile;
   }


    private static string GetAvatarKey(MessageDto.Chat dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.User.AccountId))
            return dto.User.AccountId;

        if (dto.User.Id != 0)
            return dto.User.Id.ToString(CultureInfo.InvariantCulture);

        return dto.User.Name ?? string.Empty;
    }
    private bool IsGroupChat(ChatDto.GetChats chat)
    {
        return chat.Users.Count > 2;
    }

    private void NavigateToFriendProfile(ChatDto.GetChats chat)
    {
        var otherParticipant = chat.Users
            .Where(m => !string.Equals(m.AccountId, CurrentUser.AccountId, StringComparison.Ordinal))
            .FirstOrDefault();

        if (otherParticipant is not null)
        {
            var accountId = otherParticipant.AccountId; // Dit is dus ApplicationUser.AccountId
            NavigationManager.NavigateTo($"/FriendProfilePage/{accountId}");
        }
    }
}