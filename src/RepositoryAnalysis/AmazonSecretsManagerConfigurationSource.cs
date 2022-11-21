using Microsoft.Extensions.Configuration;

namespace RepositoryAnalysis;

public class AmazonSecretsManagerConfigurationSource : IConfigurationSource
{
    private readonly string _region;
    private readonly string _secretName;

    public AmazonSecretsManagerConfigurationSource(
        string region,
        string secretName)
    {
        _region = region;
        _secretName = secretName;
    }

    public IConfigurationProvider Build(
        IConfigurationBuilder builder)
    {
        return new AmazonSecretsManagerConfigurationProvider(_region, _secretName);
    }
}