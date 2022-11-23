using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Quality;

public class QualityAnalyzer
{
    private readonly AnalysisContext _context;
    private readonly GitHubRestClient _gitHubRestClient;

    public QualityAnalyzer(
        AnalysisContext context,
        GitHubRestClient gitHubRestClient)
    {
        _context = context;
        _gitHubRestClient = gitHubRestClient;
    }

    public async Task<IReadOnlyList<Rule>> Analyze()
    {
        var rules = new List<Rule>
        {
            GetLicenseRule(),
            await GetGitIgnoreRule(),
            GetDockerIgnoreRule(),
            GetEditorConfigRule(),
            GetLargeFilesRule()
        };

        return await Task.FromResult(rules);
    }

    private Rule GetDockerIgnoreRule()
    {
        var dockerFile = Shared.GetBlobRecursive(_context.RootEntries, x => x.PathEquals("Dockerfile"));
        var dockerIgnore = Shared.GetBlobRecursive(_context.RootEntries, x => x.PathEquals("Dockerfile"));
        
        var (diagnosis, note) = GetDiagnosis(dockerFile, dockerIgnore);
        return Rule.DockerFile(diagnosis, note) with 
            { ResourceName = dockerFile?.Path, ResourceUrl = Shared.GetEntryUrl(_context, dockerFile) };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? file,
            GitHubGraphQlClient.Entry? ignore)
        {
            return file is not null && ignore is not null
                ? (Diagnosis.Info, "found docker file and docker ignore")
                : ignore is null
                    ? (Diagnosis.Warning, "found docker file but no docker ignore")
                    : (Diagnosis.Warning, "found docker ignore but no docker file");
        }
    }


    private Rule GetLicenseRule()
    {
        var license = _context.Repo.LicenseInfo;
        var (diagnosis, note) = GetDiagnosis(license);

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.LicenseInfo? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
        }

        return Rule.License(diagnosis, note) with { ResourceName = license?.Name, ResourceUrl = license?.Url };
    }

    private async Task<Rule> GetGitIgnoreRule()
    {
        var entry = Shared.GetSingleBlob(_context.RootEntries, x => x.PathEquals(".gitignore"));
        var (diagnosis, note, details) = await GetDiagnosis(entry);
        return Rule.GitIgnore(diagnosis, note, details) with
        {
            ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(_context, entry)
        };

        async Task<(Diagnosis, string, string)> GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            if (_context.Repo.PrimaryLanguage is null) return (Diagnosis.Warning, "no primary language found", "");

            var (templateName, ignoreList) = await _gitHubRestClient.GetGitIgnoreRules(_context.Repo.PrimaryLanguage.Name);
            var ignoredFiles = new List<string>();
            Shared.AnalyzeRecursive(_context.RootEntries,
                x => x.IsTree() ? ignoreList.IsIgnored(x.Path, true) : ignoreList.IsIgnored(x.Path, false),
                (
                    x,
                    _) => ignoredFiles.Add(x.Path));

            var dets = ignoredFiles.Any()
                ? $@"
According to the recommended gitignore rules for the language of the repo, {templateName}.gitignore, 
these files should not exist in the repo. See {Shared.CreateLink("https://github.com/github/gitignore", "Recommended Ignore Files")}
<br/>
Showing first 100 files:
<br/>
<br/>
{string.Join("<br/>", ignoredFiles.Take(100))}"
                : "";

            return e is not null
                ? ignoredFiles.Any()
                    ? (Diagnosis.Warning, "found but looks incomplete", details: dets)
                    : (Diagnosis.Info, "found", details: dets)
                : (Diagnosis.Error, "missing", details: dets);
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
{string.Join("<br/>", entries.Take(3).Select(x => new { x.Path, Size = x.Size / 1000000 + " Mb" }))}";

        var (diagnosis, note) = GetDiagnosis(entries);
        return Rule.LargeFiles(diagnosis, note, showExamples);

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitHubGraphQlClient.Entry> e)
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
        return Rule.EditorConfig(diagnosis, note) with 
            { ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(_context, entry) };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            return e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
        }
    }
}