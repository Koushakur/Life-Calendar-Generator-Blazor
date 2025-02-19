using Blazored.LocalStorage;
using Blazored.SessionStorage;
using LifeCalendar.BlazorApp.Components;
using LifeCalendar.BlazorApp.Data;
using LifeCalendar.BlazorApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<SkiaService>();
builder.Services.AddBlazorBootstrap();

builder.Services.AddDbContextFactory<ImageContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("ImageDb"))
);
builder.Services.AddSingleton<ImageDbService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();