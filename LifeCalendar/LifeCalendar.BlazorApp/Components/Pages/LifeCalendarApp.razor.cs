using System.Drawing;
using System.Globalization;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using LifeCalendar.BlazorApp.Services;
using SkiaSharp;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

//TODO: Kolla hur saker går sönder om earliestYear ändras iom datumändring
//TODO: Render empty fill incase earliest date should be empty when it wasn't before
//TODO: Checkboxes for "display years"
//TODO: När week nums ska visas visa UI för att editera looken

//TODO: Have a boundaryRect and update it instead of recreating all the time
//TODO: Similarly so for xSpacing and ySpacing
//TODO: Auto-update innan man genererat gör bara blank image men en period

//TODO: Fråga Johan om Bind:before motsvarighet, göra något innan inför att bindad param ändras

namespace LifeCalendar.BlazorApp.Components.Pages;

public class EntryInfo
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    [Optional] public string? Color { get; set; }
    [Optional] public string? NameOfEvent { get; set; } = "";
}

public class LifePeriod
{
    public DateTime DateFrom;
    public DateTime DateTo;

    public SKColor SkiaColor;
    public string NameOfEvent = "";
}

public partial class LifeCalendarApp : IAsyncDisposable
{
    #region Parameters

    [Inject] IJSRuntime JS { get; set; } = null!;
    [Inject] SkiaService Skia { get; set; } = null!;
    private IJSObjectReference? _jsFuncs;

    private ElementReference _imageContainer;

    private byte[]? _imgBytes = null!;
    private const int ImgWidth = 2100;
    private const int ImgHeight = 2970;
    private int _rows = 80;
    private int _circleRadius = 10;

    private int _topBorder = 100;
    private int _bottomBorder = 50;
    private int _leftBorder = 50;
    private int _rightBorder = 50;
    private float _strokeWidth = 2;

    private int _earliestYear = 0;
    private int _latestYear = 0;

    private bool _autoUpdate = false;
    private bool _visibleSortRemove = true;
    private bool _visibleBoundaryEdit = false;
    private bool _visibleWeekNumbers = true;

    private string _debugText = "Debug test";

    private List<LifePeriod> _periodsToRender = [];

