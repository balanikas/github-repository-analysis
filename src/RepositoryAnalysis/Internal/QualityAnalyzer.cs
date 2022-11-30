using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class QualityAnalyzer : IAnalyzer
{
    public async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var rules = new List<Rule>
        {
            await GetGitIgnoreRule(context),
            GetDockerIgnoreRule(context),
            GetEditorConfigRule(context),
            GetLargeFilesRule(context)
        };

        return await Task.FromResult(rules);
    }

    private Rule GetDockerIgnoreRule(
        AnalysisContext context)
    {
        var dockerFile = Shared.GetFirstBlobRecursive(context.RootEntries, x => x.PathEquals("Dockerfile"));
        var dockerIgnore = Shared.GetFirstBlobRecursive(context.RootEntries, x => x.PathEquals(".dockerignore"));

        var (diagnosis, note) = GetDiagnosis(dockerFile, dockerIgnore);
        return Rule.DockerFile(diagnosis, note) with
        {
            ResourceName = dockerFile?.Path, ResourceUrl = Shared.GetEntryUrl(context, dockerFile)
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? file,
            GitHubGraphQlClient.Entry? ignore) =>
            file is not null && ignore is not null
                ? (Diagnosis.Info, "found docker file and docker ignore")
                : file is not null && ignore is null
                    ? (Diagnosis.Warning, "found docker file but no docker ignore")
                    : (Diagnosis.Info, "not found");
    }


    private async Task<Rule> GetGitIgnoreRule(
        AnalysisContext context)
    {
        var entry = Shared.GetSingleBlob(context.RootEntries, x => x.PathEquals(".gitignore"));
        var (diagnosis, note, details) = await GetDiagnosis(entry);
        return Rule.GitIgnore(diagnosis, note, details) with
        {
            ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(context, entry)
        };

        async Task<(Diagnosis, string, string)> GetDiagnosis(
            GitHubGraphQlClient.Entry? e)
        {
            if (context.Repo.PrimaryLanguage is null) return (Diagnosis.Warning, "no primary language found", "");

            var (templateName, ignoreList) = await context.RestClient.GetGitIgnoreRules(context.Repo.PrimaryLanguage.Name);
            var ignoredFiles = new List<string>();
            Shared.AnalyzeRecursive(context.RootEntries,
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

    private Rule GetLargeFilesRule(
        AnalysisContext context)
    {
        var entries = Shared.GetBlobsRecursive(context.RootEntries, x => x.Size > 10_000_000);
        var showExamples = "";
        if (entries.Any())
            showExamples = @$"
Some examples: 
<br/>
{string.Join("<br/>", entries.Take(3).Select(x => new { x.Path, Size = x.Size / 1000000 + " Mb" }))}";

        var (diagnosis, note) = GetDiagnosis(entries);
        return Rule.LargeFiles(diagnosis, note, showExamples);

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitHubGraphQlClient.Entry> e) =>
            e.Any()
                ? (Diagnosis.Warning, $"found {e.Count()} big files")
                : (Diagnosis.Info, "did not find any large files");
    }

    private Rule GetEditorConfigRule(
        AnalysisContext context)
    {
        var entry = Shared.GetSingleBlobRecursive(context.RootEntries, x => x.PathEquals(".editorconfig"));
        var (diagnosis, note) = GetDiagnosis(entry);
        return Rule.EditorConfig(diagnosis, note) with
        {
            ResourceName = entry?.Path, ResourceUrl = Shared.GetEntryUrl(context, entry)
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.Entry? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
    }
}