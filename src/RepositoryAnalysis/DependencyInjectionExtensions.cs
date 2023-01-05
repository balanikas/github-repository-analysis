using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryAnalysis.Internal;
using RepositoryAnalysis.Internal.Rules;

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
        this IServiceCollection services,
        GitHubOptions gitHubOptions)
    {
        services.AddSingleton<GitHubGraphQlClient>();
        services.AddTransient<IAnalysisService, AnalysisService>();
        services.AddTransient<GitHubRestClient>();
        services.AddTransient<AnalysisCache>();
        services.AddTransient<AnalysisContext>();
        services.AddTransient<OverView>();
        AddInterfaceImplementations<IAnalyzer>();
        AddInterfaceImplementations<IRuleApplicator>();
        services.AddSingleton<RulesRepository>();

        services
            .AddGithubClient()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/graphql");
                client.DefaultRequestHeaders
                    .Add("Authorization", "Bearer " + Environment.GetEnvironmentVariable("GitHub__Token"));
            });

        void AddInterfaceImplementations<T>()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x is { IsInterface: false, IsAbstract: false } && x.GetInterface(typeof(T).Name) != null);

            foreach (var type in types) services.AddSingleton(typeof(T), type.UnderlyingSystemType);
        }
    }
}