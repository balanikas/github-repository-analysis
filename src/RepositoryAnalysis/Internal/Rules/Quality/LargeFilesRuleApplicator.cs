using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class LargeFilesRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string WhyBadIdea = "Why is having large files in a github repository a bad idea?";
    [RuleGuidance] private const string Alternative = "What should I do if I need to store large files in a github repository?";
    [RuleGuidance] private const string HowToDetect = "How can I find out if a repository contains large files?";

    private readonly IGpt3Client _gpt3Client;

    public LargeFilesRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "large files";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var nodes = context.GitTree.FilesRecursive(x => x.Item.Size > 100_000_000);
        var showExamples = "";
        if (nodes.Any())
            showExamples = @$"
Some examples: 
<br/>
{string.Join("<br/>", nodes.Take(3).Select(x => new { x.Item.Path, Size = x.Item.Size / 1000000 + " Mb" }))}";

        var diagnostics = nodes.Any()
            ? new RuleDiagnostics(Diagnosis.Warning, $"found {nodes.Count} big files (larger than 100Mb)", showExamples)
            : new RuleDiagnostics(Diagnosis.Info, "did not find any large files");

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(HowToDetect, Alternative),
            Text = await _gpt3Client.GetCompletion(WhyBadIdea),
            AboutLink = new Link("about large files",
                "https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github")
        });
    }
}