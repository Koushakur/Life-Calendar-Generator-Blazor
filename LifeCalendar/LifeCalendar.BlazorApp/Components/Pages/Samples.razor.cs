using LifeCalendar.BlazorApp.Data;
using Microsoft.AspNetCore.Components;

namespace LifeCalendar.BlazorApp.Components.Pages;

public partial class Samples
{
    [Inject] ImageDbService ImageDb { get; set; } = null!;

    List<ImageEntity> images = null!;

    public Samples()
    {
        ;
    }

    protected override async Task OnInitializedAsync()
    {
        var imgs = await ImageDb.GetAllImages();
        // if (imgs.Count > 0)
        // {
        //     images = imgs;
        // }
        ;
    }
}