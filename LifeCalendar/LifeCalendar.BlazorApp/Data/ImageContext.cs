using Microsoft.EntityFrameworkCore;

namespace LifeCalendar.BlazorApp.Data;

public class ImageContext : DbContext
{
    private readonly IConfiguration _configuration;

    public ImageContext(DbContextOptions<ImageContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<ImageEntity>? Images { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_configuration != null)
        {
            optionsBuilder.UseSqlite(_configuration.GetConnectionString("ImageDb"));
        }
    }
}