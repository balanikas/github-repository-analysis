using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using RepositoryAnalysis;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Configuration.AddJsonFile("appsettings.json");
builder.Host.ConfigureAppConfiguration((
    _,
    configurationBuilder) =>
{
    configurationBuilder.AddAmazonSecretsManager("us-west-2", "github-pat");
});

builder.Services.AddRazorPages();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddAppServices();
builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection(GitHubOptions.GitHub));
builder.Services.Configure<GitHubOptions>(builder.Configuration);

var app = builder.Build();
//app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();