using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Documentation;

public class DocumentationAnalyzer
{
    private readonly AnalysisContext _context;

    public DocumentationAnalyzer(
        AnalysisContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rule>> Analyze()
    {
        var rules = new List<Rule>
        {
            GetReadmeRule(),
            GetDescriptionRule(),
            GetHomePageUrlRule(),
            GetChangeLogRule(),
            GetTopicsRule()
        };
        return await Task.FromResult(rules);
    }

    private Rule GetTopicsRule()
    {
        var (diagnosis, note) = GetDiagnosis();
        return Rule.Topics(diagnosis, note);

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.RepositoryTopics.TotalCount > 0
                ? (Diagnosis.Info, $"found {_context.Repo.RepositoryTopics.TotalCount} topics")
                : (Diagnosis.Warning, "no topics found");
        }
    }

    private Rule GetChangeLogRule()
    {
        var entry = Shared.GetSingleBlob(_context.RootEntries,
            x => x.PathEndsWith("changelog.md", "change_log.md", "releasenotes.md", "release_notes.txt", "changelog.txt", "change_log.txt", "releasenotes.txt",
                "release_notes.txt"));
        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.ChangeLog(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(_context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
        }
    }

    private Rule GetHomePageUrlRule()
    {
        var (diagnosis, note) = GetDiagnosis();
        //todo: check head request if url exists
        return Rule.HomePage(diagnosis, note) with { ResourceName = _context.Repo.HomepageUrl, ResourceUrl = _context.Repo.HomepageUrl };

        (Diagnosis, string) GetDiagnosis()
        {
            return !string.IsNullOrEmpty(_context.Repo.HomepageUrl)
                ? (Diagnosis.Info, "found homepage")
                : (Diagnosis.Warning, "missing");
        }
    }

    private Rule GetDescriptionRule()
    {
        var (diagnosis, note) = GetDiagnosis();

        return Rule.Description(diagnosis, note);

        (Diagnosis, string) GetDiagnosis()
        {
            return _context.Repo.Description is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
        }
    }

    private Rule GetReadmeRule()
    {
        var entry = Shared.GetFirstBlob(_context.RootEntries, x => x.PathEquals("readme", "readme.md", "readme.txt", "readme.rst"));
        var (diagnosis, note) = GetDiagnosis(entry);

        return Rule.Readme(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(_context, entry) };

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