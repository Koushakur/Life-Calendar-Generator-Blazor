using Microsoft.EntityFrameworkCore;

namespace LifeCalendar.BlazorApp.Data;

public class ImageContext(DbContextOptions<ImageContext> options) : DbContext(options)
{
    public DbSet<ImageEntity>? Images { get; set; }

}