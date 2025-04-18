using System.Globalization;
using System.Text;
using Blazored.SessionStorage;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using LifeCalendar.BlazorApp.Services;
using SkiaSharp;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using LifeCalendar.BlazorApp.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Newtonsoft.Json;

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

    public string SkiaColor;

    public string NameOfEvent = "";
    public bool Hidden = false;
}

public partial class LifeCalendarApp : IAsyncDisposable
{
    #region Parameters

    [Inject] IJSRuntime JS { get; set; } = null!;
    private IJSObjectReference? _jsFuncs;
    [Inject] SkiaService Skia { get; set; } = null!;
    [Inject] ImageDbService? ImageDb { get; set; }
    [Inject] ISessionStorageService SessionStorage { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    private ElementReference _imageContainer;

    private byte[]? _imgBytes = null!;
    private const int ImgWidth = 2100;
    private const int ImgHeight = 2970;
    private int _rows = 80;
    private int _circleRadius = 10;
    private float _strokeWidth = 2;

    private int _topBorder = 150;
    private int _bottomBorder = 50;
    private int _leftBorder = 100;
    private int _rightBorder = 50;
    private SKRect _boundaryRect;

    private float _xSpacing;
    private float _ySpacing;

    private string _updateBtnClass = "";

    private SKColor _colorCircle = SKColors.Black;
    private SKColor _colorBackground = SKColors.White;
    private SKColor _colorText = SKColors.Black;

    private enum Colors
    {
        Background,
        Circles,
        Text
    }

    private int _earliestYear;

    private string _inputOption = "manual";

    private string _fontName = "Atkinson Hyperlegible Next";
    private string _fontResponse = "";

    private string _inputFileResponse = "";

    private string _title = "Life Calendar";

    private bool _showAdvancedOptions = false;

    private bool _visibleTitle = true;
    private bool _visibleWeekNumbers = true;
    private bool _visibleYearNumbers = true;
    private bool _visibleEventNames = false;

    private List<LifePeriod> _periodsToRender = [];

    #endregion

    public LifeCalendarApp()
    {
        UpdateBoundaryRect();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            NavigationManager.LocationChanged += OnChangingLocation;

            _jsFuncs = await JS.InvokeAsync<IJSObjectReference>("import",
                "./Components/Pages/LifeCalendarApp.razor.js");

            Skia.Surface ??= SKSurface.Create(new SKImageInfo(ImgWidth, ImgHeight));

            var oldList = await SessionStorage.GetItemAsync<string>("LifePeriods");
            if (oldList != null)
            {
                var tListFromJson = JsonConvert.DeserializeObject<List<LifePeriod>>(oldList) ?? [];
                if (tListFromJson.Count > 0)
                {
                    _periodsToRender = tListFromJson;
                    await RenderAllPeriods();
                    StateHasChanged();
                }
            }
        }
    }

    #region Rendering

    private async Task RenderAllPeriods()
    {
        using SKPaint circlePaint = new();
        circlePaint.Color = _colorCircle;
        circlePaint.IsAntialias = true;
        circlePaint.StrokeWidth = _strokeWidth;
        circlePaint.Style = SKPaintStyle.Stroke;

        if (_periodsToRender.Count != 0)
        {
            _updateBtnClass = "";

            var canvas = Skia.Surface!.Canvas;

            Skia.Fill(canvas, _colorBackground); //Background fill

            CheckAndSetEarliestYear();

            foreach (var period in _periodsToRender)
            {
                RenderSinglePeriod(canvas, period);
            }

            Skia._defaultPaintFont.Color = _colorText;

            RenderTitle(canvas);
            RenderWeekNumbers(canvas);
            RenderYearNumbers(canvas);
            Skia.DrawCircleMatrix(canvas, _boundaryRect, 52, _rows, _circleRadius, circlePaint);

            RenderEventNames(canvas);
        }

        await RenderSurfaceToImagePreview(Skia.Surface!);
    }

