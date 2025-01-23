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
        Typeface = SKTypeface.FromFamilyName("Atkinson Hyperlegible"),
        Size = 24
    };

    public SKPaint _defaultPaintFont = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
        Style = SKPaintStyle.StrokeAndFill,
        StrokeWidth = 1
    };

    public void Fill(SKCanvas canvas, SKColor color)
    {
        canvas.Clear(color);
    }

    public void DrawRectangle(SKCanvas canvas, SKRect rect)
    {
        canvas.DrawRect(rect, _defaultPaint);
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
        SKTextAlign align = SKTextAlign.Center,
        SKFont font = null!)
    {
        paint ??= _defaultPaintFont;
        font ??= _defaultFont;
        canvas.DrawText(text, x, y, align, font, paint);
    }

    public SKColor RandomColor()
    {
        var rnd = new Random();
        return SKColor.FromHsl(rnd.Next(0, 360), rnd.Next(40, 80), rnd.Next(40, 90));
    }
}