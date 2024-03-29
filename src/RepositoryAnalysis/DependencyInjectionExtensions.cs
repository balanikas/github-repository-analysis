using System.Net;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using RepositoryAnalysis.Internal;
using RepositoryAnalysis.Internal.Rules;
using RepositoryAnalysis.Internal.TextGeneration;

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
        services
            .AddHttpClient<IGpt3Client, Gpt3Client>()
            .ConfigurePrimaryHttpMessageHandler(x => new ClientSideRateLimitedHandler(new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = 16,
                QueueLimit = 16,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            })))
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

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
        services.AddHostedService<TextGenerationService>();

        void AddInterfaceImplementations<T>()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x is { IsInterface: false, IsAbstract: false } && x.GetInterface(typeof(T).Name) != null);

            foreach (var type in types) services.AddSingleton(typeof(T), type.UnderlyingSystemType);
        }
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(int.MaxValue, retryAttempt => Math.Pow(2, retryAttempt) < 10
                ? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                : TimeSpan.FromSeconds(10));
    }
}