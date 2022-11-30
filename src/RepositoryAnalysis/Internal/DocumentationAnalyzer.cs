using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class DocumentationAnalyzer : IAnalyzer
{
    public async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var rules = new List<Rule>
        {
            GetReadmeRule(context),
            GetDescriptionRule(context),
            GetHomePageUrlRule(context),
            GetChangeLogRule(context),
            GetTopicsRule(context)
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
        var entry = Shared.GetFirstBlob(context.RootEntries,
            x => x.PathEndsWith("changelog.md", "change_log.md", "releasenotes.md", "release_notes.txt", "changelog.txt", "change_log.txt", "releasenotes.txt",
                "release_notes.txt"));
        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.ChangeLog(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e) =>
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
        var entry = Shared.GetFirstBlob(context.RootEntries, x => x.PathEquals("readme", "readme.md", "readme.txt", "readme.rst"));
        var (diagnosis, note) = GetDiagnosis(entry);

        return Rule.Readme(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            return e is not null
                ? e.Size switch
                {
                    < 200 => (Diagnosis.Warning, "readme is too short"),
                    _ => (Diagnosis.Info, "found")
                }
                : (Diagnosis.Error, "missing");
        }
    }
}