using LifeCalendar.BlazorApp.Data;
using Xunit;

public class ImageEntityTests
{
    [Fact]
    public void Should_Create_ImageEntity_With_Valid_Data()
    {
        // Arrange
        var id = Guid.NewGuid();
        var imageData = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var imageEntity = new ImageEntity
        {
            Id = id,
            ImageData = imageData
        };

        // Assert
        Assert.Equal(id, imageEntity.Id);
        Assert.Equal(imageData, imageEntity.ImageData);
    }

    [Fact]
    public void Should_Throw_On_Missing_ImageData()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var imageEntity = new ImageEntity
            {
                Id = id,
                ImageData = null! // Simulerar att propertyn inte är satt
            };
        });
    }
}