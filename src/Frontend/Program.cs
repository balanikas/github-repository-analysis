using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using RepositoryAnalysis;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });

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
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Information)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
var configOptions = builder.Configuration.GetSection(GitHubOptions.GitHub);
var instance = configOptions.Get<GitHubOptions>();
builder.Services.AddAppServices(instance!);
builder.Services.Configure<GitHubOptions>(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public partial class Program
{
}