using Microsoft.AspNetCore.Components;
using Rise.Shared.Chats;

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
    [Parameter] public MessageAttachment? Attachment { get; set; }

    private RenderFragment RenderContent() => builder =>
    {
        var seq = 0;
        var isPendingOutgoing = IsOutgoing && IsPending;
        if (Attachment is not null)
        {
            var attachmentWrapper = IsOutgoing
                ? isPendingOutgoing ? "flex flex-col gap-2" : "flex flex-col gap-2 text-white"
                : "flex flex-col gap-2";
            var linkClasses = IsOutgoing && !isPendingOutgoing ? "text-white underline" : "text-blue-700 underline";

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", attachmentWrapper);

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "flex items-center gap-2 text-sm");
            builder.OpenElement(seq++, "svg");
            builder.AddAttribute(seq++, "class", "h-5 w-5");
            builder.AddAttribute(seq++, "xmlns", "http://www.w3.org/2000/svg");
            builder.AddAttribute(seq++, "fill", "none");
            builder.AddAttribute(seq++, "viewBox", "0 0 24 24");
            builder.AddAttribute(seq++, "stroke-width", "1.5");
            builder.AddAttribute(seq++, "stroke", "currentColor");
            builder.AddMarkupContent(seq++, "<path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M9 12V7.5a3.75 3.75 0 1 1 7.5 0V12a6 6 0 1 1-12 0V8.25\" />");
            builder.CloseElement();

            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "flex flex-col");
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "text-xs uppercase tracking-wide");
            builder.AddContent(seq++, Attachment.ContentType);
            builder.CloseElement();

            if (!string.IsNullOrWhiteSpace(Attachment.Url))
            {
                builder.OpenElement(seq++, "a");
                builder.AddAttribute(seq++, "class", linkClasses);
                builder.AddAttribute(seq++, "href", Attachment.Url);
                builder.AddAttribute(seq++, "target", "_blank");
                builder.AddContent(seq++, Attachment.FileName ?? "bijlage");
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(seq++, "span");
                builder.AddAttribute(seq++, "class", "text-sm");
                builder.AddContent(seq++, Attachment.FileName ?? "bijlage");
                builder.CloseElement();
            }

            builder.CloseElement();

            builder.CloseElement();

            if (!string.IsNullOrWhiteSpace(Text))
            {
                builder.OpenElement(seq++, "p");
                builder.AddAttribute(seq++, "class", "text-sm");
                builder.AddContent(seq++, Text);
                builder.CloseElement();
            }
        }
        else if (!string.IsNullOrWhiteSpace(AudioUrl))
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