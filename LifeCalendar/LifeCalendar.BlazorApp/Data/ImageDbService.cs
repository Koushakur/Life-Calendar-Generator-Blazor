using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace LifeCalendar.BlazorApp.Data;

public class ImageDbService(IDbContextFactory<ImageContext> dbContextFactory)
{
    IDbContextFactory<ImageContext> _dbContextFactory { get; set; } = dbContextFactory;

    public async Task<bool> AddImageToDb(byte[] imageData)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var tEntity = new ImageEntity
            {
                Id = Guid.NewGuid(),
                ImageData = imageData
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
            var list = await context.Images!.ToListAsync();
            return list;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null!;
    }

    public async Task<List<ImageEntity>> GetXImages(int count)
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var list = await context.Images!.Take(count).ToListAsync();
            return list;
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
            return context.Images!.FirstOrDefault(x => x.Id == id)!;
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null!;
    }
}