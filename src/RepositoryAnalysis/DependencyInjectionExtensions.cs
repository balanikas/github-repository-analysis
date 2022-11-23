using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RepositoryAnalysis;

public static class DependencyInjectionExtensions
{
    public static void AddAmazonSecretsManager(
        this IConfigurationBuilder configurationBuilder,
        string region,
        string secretName)
    {
        var configurationSource =
            new AmazonSecretsManagerConfigurationSource(region, secretName);

        configurationBuilder.Add(configurationSource);
    }

    public static void AddAppServices(
        this IServiceCollection services)
    {
        services.AddTransient<AnalysisService>();
        services.AddTransient<GitHubRestClient>();
    }
}