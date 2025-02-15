using JunkWaxDetection.LiveDemo;
using JunkWaxDetection.LiveDemo.CardList;
using JunkWaxDetection.LiveDemo.Components;
using JunkWaxDetection.LiveDemo.ML;
using JunkWaxDetection.LiveDemo.OCR;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<HubOptions>(options =>
{
    //We have to bump up the max message size for the webcam image
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MiB
}).AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHttpClient();
builder.Services.AddTransient<IMLController, MLController>();
builder.Services.AddTransient<IOCRController, OCRController>();
builder.Services.AddTransient<ICardListController, CardListController>();

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