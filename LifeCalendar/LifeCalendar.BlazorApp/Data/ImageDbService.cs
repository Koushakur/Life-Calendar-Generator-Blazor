using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace LifeCalendar.BlazorApp.Data;

public class ImageDbService
{
    [Inject] IDbContextFactory<ImageContext> _dbContextFactory { get; set; } = null!;
    // ImageContext _dbContext;

    public async Task<bool> AddImageToDb(byte[] imageData)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            // var tEntity = new ImageEntity(
            //     Guid.NewGuid(),
            //     imageData
            // );
            var tEntity = new ImageEntity
            {
                Id = Guid.NewGuid(),
                Image = imageData
            };

            context.Add(tEntity);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }

    public async Task<List<ImageEntity>> GetAllImages()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Images.ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null!;
    }

    public async Task<ImageEntity> GetImageById(Guid id)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return context.Images.FirstOrDefault(x => x.Id == id)!;
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null!;
    }
}