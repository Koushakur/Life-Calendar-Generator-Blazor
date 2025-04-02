using Bunit;
using LifeCalendar.BlazorApp.Components.Pages;
using Xunit;

public class HomePageTests : TestContext
{
    [Fact]
    public void HomePage_ShouldRender_Title()
    {
        // Arrange: Render komponenten
        var component = RenderComponent<Home>();

        // Act & Assert: Kontrollera om rubriken finns och innehållet är korrekt
        component.MarkupMatches(@"
            <div class=""container"">
                <h1>Welcome to Your Life Calendar</h1>
                <div class=""flexbox"">
                    <div class=""textContainer"">
                        <p>Time flies—but what if you could see your whole life at a glance? A Life Calendar breaks down your years into weeks, giving you a visual way to track progress, set goals, and reflect on the big picture.</p>
                        <p>Whether you're planning for the future, appreciating the present, or gaining perspective on how you spend your time, our Life Calendar app makes it easy to create and customize your own. Start mapping your life today!</p>
                        <p>To get started visualizing your life visit the
                            <a href=""/app"">app page</a>
                        </p>
                    </div>
                    <div class=""previewContainer"">
                        <img class=""sample"" src=""images/SamplePreview.png"" />
                    </div>
                </div>
            </div>
        ");
    }

    
}