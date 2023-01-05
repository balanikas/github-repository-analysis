using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class PullRequestsRuleApplicator : IRuleApplicator
{
    public string RuleName => "pull requests";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis(context.Repo.PullRequests.Nodes);

        RuleDiagnostics GetDiagnosis(IReadOnlyList<IGetRepo_Repository_PullRequests_Nodes?>? nodes)
        {
            if (nodes is null || !nodes.Any()) return new RuleDiagnostics(Diagnosis.Info, "no open pull requests found");

            var creationDates = nodes.Select(x => x!.CreatedAt).OrderBy(x => x.DateTime);
            var oldestCreationDate = creationDates.First();
            var foundStale = oldestCreationDate <= new DateTimeOffset(DateTime.UtcNow.AddDays(-90));
            var avgCreationDate = DateTimeOffset.FromUnixTimeSeconds((long)nodes.Average(x => x!.CreatedAt.ToUnixTimeSeconds()));
            var foundHighAverage = avgCreationDate <= new DateTimeOffset(DateTime.UtcNow.AddDays(-30));
            if (!foundStale && !foundHighAverage)
                return new RuleDiagnostics(Diagnosis.Info,
                    $"found {nodes.Count} open pull requests");

            var details = "Found stale open pull requests.<br/>";
            if (foundStale)
                details += $"""
Oldest one was created {(DateTime.UtcNow - oldestCreationDate).Days} days ago.
<br/>
""";
            if (foundHighAverage)
                details += $"""
Pull requests have been open on average for {(DateTime.UtcNow - avgCreationDate).Days} days.
""";

            return new RuleDiagnostics(Diagnosis.Warning, "found stale pull requests", details, new Link("stale pull requests",
                Path.Combine(context.Repo.Url.ToString(), "pulls?q=is%3Apr+is%3Aopen+sort%3Acreated-asc")));
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
Pull requests let you tell others about changes you've pushed to a branch in a repository on GitHub. 
Once a pull request is opened, you can discuss and review the potential changes with collaborators and add follow-up commits before your changes are merged into the base branch.",
            AboutLink = new Link("about pull requests",
                "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests"),
            GuidanceLink = new Link("how to work effectively with pull requests",
                "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/about-pull-request-reviews")
        });
    }
}