using System.Globalization;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using LifeCalendar.BlazorApp.Services;
using SkiaSharp;
using CsvHelper;
using Microsoft.AspNetCore.Components.Forms;

namespace LifeCalendar.BlazorApp.Components.Pages;

public class EntryInfo()
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string? Color { get; set; }
}

public class LifePeriod()
{
    public int FromYear { get; set; }
    public int FromWeek { get; set; }
    public int ToYear { get; set; }
    public int ToWeek { get; set; }

    public SKColor SkiaColor { get; set; }
    // public string? NameOfEvent { get; set; } = null!;
}

public partial class LifeCalendarApp : IAsyncDisposable
{
    [Inject] IJSRuntime JS { get; set; } = null!;
    [Inject] SkiaService Skia { get; set; } = null!;
    private IJSObjectReference? JSFuncs;

    private ElementReference _imageContainer;
    private ElementReference _debugP;
    private byte[] _imgBytes = null!;
    private int _imgWidth = 2100;
    private int _imgHeight = 2970;
    private int _rows = 80;
    private int _radius = 10;

    private int _topBorder = 100;
    private int _bottomBorder = 50;
    private int _leftBorder = 50;
    private int _rightBorder = 50;

    private int _earliestYear = 0;
    private int _latestYear = 0;

    private string debugText = "Debug test";

    private List<LifePeriod> _periodsToRender = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            JSFuncs = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/LifeCalendarApp.razor.js");

