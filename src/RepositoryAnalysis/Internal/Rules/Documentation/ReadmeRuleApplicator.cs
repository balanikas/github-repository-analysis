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
        bool IsReadme(
            GitTree.Node x) =>
            x.HasFileName("readme", "readme.md", "readme.txt", "readme.rst");

        var rootFile = context.GitTree.FirstFileOrDefault(IsReadme);
        var nonRootFiles = context.GitTree.FilesRecursive(IsReadme);

        var details =
            nonRootFiles.Count == 1
                ? ""
                : nonRootFiles.Count > 100
                    ? $"First 100 readme files <br/> {nonRootFiles.GetEmbeddedLinksAsString(context)}"
                    : $"All readme files <br/> {nonRootFiles.GetEmbeddedLinksAsString(context)}";

        var diagnostics = rootFile is not null
            ? rootFile.Item.Size switch
            {
                < 500 => new(Diagnosis.Warning, "root readme is too short (less than 500 chars long)", details, rootFile.GetLink(context)),
                _ => new(Diagnosis.Info, "found root readme root", details, rootFile.GetLink(context))
            }
            : new RuleDiagnostics(Diagnosis.Error, "missing root readme", details);

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