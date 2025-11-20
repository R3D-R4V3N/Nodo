using Microsoft.AspNetCore.Components;

namespace Rise.Client.Chats.Components;
public partial class MessageBubble
{
    [Parameter] public string Text { get; set; } = "";
    [Parameter] public bool IsOutgoing { get; set; }
    [Parameter] public string? AvatarUrl { get; set; }
    [Parameter] public string? AudioUrl { get; set; }
    [Parameter] public TimeSpan? AudioDuration { get; set; }
    [Parameter] public bool IsPending { get; set; }
    [Parameter] public int? QueuedOperationId { get; set; }
    [Parameter] public EventCallback OnCancelPending { get; set; }

    private RenderFragment RenderContent() => builder =>
    {
        var seq = 0;
        var isPendingOutgoing = IsOutgoing && IsPending;
        if (!string.IsNullOrWhiteSpace(AudioUrl))
        {
            var audioWrapperClasses = IsOutgoing
                ? isPendingOutgoing ? "flex flex-col gap-2" : "flex flex-col gap-2 text-white"
                : "flex flex-col gap-2";
            var durationLabelClasses = IsOutgoing
                ? isPendingOutgoing ? "text-[10px] uppercase tracking-wide text-neutral-600" : "text-[10px] uppercase tracking-wide text-white/70"
                : "text-[10px] uppercase tracking-wide text-neutral-500";
            var audioTextClasses = IsOutgoing
                ? isPendingOutgoing ? "text-sm text-neutral-900" : "text-sm text-white"
                : "text-sm";

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", audioWrapperClasses);

            builder.OpenElement(seq++, "audio");
            builder.AddAttribute(seq++, "class", "w-52 max-w-full");
            builder.AddAttribute(seq++, "controls", true);
            builder.AddAttribute(seq++, "src", AudioUrl);
            builder.AddAttribute(seq++, "preload", "metadata");
            builder.CloseElement();

            if (DurationLabel is not null)
            {
                builder.OpenElement(seq++, "span");
                builder.AddAttribute(seq++, "class", durationLabelClasses);
                builder.AddContent(seq++, DurationLabel);
                builder.CloseElement();
            }

            if (!string.IsNullOrWhiteSpace(Text))
            {
                builder.OpenElement(seq++, "p");
                builder.AddAttribute(seq++, "class", audioTextClasses);
                builder.AddContent(seq++, Text);
                builder.CloseElement();
            }

            builder.CloseElement();
        }
        else
        {
            builder.AddContent(seq++, Text);
        }
    };

    private string IncomingBubbleClasses => !string.IsNullOrWhiteSpace(AudioUrl)
        ? "bg-white rounded-2xl rounded-tl-none px-4 py-3 shadow-sm text-sm max-w-[80%]"
        : "bg-white rounded-2xl rounded-tl-none px-4 py-2 shadow-sm text-sm max-w-[80%]";

    private string OutgoingBubbleClasses
    {
        get
        {
            if (IsPending)
            {
                var pendingClasses = !string.IsNullOrWhiteSpace(AudioUrl)
                    ? "bg-green-50 text-neutral-900 rounded-2xl rounded-tr-none px-4 py-3 shadow-sm text-sm max-w-[80%] border-2 border-red-500 border-dashed focus:outline-none focus:ring-2 focus:ring-red-300"
                    : "bg-green-50 text-neutral-900 rounded-2xl rounded-tr-none px-4 py-2 text-sm shadow-sm max-w-[80%] border-2 border-red-500 border-dashed focus:outline-none focus:ring-2 focus:ring-red-300";

                return CanCancelPending ? $"{pendingClasses} cursor-pointer" : pendingClasses;
            }

            return !string.IsNullOrWhiteSpace(AudioUrl)
                ? "bg-[#127646] text-white rounded-2xl rounded-tr-none px-4 py-3 shadow-sm text-sm max-w-[80%]"
                : "bg-[#127646] text-white rounded-2xl rounded-tr-none px-4 py-2 text-sm shadow-sm max-w-[80%]";
        }
    }

    private bool CanCancelPending => IsPending && OnCancelPending.HasDelegate;

    private Task HandlePendingClick()
    {
        if (!CanCancelPending)
        {
            return Task.CompletedTask;
        }

        return OnCancelPending.InvokeAsync();
    }

    private string? DurationLabel => AudioDuration is { TotalSeconds: > 0 } duration
        ? duration.ToString(@"m\:ss")
        : null;
}