    #endregion

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsFuncs = await JS.InvokeAsync<IJSObjectReference>("import",
                "./Components/Pages/LifeCalendarApp.razor.js");
        }
    }

    #region Rendering

    private async Task RenderAllPeriods()
    {
        using SKPaint circlePaint = new();
        circlePaint.Color = SKColors.Black;
        circlePaint.IsAntialias = true;
        circlePaint.StrokeWidth = _strokeWidth;
        circlePaint.Style = SKPaintStyle.Stroke;

        Skia.Surface ??= SKSurface.Create(new SKImageInfo(ImgWidth, ImgHeight));

        if (_periodsToRender.Count != 0)
        {
            var canvas = Skia.Surface.Canvas;

            var boundaryRect = MakeBoundaryRect();

            Skia.Fill(canvas, SKColors.White); //Background fill

            var xSpacing = (boundaryRect.Width - _circleRadius * 2) / 51;
            var ySpacing = (boundaryRect.Height - _circleRadius * 2) / (_rows - 1);

            CheckAndSetEarliestYear();

            foreach (var period in _periodsToRender)
            {
                RenderSinglePeriod(canvas, period, boundaryRect, xSpacing, ySpacing);
            }

            RenderWeekNumbers(canvas);
            Skia.DrawCircleMatrix(canvas, boundaryRect, 52, _rows, _circleRadius, circlePaint);
        }
        else
        {
            // Empty list, give warning?
        }

        await RenderSurfaceToImagePreview(Skia.Surface);
    }

    private void RenderSinglePeriod(SKCanvas canvas, LifePeriod period, SKRect boundaryRect, float xSpacing,
        float ySpacing, SKPaint paint = null!)
    {
        using var fillPaint = paint ?? new SKPaint();
        if (paint is null)
        {
            fillPaint.Color = period.SkiaColor;
            fillPaint.IsAntialias = true;
            fillPaint.StrokeWidth = 2;
            fillPaint.Style = SKPaintStyle.Fill;
        }

        var numYears = (period.DateTo.Year - period.DateFrom.Year) + 1;
        var tYearStartIndex = period.DateFrom.Year - _earliestYear;
        var tYearEndIndex = tYearStartIndex + numYears - 1;

        var tCulture = CultureInfo.InvariantCulture;
        var tFromWeek = tCulture.Calendar.GetWeekOfYear(period.DateFrom, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        var tToWeek = tCulture.Calendar.GetWeekOfYear(period.DateTo, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

        tFromWeek = CheckWeek(tFromWeek, period.DateFrom.Month);
        tToWeek = CheckWeek(tToWeek, period.DateTo.Month);

        var fillRadius = _circleRadius - _strokeWidth / 2;

        for (int yearIndex = tYearStartIndex; yearIndex <= tYearEndIndex; yearIndex++)
        {
            var tYPos = boundaryRect.Top + _circleRadius + (ySpacing * yearIndex);

            for (int weekIndex = 1; weekIndex <= 52; weekIndex++)
            {
                var xPos = boundaryRect.Left + _circleRadius + (xSpacing * (weekIndex - 1));

                switch (numYears)
                {
                    case 1:
                    {
                        if (weekIndex >= tFromWeek && weekIndex <= tToWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case 2 when yearIndex == tYearStartIndex:
                    {
                        if (weekIndex >= tFromWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case 2:
                    {
                        if (weekIndex <= tToWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case >= 3 when yearIndex == tYearStartIndex:
                    {
                        if (weekIndex >= tFromWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case >= 3 when yearIndex == tYearEndIndex:
                    {
                        if (weekIndex <= tToWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case >= 3:
                        canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                }
            }
        }
    }

    private void BlankOutSinglePeriod(SKCanvas canvas, LifePeriod period, SKRect boundaryRect, float xSpacing,
        float ySpacing)
    {
        using SKPaint fillPaint = new();
        fillPaint.Color = SKColor.Empty;
        fillPaint.IsAntialias = true;
        fillPaint.StrokeWidth = 2;
        fillPaint.Style = SKPaintStyle.Fill;

        var numYears = (period.DateTo.Year - period.DateFrom.Year) + 1;
        var tYearStartIndex = period.DateFrom.Year - _earliestYear;
        var tYearEndIndex = tYearStartIndex + numYears - 1;

        var tCulture = CultureInfo.InvariantCulture;
        var tFromWeek = tCulture.Calendar.GetWeekOfYear(period.DateFrom, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        var tToWeek = tCulture.Calendar.GetWeekOfYear(period.DateTo, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

        tFromWeek = CheckWeek(tFromWeek, period.DateFrom.Month);
        tToWeek = CheckWeek(tToWeek, period.DateTo.Month);

        var fillRadius = _circleRadius - _strokeWidth / 2;

        for (int yearIndex = tYearStartIndex; yearIndex <= tYearEndIndex; yearIndex++)
        {
            var tYPos = boundaryRect.Top + _circleRadius + (ySpacing * yearIndex);

            for (int weekIndex = 1; weekIndex <= 52; weekIndex++)
            {
                var xPos = boundaryRect.Left + _circleRadius + (xSpacing * (weekIndex - 1));

                switch (numYears)
                {
                    case 1:
                    {
                        if (weekIndex >= tFromWeek && weekIndex <= tToWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case 2 when yearIndex == tYearStartIndex:
                    {
                        if (weekIndex >= tFromWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case 2:
                    {
                        if (weekIndex <= tToWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case >= 3 when yearIndex == tYearStartIndex:
                    {
                        if (weekIndex >= tFromWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case >= 3 when yearIndex == tYearEndIndex:
                    {
                        if (weekIndex <= tToWeek)
                            canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                    }
                    case >= 3:
                        canvas.DrawCircle(xPos, tYPos, fillRadius, fillPaint);
                        break;
                }
            }
        }
    }

    private async Task PartialReRender(int index)
    {
        Skia.Surface ??= SKSurface.Create(new SKImageInfo(ImgWidth, ImgHeight));

        var boundaryRect = MakeBoundaryRect();
        var xSpacing = (boundaryRect.Width - _circleRadius * 2) / 51;
        var ySpacing = (boundaryRect.Height - _circleRadius * 2) / (_rows - 1);

        CheckAndSetEarliestYear();

        RenderSinglePeriod(Skia.Surface.Canvas, _periodsToRender[index], boundaryRect, xSpacing, ySpacing);

        await RenderSurfaceToImagePreview(Skia.Surface);
    }

    private async Task RenderSurfaceToImagePreview(SKSurface surface)
    {
        using (SKImage image = surface.Snapshot())
        using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (MemoryStream ms = new MemoryStream(data.ToArray()))
        {
            string base64Image = Convert.ToBase64String(ms.ToArray());
            _imgBytes = ms.ToArray();
            await _jsFuncs!.InvokeVoidAsync("displayBase64Image", _imageContainer,
                $"data:image/png;base64,{base64Image}");
        }
    }

    private void RenderWeekNumbers(SKCanvas canvas)
    {
        if (!_visibleWeekNumbers) return;

        var boundaryRect = MakeBoundaryRect();

        var xSpacing = (boundaryRect.Width - _circleRadius * 2) / 51;

        for (int i = 0; i < 52; i++)
        {
            var yPos = boundaryRect.Top - 20;
            var xPos = boundaryRect.Left + _circleRadius + (xSpacing * (i));
            Skia.DrawText(canvas, (i + 1).ToString(), xPos, yPos, align: SKTextAlign.Center);
        }
    }

    #endregion

    private async Task DownloadToFile()
    {
        if (_jsFuncs != null && _imgBytes != null)
        {
            await _jsFuncs!.InvokeVoidAsync(
                "downloadFileFromStream",
                $"Image.png",
                new DotNetStreamReference(new MemoryStream(_imgBytes))
            );
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

        foreach (var entry in entries)
        {
            tList.Add(new LifePeriod
            {
                DateFrom = entry.DateFrom,
                DateTo = entry.DateTo,
                SkiaColor = (entry.Color == "") ? Skia.RandomColor() : SKColor.Parse(entry.Color),
                NameOfEvent = entry.NameOfEvent!,
            });
        }

        CheckAndSetEarliestYear();

        return tList;
    }

    private static int CheckWeek(int week, int month)
    {
        if (week == 53)
            return month == 1 ? 1 : 52;
        else
            return week;
    }

    private void CheckAndSetEarliestYear()
    {
        foreach (var period in _periodsToRender)
        {
            if (period.DateFrom.Year < _earliestYear || _earliestYear == 0)
                _earliestYear = period.DateFrom.Year;
        }
    }

    private SKRect MakeBoundaryRect()
    {
        return new SKRect(_leftBorder, _topBorder, ImgWidth - _rightBorder, ImgHeight - _bottomBorder);
    }

    private void RandomizeAllColors()
    {
        foreach (var period in _periodsToRender)
        {
            period.SkiaColor = Skia.RandomColor();
        }
    }

    private void AddBlankLpToList()
    {
        var tP = new LifePeriod
        {
            DateFrom = DateTime.Now.AddYears(-5),
            DateTo = DateTime.Now,
            SkiaColor = Skia.RandomColor()
        };
        _periodsToRender.Add(tP);
    }

    private void Test(EventArgs e)
    {
        var t = e.ToString();
    }

    #region Triggers

    private async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        var tFile = e.File;
        if (tFile.ContentType == "text/csv")
        {
            var mStream = new MemoryStream();
            await tFile.OpenReadStream().CopyToAsync(mStream);
            _periodsToRender = ReadFromCsvStream(mStream);
            await RenderAllPeriods();
        }
    }

    private async Task OnChangedColor(ChangeEventArgs e, int index)
    {
        _periodsToRender[index].SkiaColor = SKColor.Parse(e.Value!.ToString());

        if (!_autoUpdate) return;

        await PartialReRender(index);
    }

    private async Task OnChangedDate(int index)
    {
        if (!_autoUpdate) return;

        Skia.Surface ??= SKSurface.Create(new SKImageInfo(ImgWidth, ImgHeight));

        BlankOutSinglePeriod(Skia.Surface!.Canvas, _periodsToRender[index], MakeBoundaryRect(), 32, 32);

        await PartialReRender(index);
    }

    #endregion

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_jsFuncs != null)
        {
            try
            {
                await _jsFuncs.DisposeAsync();
                GC.SuppressFinalize(this);
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}