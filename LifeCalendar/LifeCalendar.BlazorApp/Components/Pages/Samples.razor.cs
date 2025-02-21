using LifeCalendar.BlazorApp.Data;
using Microsoft.AspNetCore.Components;

namespace LifeCalendar.BlazorApp.Components.Pages;

//TODO: Open larger preview window when clicking on image

public partial class Samples
{
    [Inject] ImageDbService? ImageDb { get; set; }

    private List<ImageEntity>? _images;

    protected override async Task OnInitializedAsync()
    {
        var imgList = await ImageDb!.GetXImages(40);
        _images = imgList is {Count: > 0} ? imgList : [];
    }
}