using Microsoft.AspNetCore.Components;

namespace Rise.Client.Chats.Components;
public partial class SuggestionChips
{
    [Parameter] public required IEnumerable<string> Suggestions { get; set; } 
    [Parameter] public EventCallback<string> OnPick { get; set; }
}