    private void RenderSinglePeriod(SKCanvas canvas, LifePeriod period, SKPaint paint = null!, bool makeBlank = false)
    {
        using var fillPaint = paint ?? new SKPaint();
        if (paint is null)
        {
            fillPaint.Color = makeBlank ? _colorBackground : SKColor.Parse(period.SkiaColor);
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
            var tYPos = _boundaryRect.Top + _circleRadius + (_ySpacing * yearIndex);

            for (int weekIndex = 1; weekIndex <= 52; weekIndex++)
            {
                var xPos = _boundaryRect.Left + _circleRadius + (_xSpacing * (weekIndex - 1));

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

    private async Task RenderSurfaceToImagePreview(SKSurface surface)
    {
        try
        {
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream(data.ToArray());

            var base64Image = Convert.ToBase64String(ms.ToArray());
            _imgBytes = ms.ToArray();
            await _jsFuncs!.InvokeVoidAsync("displayBase64Image", _imageContainer,
                $"data:image/png;base64,{base64Image}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void RenderTitle(SKCanvas canvas)
    {
        if (!_visibleTitle) return;

        var yPos = _boundaryRect.Top - 65;
        var xPos = _boundaryRect.MidX;
        var tFont = Skia.CopyOfDefaultFont();
        tFont.Size = 75;

        Skia.DrawText(canvas, _title, xPos, yPos, alignment: SKTextAlign.Center, font: tFont);
    }

    private void RenderWeekNumbers(SKCanvas canvas)
    {
        if (!_visibleWeekNumbers) return;

        for (var i = 0; i < 52; i++)
        {
            var yPos = _boundaryRect.Top - 20;
            var xPos = _boundaryRect.Left + _circleRadius + (_xSpacing * (i));
            Skia.DrawText(canvas, (i + 1).ToString(), xPos, yPos, alignment: SKTextAlign.Center);
        }
    }

    private void RenderYearNumbers(SKCanvas canvas)
    {
        if (!_visibleYearNumbers) return;

        for (var i = 0; i < _rows; i++)
        {
            String yearText = (i + _earliestYear).ToString();
            Skia._defaultFont.MeasureText(yearText, out var r);

            var centeringOffset = ((_circleRadius * 2) - r.Height) / 2;

            var xPos = _boundaryRect.Left - 45;
            var yPos = _boundaryRect.Top + (_circleRadius * 2) - centeringOffset + (_ySpacing * (i));

            Skia.DrawText(canvas, yearText, xPos, yPos, alignment: SKTextAlign.Center);
        }
    }

    private void RenderEventNames(SKCanvas canvas)
    {
        if (!_visibleEventNames) return;

        var tFont = Skia.CopyOfDefaultFont();
        tFont.Size = 40;

        using var tStrokePaint = new SKPaint();
        tStrokePaint.Color = SKColors.White;
        tStrokePaint.IsAntialias = true;
        tStrokePaint.Style = SKPaintStyle.Stroke;
        tStrokePaint.StrokeWidth = 4;

        var tCulture = CultureInfo.InvariantCulture;

        foreach (var period in _periodsToRender.Where(period => !period.Hidden))
        {
            tFont.MeasureText(period.NameOfEvent, out var textMeasureRect);
            var centeringOffset = ((_circleRadius * 2) - textMeasureRect.Height) / 2;

            var tFromWeek =
                tCulture.Calendar.GetWeekOfYear(period.DateFrom, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            var tToWeek =
                tCulture.Calendar.GetWeekOfYear(period.DateTo, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            float startRow = period.DateFrom.Year - _earliestYear;

            // Measure name width
            // Do math to see if its FromWeek is too late to contain the text
            var spaceForName = ((52 - tFromWeek) * (_circleRadius * 2) + _xSpacing) + _circleRadius;

            var xPos = _boundaryRect.Left + _circleRadius + (_xSpacing * (tFromWeek - 1));

            if (textMeasureRect.Width > spaceForName)
            {
                switch (period.DateTo.Year - period.DateFrom.Year)
                {
                    //Not wide enough for first row, but if the next year is completely filled assume it fits 
                    case >= 2:
                        startRow += 1;
                        xPos = _boundaryRect.Left + _circleRadius;
                        break;

                    //Period ends before the end of next year, check width for the following year
                    case 1:
                    {
                        spaceForName = (tToWeek * (_circleRadius * 2) + _xSpacing) + _circleRadius;
                        if (textMeasureRect.Width < spaceForName)
                        {
                            startRow += 1;
                            xPos = _boundaryRect.Left + _circleRadius;
                        }
                        else
                            continue;

                        break;
                    }
                }
            }

            var yPos = _boundaryRect.Top + (_circleRadius * 2) - centeringOffset + (_ySpacing * (startRow));

            Skia.DrawText(canvas, period.NameOfEvent, xPos, yPos, tStrokePaint, SKTextAlign.Left, tFont);
            Skia.DrawText(canvas, period.NameOfEvent, xPos, yPos, null!, SKTextAlign.Left, tFont);
        }
    }

    #endregion

    private async Task DownloadImageToFile()
    {
        if (_jsFuncs != null && _imgBytes != null)
        {
            await _jsFuncs!.InvokeVoidAsync(
                "downloadFileFromStream",
                $"{(_title != "" ? _title : "Life Calendar")}.png",
                new DotNetStreamReference(new MemoryStream(_imgBytes))
            );
        }
    }

    private async Task DownloadListToFile()
    {
        if (_jsFuncs != null && _periodsToRender.Count > 0)
        {
            var stringToSave = "DateFrom,DateTo,Color,NameOfEvent\n";
            foreach (var period in _periodsToRender)
            {
                stringToSave +=
                    $"{period.DateFrom:yyyy-MM-dd},{period.DateTo:yyyy-MM-dd},{period.SkiaColor},{period.NameOfEvent}\n";
            }

            var bytesToSave = Encoding.UTF8.GetBytes(stringToSave);

            await _jsFuncs!.InvokeVoidAsync(
                "downloadFileFromStream",
                $"{(_title != "" ? _title : "Life Calendar")}_{DateTime.Now:yyyy-MM-dd}.txt",
                new DotNetStreamReference(new MemoryStream(bytesToSave))
            );
        }
    }

    private async Task<bool> ConfirmationDialog(string message)
    {
        return await JS.InvokeAsync<bool>("confirm", message);
    }

    private async Task UploadImageToDb()
    {
        try
        {
            if (await ConfirmationDialog(
                    "Are you sure you want to upload the preview image to the showcase?\nIt will be publicly visible"))
                await ImageDb!.AddImageToDb(_imgBytes!);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
                SkiaColor = (entry.Color == "") ? Skia.RandomColorString() : entry.Color!,
                NameOfEvent = entry.NameOfEvent!
            });
        }

        CheckAndSetEarliestYear();

        return tList;
    }

    private static int CheckWeek(int week, int month)
    {
        if (week == 53)
            return month == 1 ? 1 : 52;

        return week;
    }

    private void CheckAndSetEarliestYear()
    {
        _earliestYear = 0;

        foreach (var period in _periodsToRender)
        {
            if (period.DateFrom.Year < _earliestYear || _earliestYear == 0)
                _earliestYear = period.DateFrom.Year;
        }
    }

    private void UpdateBoundaryRect()
    {
        _boundaryRect = new SKRect(_leftBorder, _topBorder, ImgWidth - _rightBorder, ImgHeight - _bottomBorder);
        _xSpacing = (_boundaryRect.Width - _circleRadius * 2) / 51;
        _ySpacing = (_boundaryRect.Height - _circleRadius * 2) / (_rows - 1);

        OnAnyChange();
    }

    private void RandomizeAllColors()
    {
        foreach (var period in _periodsToRender)
        {
            period.SkiaColor = Skia.RandomColorString();
        }

        OnAnyChange();
    }

    private async Task AddBlankLpToList()
    {
        try
        {
            LifePeriod tP;
            if (_periodsToRender == null! || _periodsToRender.Count == 0)
            {
                tP = new LifePeriod
                {
                    DateFrom = DateTime.Now.AddYears(-1),
                    DateTo = DateTime.Now,
                    SkiaColor = Skia.RandomColorString()
                };
            }
            else
            {
                var lastPeriod = _periodsToRender.Last();
                tP = new LifePeriod
                {
                    DateFrom = lastPeriod.DateTo.AddDays(1),
                    DateTo = lastPeriod.DateTo.AddDays(1).AddYears(1),
                    SkiaColor = Skia.RandomColorString()
                };
            }

            _periodsToRender!.Add(tP);

            OnAnyChange();

            await SaveListToSessionStorage();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    #region Triggers

    private async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        _inputFileResponse = "";

        var tFile = e.File;
        if (tFile.ContentType is "text/csv" or "text/plain")
        {
            try
            {
                var mStream = new MemoryStream();
                await tFile.OpenReadStream().CopyToAsync(mStream);
                _periodsToRender = ReadFromCsvStream(mStream);
                await RenderAllPeriods();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _inputFileResponse =
                    "Failed to read file. Make sure you choose the right file, it's either a .csv or .txt, and is formatted as per the How-To page";
            }
        }
    }

    private async Task OnClickLoadFont()
    {
        if (await Skia.FetchFontFromGoogle(_fontName))
            _fontResponse = "Successfully loaded font";
        else
        {
            _fontResponse = "Failed to load font, make sure spelling is exactly correct";
        }
    }

    private void OnChangedColor(ChangeEventArgs e, Colors colorEnum)
    {
        switch (colorEnum)
        {
            case Colors.Background:
                _colorBackground = SKColor.Parse(e.Value!.ToString());
                break;

            case Colors.Circles:
                _colorCircle = SKColor.Parse(e.Value!.ToString());
                break;

            case Colors.Text:
                _colorText = SKColor.Parse(e.Value!.ToString());
                break;
        }

        OnAnyChange();
    }

    private async void OnChangingLocation(object? sender, LocationChangedEventArgs e)
    {
        //Leaving the page, time to save the list
        if (_periodsToRender.Count <= 0) return;

        await SaveListToSessionStorage();
    }

    private void OnAnyChange()
    {
        if (_updateBtnClass == "")
        {
            _updateBtnClass = "preview-button";
        }
    }

    #endregion

    private async Task SaveListToSessionStorage()
    {
        try
        {
            var tJson = JsonConvert.SerializeObject(_periodsToRender);
            await SessionStorage.SetItemAsync("LifePeriods", tJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void SwapPeriods(int index1, int index2)
    {
        (_periodsToRender[index1], _periodsToRender[index2]) = (_periodsToRender[index2], _periodsToRender[index1]);
    }

    private void InputOptionChanged(ChangeEventArgs e)
    {
        _inputOption = e.Value!.ToString()!;
    }

    private async Task ResetAdvancedToDefaults()
    {
        if (await ConfirmationDialog("Are you sure you want to reset all advanced settings to default?"))
        {
            _rows = 80;
            _visibleWeekNumbers = true;
            _visibleYearNumbers = true;
            _visibleEventNames = false;

            _colorCircle = SKColors.Black;
            _circleRadius = 10;
            _strokeWidth = 2;
            _colorBackground = SKColors.White;
            _colorText = SKColors.Black;

            _topBorder = 150;
            _leftBorder = 100;
            _rightBorder = 50;
            _bottomBorder = 50;

            _fontName = "Atkinson Hyperlegible Next";
            if (Skia._defaultFont.Typeface.FamilyName != _fontName)
                await Skia.FetchFontFromGoogle(_fontName);

            UpdateBoundaryRect();
        }
    }

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