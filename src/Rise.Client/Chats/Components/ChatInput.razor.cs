using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Rise.Client.Chats.Components;
public partial class ChatInput
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public EventCallback<string> OnSend { get; set; }
    [Parameter] public EventCallback<RecordedAudio> OnSendVoice { get; set; }
    [Parameter] public bool IsSending { get; set; }

    private IJSObjectReference? _module;
    private bool _isRecording;
    private bool _isProcessing;
    private bool _canRecord = true;
    private string? _errorMessage;

    private async Task HandleSubmit()
    {
        if (IsSending)
        {
            return;
        }

        var text = Value?.Trim();
        if (!string.IsNullOrEmpty(text))
        {
            await OnSend.InvokeAsync(text);
            Value = string.Empty;
            await ValueChanged.InvokeAsync(Value);
        }
    }

    private async Task ToggleRecordingAsync()
    {
        if (_isProcessing)
        {
            return;
        }

        if (!_isRecording)
        {
            await StartRecordingAsync();
        }
        else
        {
            await CompleteRecordingAsync();
        }
    }

    private async Task StartRecordingAsync()
    {
        if (!OnSendVoice.HasDelegate)
        {
            _errorMessage = "Spraakberichten worden niet ondersteund.";
            StateHasChanged();
            return;
        }

        _errorMessage = null;

        try
        {
            await EnsureModuleAsync();
            await _module!.InvokeVoidAsync("startRecording");
            _isRecording = true;
        }
        catch (JSException ex)
        {
            Console.Error.WriteLine(ex);
            _errorMessage = "Kon de microfoon niet starten. Controleer je browserinstellingen.";
            _canRecord = false;
        }

        StateHasChanged();
    }

    private async Task CompleteRecordingAsync()
    {
        if (_module is null)
        {
            _isRecording = false;
            return;
        }

        _isProcessing = true;
        StateHasChanged();

        try
        {
            var audio = await _module.InvokeAsync<RecordedAudio?>("stopRecording");
            if (audio is not null && !string.IsNullOrWhiteSpace(audio.DataUrl))
            {
                _errorMessage = null;
                if (OnSendVoice.HasDelegate)
                {
                    await OnSendVoice.InvokeAsync(audio);
                }
            }
        }
        catch (JSException ex)
        {
            Console.Error.WriteLine(ex);
            _errorMessage = "Het opnemen van audio is mislukt.";
        }
        finally
        {
            _isRecording = false;
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task EnsureModuleAsync()
    {
        _module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./js/voiceRecorder.js");
    }

    private string VoiceButtonClasses
    {
        get
        {
            var classes = new List<string>
        {
            "grid",
            "place-items-center",
            "h-12",
            "w-12",
            "rounded-xl",
            "transition",
            "text-white"
        };

            if (!_canRecord)
            {
                classes.AddRange(new[] { "bg-neutral-300", "text-neutral-500" });
            }
            else if (_isRecording)
            {
                classes.AddRange(new[] { "bg-red-600", "hover:bg-red-700", "animate-pulse" });
            }
            else
            {
                classes.AddRange(new[] { "bg-blue-700", "hover:bg-blue-800" });
            }

            if (IsSending)
            {
                classes.Add("opacity-50");
                classes.Add("cursor-wait");
            }

            if (_isProcessing)
            {
                classes.AddRange(new[] { "opacity-75", "cursor-wait" });
            }

            classes.Add("disabled:opacity-50");
            classes.Add("disabled:cursor-not-allowed");

            return string.Join(' ', classes);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("disposeRecorder");
            }
            catch (JSException)
            {
                // ignore disposal exceptions
            }

            await _module.DisposeAsync();
        }
    }
}