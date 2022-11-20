using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using RepositoryAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddAWSProvider();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<AnalysisService>();
builder.Services.AddHttpClient<GitHubApi>();
builder.Services.AddMudServices();
builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection(GitHubOptions.GitHub));

var app = builder.Build();

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