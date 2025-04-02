using System;
using System.Linq;
using System.Threading.Tasks;
using LifeCalendar.BlazorApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LifeCalendar.Tests.Data;

public class ImageDbServiceTests
{
    private static IDbContextFactory<ImageContext> CreateInMemoryDbContextFactory()
    {
        var options = new DbContextOptionsBuilder<ImageContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Skapar en unik databas för varje test
            .Options;

        return new DbContextFactoryWrapper<ImageContext>(options);
    }

    [Fact]
    public async Task AddImageToDb_ShouldAddImageAndReturnTrue()
    {
        // Arrange
        var dbContextFactory = CreateInMemoryDbContextFactory();
        var service = new ImageDbService(dbContextFactory);
        var imageData = new byte[] { 1, 2, 3 };

        // Act
        var result = await service.AddImageToDb(imageData);

        // Assert
        Assert.True(result);

        var images = await service.GetAllImages();
        Assert.Single(images);
        Assert.Equal(imageData, images[0].ImageData);
    }

    [Fact]
    public async Task GetAllImages_ShouldReturnListOfImages()
    {
        // Arrange
        var dbContextFactory = CreateInMemoryDbContextFactory();
        var service = new ImageDbService(dbContextFactory);

        await service.AddImageToDb(new byte[] { 1, 2, 3 });
        await service.AddImageToDb(new byte[] { 4, 5, 6 });

        // Act
        var images = await service.GetAllImages();

        // Assert
        Assert.Equal(2, images.Count);
    }

    [Fact]
    public async Task GetXImages_ShouldReturnSpecificNumberOfImages()
    {
        // Arrange
        var dbContextFactory = CreateInMemoryDbContextFactory();
        var service = new ImageDbService(dbContextFactory);

        await service.AddImageToDb(new byte[] { 1 });
        await service.AddImageToDb(new byte[] { 2 });
        await service.AddImageToDb(new byte[] { 3 });

        // Act
        var images = await service.GetXImages(2);

        // Assert
        Assert.Equal(2, images.Count);
    }

    [Fact]
    public async Task GetImageById_ShouldReturnCorrectImage_WhenIdExists()
    {
        // Arrange
        var dbContextFactory = CreateInMemoryDbContextFactory();
        var service = new ImageDbService(dbContextFactory);
        var imageData = new byte[] { 1, 2, 3 };

        await service.AddImageToDb(imageData);
        var addedImage = (await service.GetAllImages()).First();

        // Act
        var image = await service.GetImageById(addedImage.Id);

        // Assert
        Assert.NotNull(image);
        Assert.Equal(addedImage.Id, image.Id);
        Assert.Equal(imageData, image.ImageData);
    }

    [Fact]
    public async Task GetImageById_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        var dbContextFactory = CreateInMemoryDbContextFactory();
        var service = new ImageDbService(dbContextFactory);

        // Act
        var image = await service.GetImageById(Guid.NewGuid());

        // Assert
        Assert.Null(image);
    }
}

public class DbContextFactoryWrapper<TContext> : IDbContextFactory<TContext> where TContext : DbContext
{
    private readonly DbContextOptions<TContext> _options;

    public DbContextFactoryWrapper(DbContextOptions<TContext> options)
    {
        _options = options;
    }

    public TContext CreateDbContext()
    {
        return (TContext)Activator.CreateInstance(typeof(TContext), _options)!;
    }
}