using Microsoft.JSInterop;
using Rise.Client.Chats.Components;

namespace Rise.Client.Chats;

public interface IVoiceRecorderService : IAsyncDisposable
{
    Task StartRecordingAsync();
    Task<RecordedAudio?> StopRecordingAsync();
}

public sealed class VoiceRecorderService : IVoiceRecorderService
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public VoiceRecorderService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task StartRecordingAsync()
    {
        await EnsureModuleAsync();
        await _module!.InvokeVoidAsync("startRecording");
    }

    public async Task<RecordedAudio?> StopRecordingAsync()
    {
        if (_module is null)
        {
            return null;
        }

        return await _module.InvokeAsync<RecordedAudio?>("stopRecording");
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

    private async Task EnsureModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/voiceRecorder.js");
    }
}
