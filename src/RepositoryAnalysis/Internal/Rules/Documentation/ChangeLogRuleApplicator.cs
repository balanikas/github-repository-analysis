using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class ChangeLogRuleApplicator : IRuleApplicator
{
    public string RuleName => "changelog";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var node = context.GitTree.FirstFileOrDefault(
                x => x.PathEndsWith("changelog.md", "change_log.md", "releasenotes.md", "release_notes.txt", "changelog.txt", "change_log.txt",
                    "releasenotes.txt",
                    "release_notes.txt"));
            var release = context.Repo.Releases.Edges?.SingleOrDefault()?.Node;

            if (node is not null)
                return new RuleDiagnostics(Diagnosis.Info, "found", null, node.GetLink(context));
            return release is not null
                ? new RuleDiagnostics(Diagnosis.Info, "found", null, new Link(release.Name!, release.Url.ToString()))
                : new RuleDiagnostics(Diagnosis.Warning, "missing");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
A changelog is a kind of summary of all your changes. 
It should be easy to understand both by the users using your project and the developers working on it.
Adding a CHANGELOG.md file in the repo root is a good start. Or use the Github Releases feature.
<br/>"
        });
    }
}