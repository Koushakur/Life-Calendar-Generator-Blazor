using Microsoft.EntityFrameworkCore;

namespace LifeCalendar.BlazorApp.Data;

public class ImageContext(IConfiguration configuration) : DbContext
{
    protected readonly IConfiguration Configuration = configuration;
    public DbSet<ImageEntity>? Images { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(Configuration.GetConnectionString("ImageDb"));
    }
}