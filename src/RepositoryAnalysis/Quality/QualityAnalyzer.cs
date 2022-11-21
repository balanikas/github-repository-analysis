using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Quality;

public class QualityAnalyzer
{
    private readonly AnalysisContext _context;

    public QualityAnalyzer(
        AnalysisContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rule>> Analyze()
    {
        var rules = new List<Rule>
        {
            GetLicenseRule(),
            GetGitIgnoreRule(),
            GetEditorConfigRule(),
            GetLargeFilesRule()
        };

        return await Task.FromResult(rules);
    }


    private Rule GetLicenseRule()
    {
        var license = _context.Repo.LicenseInfo;
        var (diagnosis, note) = GetDiagnosis(license);

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.LicenseInfo? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
        }

        return Rule.License(diagnosis, note) with { ResourceName = license?.Name, ResourceUrl = license?.Url };
    }

    private Rule GetGitIgnoreRule()
    {
        var entry = Shared.GetBlob(_context.RootEntries, x => x.PathEquals(".gitignore"));
        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.GitIgnore(diagnosis, note) with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(_context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing"); //todo: advice on which gitignore file to choose based on lang.
        }
    }

    private Rule GetLargeFilesRule()
    {
        var entries = Shared.GetBlobsRecursive(_context.RootEntries, x => x.Size > 10_000_000);
        var showExamples = "";
        if (entries.Any())
            showExamples = @$"
Some examples: 
<br/>
{string.Join("<br/>", entries.Take(3).Select(x => new { x.Path, Size = (x.Size / 1000000) + " Mb" }))}";

        var (diagnosis, note) = GetDiagnosis(entries);
        return Rule.LargeFiles(diagnosis, note, showExamples);

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitHubApi.Entry> e)
        {
            return e.Any()
                ? (Diagnosis.Warning, $"found {e.Count()} big files")
                : (Diagnosis.Info, "did not find any large files");
        }
    }


    private Rule GetEditorConfigRule()
    {
        var entry = Shared.GetBlobRecursive(_context.RootEntries, x => x.PathEquals(".editorconfig"));
        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.EditorConfig(diagnosis, note)with { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(_context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubApi.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
        }
    }
}