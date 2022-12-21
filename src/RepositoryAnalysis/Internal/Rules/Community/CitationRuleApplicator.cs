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
        var (diagnosis, note) = GetDiagnosis(node);

        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Diagnosis = diagnosis,
            Note = note,
            Explanation = new Explanation
            {
                Details = null,
                Text = @"
You can add a CITATION file to your repository to help users correctly cite your software.
",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-citation-files",
                AboutHeader = "about citation files",
                GuidanceUrl = null,
                GuidanceHeader = null
            }
        };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing citation file");
    }
}