namespace LifeCalendar.BlazorApp.Data;

public class ImageEntity()
{
    public Guid Id { get; set; }
    public required byte[] Image { get; set; }
}