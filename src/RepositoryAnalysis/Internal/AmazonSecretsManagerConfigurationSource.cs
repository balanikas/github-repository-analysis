using Microsoft.Extensions.Configuration;

namespace RepositoryAnalysis.Internal;

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
        IConfigurationBuilder builder) =>
        new AmazonSecretsManagerConfigurationProvider(_region, _secretName);
}