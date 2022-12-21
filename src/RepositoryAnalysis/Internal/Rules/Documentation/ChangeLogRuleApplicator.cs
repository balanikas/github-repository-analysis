using RepositoryAnalysis.Internal.GraphQL;
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
        var release = context.Repo.Releases.Edges?.SingleOrDefault()?.Node;
        var (diagnosis, note, resourceName, resourceUrl) = GetDiagnosis(node, release);

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
A changelog is a kind of summary of all your changes. 
It should be easy to understand both by the users using your project and the developers working on it.
Adding a CHANGELOG.md file in the repo root is a good start. Or use the Github Releases feature.
<br/>"
            },
            ResourceName = resourceName,
            ResourceUrl = resourceUrl
        };

        (Diagnosis, string, string?, string?) GetDiagnosis(
            GitTree.Node? n,
            IGetRepo_Repository_Releases_Edges_Node? r)
        {
            if (n is not null)
                return (Diagnosis.Info, "found", node?.Item.Path, node.GetUrl(context));
            return r is not null
                ? (Diagnosis.Info, "found", r.Name, r.Url.ToString())
                : (Diagnosis.Warning, "missing", null, null);
        }
    }
}