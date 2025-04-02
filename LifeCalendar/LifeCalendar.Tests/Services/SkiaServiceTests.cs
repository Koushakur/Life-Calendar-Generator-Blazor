using LifeCalendar.BlazorApp.Services;
using SkiaSharp;

namespace LifeCalendar.Tests.Services;

public class SkiaServiceTests
{
    private readonly SkiaService _skiaService;

    public SkiaServiceTests()
    {
        _skiaService = new SkiaService();
    }

    [Fact]
    public void Fill_ShouldClearCanvasWithGivenColor()
    {
        // Arrange
        using var surface = SKSurface.Create(new SKImageInfo(100, 100));
        var canvas = surface.Canvas;
        var color = SKColors.Red;

        // Act
        _skiaService.Fill(canvas, color);

        // Assert
        // Kontroll att canvas renderade rätt färg
        var image = surface.Snapshot();
        var pixel = GetPixelColor(image.PeekPixels(), 0, 0);
        Assert.Equal(color, pixel);
    }

    [Fact]
    public void DrawRectangle_ShouldDrawRectangleOnCanvas()
    {
        // Arrange
        using var surface = SKSurface.Create(new SKImageInfo(200, 200));
        var canvas = surface.Canvas;
        var rect = new SKRect(50, 50, 150, 150);

        // Act
        _skiaService.DrawRectangle(canvas, rect);

        // Assert
        // Sparar canvas till en bild för manuell verifiering
        SaveSurfaceToFile(surface, "DrawRectangle_Test.png");
        Assert.True(File.Exists("DrawRectangle_Test.png"));
    }

    [Fact]
    public async Task FetchFontFromGoogle_ShouldReturnTrueForValidFont()
    {
        // Arrange
        var fontName = "Roboto"; // Ett testfont som finns hos Google Fonts.

        // Act
        var result = await _skiaService.FetchFontFromGoogle(fontName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DrawCircleMatrix_ShouldDrawMatrixOfCircles()
    {
        // Arrange
        using var surface = SKSurface.Create(new SKImageInfo(300, 300));
        var canvas = surface.Canvas;
        var rect = new SKRect(50, 50, 250, 250);
        int columns = 3;
        int rows = 3;
        float radius = 10;

        // Act
        _skiaService.DrawCircleMatrix(canvas, rect, columns, rows, radius);

        // Assert
        // Sparar canvas till en bild för manuell verifiering
        SaveSurfaceToFile(surface, "DrawCircleMatrix_Test.png");
        Assert.True(File.Exists("DrawCircleMatrix_Test.png"));
    }

    [Fact]
    public void DrawText_ShouldDrawTextOnCanvas()
    {
        // Arrange
        using var surface = SKSurface.Create(new SKImageInfo(200, 200));
        var canvas = surface.Canvas;
        var text = "Hello, World!";
        float x = 100;
        float y = 100;

        // Act
        _skiaService.DrawText(canvas, text, x, y);

        // Assert
        // Sparar canvas till en bild för manuell verifiering
        SaveSurfaceToFile(surface, "DrawText_Test.png");
        Assert.True(File.Exists("DrawText_Test.png"));
    }

    [Fact]
    public void RandomColorString_ShouldReturnValidHexColor()
    {
        // Act
        var colorString = _skiaService.RandomColorString();

        // Assert
        Assert.NotNull(colorString);
        Assert.StartsWith("#", colorString); // Hexfärger börjar med #
        Assert.True(colorString.Length == 7); // Formatet är #RRGGBB
    }

    [Fact]
    public void RandomColorSkColor_ShouldReturnSKColor()
    {
        // Act
        var randomColor = _skiaService.RandomColorSkColor();

        // Assert
        Assert.IsType<SKColor>(randomColor);
    }

    // Hjälpmetoder

    // Hämtar färgen för en pixel från en bild
    private SKColor GetPixelColor(SKPixmap pixmap, int x, int y)
    {
        return pixmap.GetPixelColor(x, y);
    }

    // Sparar en yta till en bild för att kunna verifiera den visuellt
    private void SaveSurfaceToFile(SKSurface surface, string fileName)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(fileName);

        data.SaveTo(stream);
    }
}