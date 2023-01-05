using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class LargeFilesRuleApplicator : IRuleApplicator
{
    public string RuleName => "large files";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
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
            Text = @"
Large files contained in a repository might be a sign of unoptimized repository. 
",
            AboutLink = new Link("about large files",
                "https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github")
        });
    }
}