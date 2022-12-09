using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class DocumentationAnalyzer : IAnalyzer
{
    private readonly ILogger<DocumentationAnalyzer> _logger;

    public DocumentationAnalyzer(ILogger<DocumentationAnalyzer> logger)
    {
        _logger = logger;
    }
    
    public async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var rules = new List<Rule>
        {
            _logger.LogPerf( () => GetReadmeRule(context)),
            _logger.LogPerf( () => GetDescriptionRule(context)),
            _logger.LogPerf( () => GetHomePageUrlRule(context)),
            _logger.LogPerf( () => GetChangeLogRule(context)),
            _logger.LogPerf( () => GetTopicsRule(context)),
        };
        return await Task.FromResult(rules);
    }

    private Rule GetTopicsRule(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();
        return Rule.Topics(diagnosis, note);

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.RepositoryTopics.TotalCount > 0
                ? (Diagnosis.Info, $"found {context.Repo.RepositoryTopics.TotalCount} topics")
                : (Diagnosis.Warning, "no topics found");
    }

    private Rule GetChangeLogRule(
        AnalysisContext context)
    {
        var node = context.GitTree.FirstFileOrDefault(
            x => x.PathEndsWith("changelog.md", "change_log.md", "releasenotes.md", "release_notes.txt", "changelog.txt", "change_log.txt", "releasenotes.txt",
                "release_notes.txt"));
        var (diagnosis, note) = GetDiagnosis(node);
        return Rule.ChangeLog(diagnosis, note) with
        {
            ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
        };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
    }

    private Rule GetHomePageUrlRule(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();
        //todo: check head request if url exists
        return Rule.HomePage(diagnosis, note) with { ResourceName = context.Repo.HomepageUrl, ResourceUrl = context.Repo.HomepageUrl };

        (Diagnosis, string) GetDiagnosis() =>
            !string.IsNullOrEmpty(context.Repo.HomepageUrl)
                ? (Diagnosis.Info, "found homepage")
                : (Diagnosis.Warning, "missing");
    }

    private Rule GetDescriptionRule(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();

        return Rule.Description(diagnosis, note);

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.Description is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
    }

    private Rule GetReadmeRule(
        AnalysisContext context)
    {
        var node = context.GitTree.FirstFileOrDefault(x => x.PathEquals("readme", "readme.md", "readme.txt", "readme.rst"));
        var (diagnosis, note) = GetDiagnosis(node);

        return Rule.Readme(diagnosis, note) with { ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context) };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e)
        {
            return e is not null
                ? e.Item.Size switch
                {
                    < 200 => (Diagnosis.Warning, "readme is too short"),
                    _ => (Diagnosis.Info, "found")
                }
                : (Diagnosis.Error, "missing");
        }
    }
}