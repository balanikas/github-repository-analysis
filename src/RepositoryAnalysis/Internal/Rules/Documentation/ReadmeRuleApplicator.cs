using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

public class ReadmeRuleApplicator : IRuleApplicator
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
        var (diagnosis, note) = GetDiagnosis(node);

        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = null,
                Text = @"
A repository should contain a readme file, to tell other people why your project is useful, what they can do with your project, and how they can use it.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes",
                AboutHeader = "about readmes"
            },
            ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
        };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e)
        {
            return e is not null
                ? e.Item.Size switch
                {
                    < 200 => (Diagnosis.Warning, "readme is too short"),
                    _ => (Diagnosis.Info, "found")
                }
                : (Diagnosis.Error, "missing");
        }
    }
}