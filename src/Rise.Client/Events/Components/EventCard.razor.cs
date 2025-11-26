using Microsoft.AspNetCore.Components;
using Rise.Shared.Events;

namespace Rise.Client.Events.Components;

public partial class EventCard
{
    [Parameter] public int EventId { get; set; }
    [Parameter] public string ImageUrl { get; set; } = "";
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Location { get; set; } = "";
    [Parameter] public string DateText { get; set; } = "";
    [Parameter] public List<EventDto.InterestedUser> InterestedUsers { get; set; } = [];
    [Parameter] public bool IsInterested { get; set; }
    [Parameter] public double Price { get; set; }
    [Parameter] public EventCallback<int> OnToggleInterest { get; set; }

    private bool _isLoading;

    private async Task HandleToggleInterest()
    {
        if (_isLoading) return;
        _isLoading = true;
        await OnToggleInterest.InvokeAsync(EventId);
        _isLoading = false;
    }

    private string GetButtonClasses()
    {
        var baseClasses = "mt-5 w-full h-[52px] rounded-full font-semibold text-base transition-all duration-200 active:scale-95 shadow-md flex items-center justify-center";
        
        if (_isLoading)
        {
            return $"{baseClasses} bg-gray-100 text-gray-400 cursor-wait shadow-none";
        }
        
        if (IsInterested)
        {
            return $"{baseClasses} bg-[#127646] text-white hover:bg-[#0f6239] hover:shadow-lg";
        }
        
        return $"{baseClasses} bg-white text-[#127646] border-2 border-[#127646] hover:bg-green-50 hover:shadow-lg";
    }
}