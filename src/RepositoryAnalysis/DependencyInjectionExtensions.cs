using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryAnalysis.Internal;

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
        services.AddOptions<GitHubOptions>(GitHubOptions.GitHub);
        services.AddHttpClient<GitHubGraphQlClient>();
        services.AddHttpClient<RepositoryVerifier>();
        services.AddTransient<AnalysisService>();
        services.AddTransient<GitHubRestClient>();
        services.AddTransient<AnalysisCache>();
        services.AddTransient<AnalysisContext>();
        services.AddTransient<OverViewAnalyzer>();
        services.AddTransient<DocumentationAnalyzer>();
        services.AddTransient<QualityAnalyzer>();
        services.AddTransient<SecurityAnalyzer>();
        services.AddTransient<CommunityAnalyzer>();
        services.AddTransient<LanguageSpecificAnalyzer>();
    }
}