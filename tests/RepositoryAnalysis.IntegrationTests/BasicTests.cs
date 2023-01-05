using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RepositoryAnalysis.IntegrationTests;

public class BasicTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        Environment.SetEnvironmentVariable("GitHub__Token", "");
    }

    // [Theory]
    // [InlineData("https://github.com/balanikas/github-repository-analysis")]
    // public async Task Get_EndpointsReturnSuccessAndCorrectContentType(
    //     string repoUrl)
    // {
    //     var analysisService = _factory.Services.GetRequiredService<IAnalysisService>();
    //     var analysis = await analysisService.GetAnalysis(repoUrl);
    //     analysis.Status.Should().Be(RepoAnalysis.AnalysisStatus.Ok);
    // }
}
