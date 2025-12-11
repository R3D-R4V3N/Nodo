using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Rise.Shared.Chats;

namespace Rise.Client.Helpdesk.Pages;

public partial class SupervisorChat : ComponentBase
{
    [Inject] public IChatService ChatService { get; set; } = null!;

    private int? _supervisorChatId;
    private bool _isLoading = true;
    private string? _loadError;
    

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _loadError = null;

        StateHasChanged();
        try
        {
            // 1. Haal specifiek de supervisor chat op
            var result = await ChatService.GetSupervisorChatAsync();

            if (result.IsSuccess && result.Value?.Chat != null)
            {
                // 2. Sla ALLEEN het ID op
                _supervisorChatId = result.Value.Chat.ChatId;
            }
            else
            {
                _loadError = result.Errors.FirstOrDefault() 
                             ?? "Kon het gesprek met de begeleider niet vinden.";
            }
        }
        catch (Exception)
        {
            _loadError = "Er is een fout opgetreden bij het laden van de chat.";
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
}