            // _entries = ReadFromCsvData(@"C:\Temp\dates.csv");
            // await RenderSkia();
            // await RenderEntries();
        }
    }

    private async Task RenderSkiaTest()
    {
        using (SKPaint paint = new()
               {
                   Color = SKColors.Black,
                   IsAntialias = true,
                   StrokeWidth = 2,
                   Style = SKPaintStyle.Fill
               })
        using (SKSurface surface = SKSurface.Create(new SKImageInfo(_imgWidth, _imgHeight)))
        {
            var canvas = surface.Canvas;
            Skia.Fill(canvas, SKColors.White);

            Skia.defaultPaint = paint;

            Skia.defaultPaint.Style = SKPaintStyle.Fill;
            Skia.DrawText(canvas, "Test text 1234567890", (float) _imgWidth / 2, 50);

            var tRect = new SKRect(50, 100, _imgWidth - 50, _imgHeight - 50);
            Skia.defaultPaint.Style = SKPaintStyle.Stroke;
            // Skia.DrawRectangle(canvas, tRect);
            Skia.defaultFont.Size = 50;
            Skia.DrawCircleMatrix(canvas, tRect, 52, _rows, _radius);

            await RenderSurfaceToImagePreview(surface);
        }
    }

    private async Task RenderLPs()
    {
        using (SKPaint circlePaint = new()
               {
                   Color = SKColors.Black,
                   IsAntialias = true,
                   StrokeWidth = 2,
                   Style = SKPaintStyle.Stroke
               })
        using (SKPaint fillPaint = new()
               {
                   Color = SKColors.CornflowerBlue,
                   IsAntialias = true,
                   StrokeWidth = 2,
                   Style = SKPaintStyle.Fill
               })
        using (SKSurface surface = SKSurface.Create(new SKImageInfo(_imgWidth, _imgHeight)))
        {
            if (_periodsToRender.Any())
            {
                var canvas = surface.Canvas;
                var periodIndex = 0;
                var currentYear = _earliestYear;

                var boundaryRect = new SKRect(_leftBorder, _topBorder, _imgWidth - _rightBorder,
                    _imgHeight - _bottomBorder);

                Skia.Fill(canvas, SKColors.White); //Background fill

                var xSpacing = (boundaryRect.Width - _radius * 2) / 51;
                var ySpacing = (boundaryRect.Height - _radius * 2) / (_rows - 1);

                fillPaint.Color = _periodsToRender[periodIndex].SkiaColor;
                var numYears = (_periodsToRender.Last().ToYear - _periodsToRender.First().FromYear) + 1;

                // Years / Rows
                for (int row = 0; row < numYears; row++)
                {
                    var tYPos = boundaryRect.Top + _radius + ySpacing * row;

                    // Weeks / Columns
                    for (int column = 0; column < 52; column++)
                    {
                        var tXPos = boundaryRect.Left + _radius + (xSpacing * column);

                        canvas.DrawCircle(tXPos, tYPos, _radius, fillPaint);

                        if ((column + 1) >= _periodsToRender[periodIndex].ToWeek &&
                            currentYear >= _periodsToRender[periodIndex].ToYear)
                        {
                            if (periodIndex < _periodsToRender.Count - 1)
                            {
                                periodIndex++;
                                fillPaint.Color = _periodsToRender[periodIndex].SkiaColor;
                            }
                            else
                            {
                                // Out of life periods, rest should be blank
                                fillPaint.Color = SKColors.Empty;
                            }
                        }
                    }

                    currentYear++;
                }

                Skia.DrawCircleMatrix(canvas, boundaryRect, 52, _rows, _radius, circlePaint);
            }
            else
            {
                // Empty list, give warning?
            }

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
            _imgBytes = ms.ToArray();
            await JSFuncs!.InvokeVoidAsync("displayBase64Image", _imageContainer,
                $"data:image/png;base64,{base64Image}");
        }
    }

    private async Task DownloadToFile()
    {
        if (JSFuncs != null && _imgBytes != null)
        {
            await JSFuncs!.InvokeVoidAsync(
                "downloadFileFromStream",
                $"Image.png",
                new DotNetStreamReference(new MemoryStream(_imgBytes))
            );
        }
    }

    private async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        var tFile = e.File;
        if (tFile.ContentType == "text/csv")
        {
            var mStream = new MemoryStream();
            await tFile.OpenReadStream().CopyToAsync(mStream);
            _periodsToRender = ReadFromCsvStream(mStream);
            await RenderLPs();
        }
    }

    private List<LifePeriod> ReadFromCsvStream(MemoryStream fileStream)
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        using var sr = new StreamReader(fileStream);
        using var csv = new CsvReader(sr, CultureInfo.InvariantCulture);

        List<LifePeriod> tList = null!;
        try
        {
            var records = csv.GetRecords<EntryInfo>();
            tList = ConvertEntriesToLifePeriods(records.ToList());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return tList;
    }

    private List<LifePeriod> ConvertEntriesToLifePeriods(List<EntryInfo> entries)
    {
        var tList = new List<LifePeriod>();
        var tEarliestYear = 0;
        var tLastYear = 0;

        foreach (var entry in entries)
        {
            var tCulture = CultureInfo.InvariantCulture;
            var weekF = tCulture.Calendar.GetWeekOfYear(entry.DateFrom, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            var weekT = tCulture.Calendar.GetWeekOfYear(entry.DateTo, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            weekF = CheckWeek(weekF, entry.DateFrom.Month);
            weekT = CheckWeek(weekT, entry.DateTo.Month);

            if (tEarliestYear == 0) tEarliestYear = entry.DateFrom.Year;

            tList.Add(new LifePeriod
            {
                FromYear = entry.DateFrom.Year,
                FromWeek = weekF,
                ToYear = entry.DateTo.Year,
                ToWeek = weekT,
                SkiaColor = (entry.Color == "") ? Skia.RandomColor() : SKColor.Parse(entry.Color),
            });

            if (tLastYear == 0) tLastYear = entry.DateTo.Year;
            else if (tLastYear < entry.DateTo.Year) tLastYear = entry.DateTo.Year;
        }

        _earliestYear = tEarliestYear;
        _latestYear = tLastYear;

        return tList;
    }

    private static int CheckWeek(int week, int month)
    {
        if (week == 53)
            return month == 1 ? 1 : 52;
        else
            return week;
    }

    private async Task debug()
    {
        debugText = "";

        RandomizeAllColors();
        await RenderLPs();
    }

    private void RandomizeAllColors()
    {
        foreach (var period in _periodsToRender)
        {
            period.SkiaColor = Skia.RandomColor();
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
            catch (JSDisconnectedException)
            {
            }
        }
    }
}