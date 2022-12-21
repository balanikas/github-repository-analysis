using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class LargeFilesRuleApplicator : IRuleApplicator
{
    public string RuleName => "large files";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
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
        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = showExamples,
                Text = @"
Large files contained in a repository might be a sign of unoptimized repository. 
",
                AboutUrl = "https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github",
                AboutHeader = "about large files"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            IReadOnlyList<GitTree.Node> e) =>
            e.Any()
                ? (Diagnosis.Warning, $"found {e.Count} big files")
                : (Diagnosis.Info, "did not find any large files");
    }
}