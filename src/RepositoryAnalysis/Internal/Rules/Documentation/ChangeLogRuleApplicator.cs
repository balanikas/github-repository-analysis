using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

public class ChangeLogRuleApplicator : IRuleApplicator
{
    public string RuleName => "changelog";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var node = context.GitTree.FirstFileOrDefault(
            x => x.PathEndsWith("changelog.md", "change_log.md", "releasenotes.md", "release_notes.txt", "changelog.txt", "change_log.txt", "releasenotes.txt",
                "release_notes.txt"));
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
                    Text = $@"
A changelog is a kind of summary of all your changes. 
It should be easy to understand both by the users using your project and the developers working on it.
Adding a CHANGELOG.md file in the repo root is a good start.
<br/>
Note: this currently only look for related files in the repo root, and does not look in 
{Shared.CreateLink("https://help.github.com/articles/creating-releases/", "Github Releases")}"
                }
            } with
            {
                ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
            };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
    }
}