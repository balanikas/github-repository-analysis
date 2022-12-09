using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class QualityAnalyzer : IAnalyzer
{
    private readonly ILogger<QualityAnalyzer> _logger;

    public QualityAnalyzer(ILogger<QualityAnalyzer> logger)
    {
        _logger = logger;
    }
    
    public async Task<IReadOnlyList<Rule>> Analyze(
        AnalysisContext context)
    {
        var rules = new List<Rule>
        {
            await _logger.LogPerfAsync(() => GetGitIgnoreRule(context)),
            _logger.LogPerf(() => GetDockerIgnoreRule(context)),
            _logger.LogPerf(() => GetEditorConfigRule(context)),
            _logger.LogPerf( () => GetLargeFilesRule(context))
        };

        return await Task.FromResult(rules);
    }

    private Rule GetDockerIgnoreRule(
        AnalysisContext context)
    {
        var dockerFile = context.GitTree.FirstFileOrDefault(x => x.PathEquals("Dockerfile"));
        var dockerIgnore = context.GitTree.FirstFileOrDefault(x => x.PathEquals(".dockerignore"));

        var (diagnosis, note) = GetDiagnosis(dockerFile, dockerIgnore);
        return Rule.DockerFile(diagnosis, note) with
        {
            ResourceName = dockerFile?.Item.Path, ResourceUrl = dockerFile.GetUrl(context)
        };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? file,
            GitTree.Node? ignore) =>
            file is not null && ignore is not null
                ? (Diagnosis.Info, "found docker file and docker ignore")
                : file is not null && ignore is null
                    ? (Diagnosis.Warning, "found docker file but no docker ignore")
                    : (Diagnosis.Info, "not found");
    }


    private async Task<Rule> GetGitIgnoreRule(
        AnalysisContext context)
    {
        var node = context.GitTree.SingleFileOrDefault(x => x.PathEquals(".gitignore"));
        var (diagnosis, note, details) = await GetDiagnosis(node);
        return Rule.GitIgnore(diagnosis, note, details) with
        {
            ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
        };

        async Task<(Diagnosis, string, string)> GetDiagnosis(
            GitTree.Node? e)
        {
            if (context.Repo.PrimaryLanguage is null) return (Diagnosis.Info, "no primary language found, will not analyze", "");

            var (templateName, ignoreList) = await context.RestClient.GetGitIgnoreRules(context.Repo.PrimaryLanguage.Name);
            var ignoredFiles = new List<string>();
            context.GitTree.AnalyzeRecursive(
                x => x.IsTree() ? ignoreList.IsIgnored(x.Item.Path, true) : ignoreList.IsIgnored(x.Item.Path, false),
                (
                    x,
                    _) => ignoredFiles.Add(x.Item.Path));

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
        var nodes = context.GitTree.FilesRecursive(x => x.Item.Size > 10_000_000);
        var showExamples = "";
        if (nodes.Any())
            showExamples = @$"
Some examples: 
<br/>
{string.Join("<br/>", nodes.Take(3).Select(x => new { x.Item.Path, Size = x.Item.Size / 1000000 + " Mb" }))}";

        var (diagnosis, note) = GetDiagnosis(nodes);
        return Rule.LargeFiles(diagnosis, note, showExamples);

        (Diagnosis, string) GetDiagnosis(
            IEnumerable<GitTree.Node> e) =>
            e.Any()
                ? (Diagnosis.Warning, $"found {e.Count()} big files")
                : (Diagnosis.Info, "did not find any large files");
    }

    private Rule GetEditorConfigRule(
        AnalysisContext context)
    {
        var node = context.GitTree.FirstFileOrDefaultRecursive(x => x.PathEquals(".editorconfig"));
        var (diagnosis, note) = GetDiagnosis(node);
        return Rule.EditorConfig(diagnosis, note) with
        {
            ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
        };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");
    }
}