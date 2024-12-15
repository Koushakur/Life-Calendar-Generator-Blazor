using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace LifeCalendar.BlazorApp.Components.Pages;

public partial class LifeCalendarApp : IAsyncDisposable {
    private byte[] imgBytes = [];
    [Inject] IJSRuntime JS { get; set; } = null!;
    private IJSObjectReference? JSFuncs;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            JSFuncs = await JS.InvokeAsync<IJSObjectReference>("import",
                "./Components/Pages/LifeCalendarApp.razor.js");
        }
    }

    private async Task DownloadToFile() {
        if (JSFuncs != null) {
            await JSFuncs!.InvokeVoidAsync(
            "downloadFileFromStream",
                $"Image.png",
                new DotNetStreamReference(new MemoryStream(imgBytes))
            );
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync() {
        if (JSFuncs != null) {
            try {
                await JSFuncs.DisposeAsync();
                GC.SuppressFinalize(this);
            } catch (JSDisconnectedException) { }
        }
    }
}
