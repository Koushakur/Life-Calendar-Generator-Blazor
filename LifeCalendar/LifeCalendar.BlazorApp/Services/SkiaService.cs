using System.Globalization;
using System.Reflection;
using SkiaSharp;

namespace LifeCalendar.BlazorApp.Services;

public class SkiaService
{
    public SKSurface? Surface;

    public SKPaint _defaultPaint = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
        StrokeWidth = 3,
        Style = SKPaintStyle.Stroke
    };

    public SKFont _defaultFont = new()
    {
        Size = 24
    };

    public SKPaint _defaultPaintFont = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
        Style = SKPaintStyle.StrokeAndFill,
        StrokeWidth = 1
    };

    public SkiaService()
    {
        // _defaultFont.Typeface = GetTypefaceResource("AtkinsonHyperlegible-Regular.ttf");
        _defaultFont.Typeface = GetTypefaceResource("AtkinsonHyperlegibleNext-VariableFont_wght.ttf");
    }

    public void Fill(SKCanvas canvas, SKColor color)
    {
        canvas.Clear(color);
    }

    public void DrawRectangle(SKCanvas canvas, SKRect rect)
    {
        canvas.DrawRect(rect, _defaultPaint);
    }

    private static SKTypeface GetTypefaceResource(string fontFileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(@"LifeCalendar.BlazorApp.Services.Fonts." + fontFileName);
        if (stream == null)
            return null!;

        return SKTypeface.FromStream(stream);
    }

    public async Task<bool> FetchFontFromGoogle(string fontName)
    {
        try
        {
            //Case-sensitive, making sure it's right
            fontName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fontName);

            using var client = new HttpClient();

            var res = await client.GetAsync($"https://fonts.googleapis.com/css?family={fontName}");

            if (!res.IsSuccessStatusCode) return false;

            var content = await res.Content.ReadAsStringAsync();
            const string searchString = @"src: url(";
            var startIndex = content.IndexOf(searchString, StringComparison.Ordinal);
            var endIndex = content.IndexOf(')', startIndex);

            var urlSubstring = content.Substring(startIndex + searchString.Length,
                (endIndex - startIndex) - searchString.Length);

            var fontRes = await client.GetAsync(urlSubstring);

            if (!fontRes.IsSuccessStatusCode) return false;

            _defaultFont.Typeface = SKTypeface.FromStream(await fontRes.Content.ReadAsStreamAsync());

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public void DrawCircleMatrix(
        SKCanvas canvas,
        SKRect rect,
        int columns,
        int rows,
        float radius,
        SKPaint? paint = null)
    {
        paint ??= _defaultPaint;

        var xSpacing = (rect.Width - radius * 2) / (columns - 1);
        var ySpacing = (rect.Height - radius * 2) / (rows - 1);

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                canvas.DrawCircle(
                    rect.Left + radius + i * xSpacing,
                    rect.Top + radius + j * ySpacing,
                    radius,
                    paint
                );
            }
        }
    }

    public void DrawText(
        SKCanvas canvas,
        string text,
        float x,
        float y,
        SKPaint paint = null!,
        SKTextAlign alignment = SKTextAlign.Center,
        SKFont font = null!)
    {
        paint ??= _defaultPaintFont;
        font ??= _defaultFont;
        canvas.DrawText(text, x, y, alignment, font, paint);
    }

    public string RandomColorString()
    {
        var rnd = new Random();
        var colStr = SKColor.FromHsl(rnd.Next(0, 360), rnd.Next(40, 80), rnd.Next(40, 90)).ToString();
        return colStr.Remove(1, 2);
    }

    public SKColor RandomColorSkColor()
    {
        var rnd = new Random();
        return SKColor.FromHsl(rnd.Next(0, 360), rnd.Next(40, 80), rnd.Next(40, 90));
    }
}