using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;

namespace RepositoryAnalysis.Internal;

internal class AmazonSecretsManagerConfigurationProvider : ConfigurationProvider
{
    private readonly string _region;
    private readonly string _secretName;

    public AmazonSecretsManagerConfigurationProvider(
        string region,
        string secretName)
    {
        _region = region;
        _secretName = secretName;
    }

    public override void Load()
    {
        var chain = new CredentialProfileStoreChain();
        if (chain.ListProfiles().Any())
        {
            var secret = GetSecret();
            Data = JsonSerializer.Deserialize<Dictionary<string, string>>(secret);
        }
        else
        {
            Console.WriteLine("no profiles, using env vars instead");
        }
    }

    private string GetSecret()
    {
        var request = new GetSecretValueRequest
        {
            SecretId = _secretName,
            VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
        };

        using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region));
        var response = client.GetSecretValueAsync(request).Result;

        string secretString;
        if (response.SecretString != null)
        {
            secretString = response.SecretString;
        }
        else
        {
            var memoryStream = response.SecretBinary;
            var reader = new StreamReader(memoryStream);
            secretString =
                Encoding.UTF8
                    .GetString(Convert.FromBase64String(reader.ReadToEnd()));
        }

        return secretString;
    }
}