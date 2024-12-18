using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using LifeCalendar.BlazorApp.Services;
using SkiaSharp;

namespace LifeCalendar.BlazorApp.Components.Pages;

public partial class LifeCalendarApp : IAsyncDisposable
{
    [Inject] IJSRuntime JS { get; set; } = null!;
    [Inject] SkiaService Skia { get; set; } = null!;
    private IJSObjectReference? JSFuncs;

    private ElementReference imageContainer;
    private byte[] imgBytes = null!;
    private int imgWidth = 2100;
    private int imgHeight = 2970;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            JSFuncs = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/LifeCalendarApp.razor.js");
            await RenderSkia();
        }
    }

    public async Task RenderSkia()
    {
        using (SKPaint paint = new()
        {
            Color = SKColors.Black,
            IsAntialias = true,
            StrokeWidth = 2,
            Style = SKPaintStyle.Fill
        })
        using (SKSurface surface = SKSurface.Create(new SKImageInfo(imgWidth, imgHeight)))
        {
            var canvas = surface.Canvas;
            Skia.Fill(canvas, SKColors.White);

            Skia.defaultPaint = paint;

            Skia.defaultPaint.Style = SKPaintStyle.Fill;
            Skia.DrawText(canvas, "Test text 1234567890", imgWidth / 2, 50);

            var tRect = new SKRect(50, 100, imgWidth - 50, imgHeight - 50);
            Skia.defaultPaint.Style = SKPaintStyle.Stroke;
            Skia.DrawCircleMatrix(canvas, tRect, 20, 20, 10);

            await RenderSurfaceToImagePreview(surface);
        }
    }

    private async Task RenderSurfaceToImagePreview(SKSurface surface)
    {
        using (SKImage image = surface.Snapshot())
        using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (MemoryStream ms = new MemoryStream(data.ToArray()))
        {
            string base64Image = Convert.ToBase64String(ms.ToArray());
            imgBytes = ms.ToArray();
            await JSFuncs!.InvokeVoidAsync("displayBase64Image", imageContainer, $"data:image/png;base64,{base64Image}");
        }
    }

    private async Task DownloadToFile()
    {
        if (JSFuncs != null && imgBytes != null)
        {
            await JSFuncs!.InvokeVoidAsync(
               "downloadFileFromStream",
               $"Image.png",
               new DotNetStreamReference(new MemoryStream(imgBytes))
            );
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (JSFuncs != null)
        {
            try
            {
                await JSFuncs.DisposeAsync();
                GC.SuppressFinalize(this);
            }
            catch (JSDisconnectedException) { }
        }
    }
}
