using SkiaSharp;

namespace LifeCalendar.BlazorApp.Services;

public class SkiaService
{
    public SKPaint defaultPaint;
    private SKFont defaultFont;

    public SkiaService()
    {
        defaultPaint = new()
        {
            Color = SKColors.Black,
            IsAntialias = true,
            StrokeWidth = 3,
            Style = SKPaintStyle.Stroke
        };

        defaultFont = new()
        {
            Typeface = SKTypeface.FromFamilyName("Atkinson Hyperlegible"),
            Size = 50
        };
    }

    public void Fill(SKCanvas canvas, SKColor color)
    {
        canvas.Clear(color);
    }

    public void DrawCircleMatrix(
        SKCanvas canvas,
        SKRect rect,
        int columns,
        int rows,
        float radius,
        SKPaint paint = null!)
    {
        paint ??= defaultPaint;

        //This math does not give wanted result, check it
        var xSpacing = (rect.Width - radius * 2) / (columns - 1);
        var ySpacing = (rect.Height - radius * 2) / (rows - 1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Black;
                canvas.DrawCircle(
                    rect.Left + i * xSpacing,
                    rect.Top + j * ySpacing,
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
        paint ??= defaultPaint;
        font ??= defaultFont;
        canvas.DrawText(text, x, y, align, font, paint);
    }
}
