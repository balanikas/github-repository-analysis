using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class ReadmeRuleApplicator : IRuleApplicator
{
    public string RuleName => "readme";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var node = context.GitTree.FirstFileOrDefault(x => x.PathEquals("readme", "readme.md", "readme.txt", "readme.rst"));
        var diagnostics = node is not null
            ? node.Item.Size switch
            {
                < 200 => new(Diagnosis.Warning, "readme is too short", null, node.GetLink(context)),
                _ => new(Diagnosis.Info, "found", null, node.GetLink(context))
            }
            : new RuleDiagnostics(Diagnosis.Error, "missing");

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
A repository should contain a readme file, to tell other people why your project is useful, what they can do with your project, and how they can use it.",
            AboutLink = new("about readmes",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Error ? context.GetCommunityLink() : null
        });
    }
}