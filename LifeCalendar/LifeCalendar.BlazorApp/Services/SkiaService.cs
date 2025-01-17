using SkiaSharp;

namespace LifeCalendar.BlazorApp.Services;

public class SkiaService
{
    public SKSurface? Surface;

    public SKPaint DefaultPaint = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
        StrokeWidth = 3,
        Style = SKPaintStyle.Stroke
    };

    public SKFont DefaultFont = new()
    {
        Typeface = SKTypeface.FromFamilyName("Atkinson Hyperlegible"),
        Size = 25
    };

    public void Fill(SKCanvas canvas, SKColor color)
    {
        canvas.Clear(color);
    }

    public void DrawRectangle(SKCanvas canvas, SKRect rect)
    {
        canvas.DrawRect(rect, DefaultPaint);
    }

    public void DrawCircleMatrix(
        SKCanvas canvas,
        SKRect rect,
        int columns,
        int rows,
        float radius,
        SKPaint? paint = null)
    {
        paint ??= DefaultPaint;

        var xSpacing = (rect.Width - radius * 2) / (columns - 1);
        var ySpacing = (rect.Height - radius * 2) / (rows - 1);

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Black;
                canvas.DrawCircle(
                    rect.Left + radius + i * xSpacing,
                    rect.Top + radius + j * ySpacing,
                    radius,
                    paint
                );
            }
        }
    }

    public void FillCircles(
        SKCanvas canvas,
        int from,
        int to,
        SKPaint paint1,
        SKPaint paint2 = null!)
    {
        //If paint2 is null draw fill, else draw gradient
        // paint.Style = SKPaintStyle.Fill;
        // paint.Color = SKColors.Red;
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
        paint ??= DefaultPaint;
        font ??= DefaultFont;
        canvas.DrawText(text, x, y, align, font, paint);
    }

    public SKColor RandomColor()
    {
        var rnd = new Random();
        return SKColor.FromHsl(rnd.Next(0, 360), rnd.Next(40, 80), rnd.Next(40, 90));
    }
}