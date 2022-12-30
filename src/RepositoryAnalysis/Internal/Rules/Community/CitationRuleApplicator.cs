using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class CitationRuleApplicator : IRuleApplicator
{
    public string RuleName => "citation";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
You can add a CITATION file to your repository to help users correctly cite your software.
",
            AboutUrl =
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-citation-files",
            AboutHeader = "about citation files"
        });

        RuleDiagnostics GetDiagnosis()
        {
            var citationFileNames = new[]
            {
                "CITATION",
                "CITATIONS",
                "CITATION.bib",
                "CITATIONS.bib",
                "CITATION.md",
                "CITATIONS.md",
                "CITATION.cff"
            };
            var node = context.GitTree.FirstFileOrDefault(
                x => citationFileNames.Contains(x.Item.Path, StringComparer.OrdinalIgnoreCase));
            return node is not null
                ? new RuleDiagnostics(Diagnosis.Info, "found")
                : new RuleDiagnostics(Diagnosis.Warning, "missing citation file");
        }
    }
}