using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class IssueLabelsRuleApplicator : IRuleApplicator
{
    public string RuleName => "issue labels";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            if (!context.Repo.HasIssuesEnabled) return new RuleDiagnostics(Diagnosis.NotApplicable, "feature is disabled");
            if (context.Repo.Issues.Edges is null || !context.Repo.Issues.Edges.Any()) return new RuleDiagnostics(Diagnosis.NotApplicable, "no open issues");

            var nodes = context.Repo.Issues.Edges
                .Select(x => x!.Node!)
                .ToArray();
            var nodesWithoutLabels = nodes
                .Where(x => x.Labels is not null && x.Labels.TotalCount == 0)
                .ToArray();

            if (nodesWithoutLabels.Length <= 0)
                return new RuleDiagnostics(
                    Diagnosis.Info,
                    "issues are enabled and all open issues are labeled",
                    null,
                    new Link("issues", Path.Combine(context.Repo.Url.ToString(), "issues")));

            var percent = (int)Math.Round((double)(100 * nodesWithoutLabels.Length) / nodes.Length);
            var details = $"""
Sample of unlabeled issues: <br/>{string.Join("<br/>", nodesWithoutLabels.Take(5).Select(x => GetUrl(x, context)))}
""";
            return new RuleDiagnostics(Diagnosis.Warning, $"found {percent}% of {nodes.Length} issues unlabeled", details,
                new Link("unlabeled issues", Path.Combine(context.Repo.Url.ToString(), "issues?q=is%3Aopen+is%3Aissue+no%3Alabel")));
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
Issue labels let you categorize your work on GitHub, where development happens.
You may wish to turn issues off for your repository if you do not accept contributions or bug reports.
",
            AboutLink = new Link("about issue labels", "https://docs.github.com/en/issues/using-labels-and-milestones-to-track-work/managing-labels")
        });
    }

    private string GetUrl(
        IGetRepo_Repository_Issues_Edges_Node node,
        AnalysisContext context) =>
        Shared.CreateEmbeddedLink(Path.Combine(context.Repo.Url.ToString(), "issues", node.Number.ToString()), "issue " + node.Number);
}