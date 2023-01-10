using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class IssuesRuleApplicator : IRuleApplicator
{
    public string RuleName => "issues";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            if (!context.Repo.HasIssuesEnabled) return new RuleDiagnostics(Diagnosis.NotApplicable, "feature is disabled");
            if (context.Repo.Issues.Edges is null || !context.Repo.Issues.Edges.Any())
                return new RuleDiagnostics(Diagnosis.NotApplicable, "no open issues");

            var nodes = context.Repo.Issues.Edges
                .Select(x => x!.Node)
                .ToList();

            var creationDates = nodes.Select(x => x!.CreatedAt).OrderBy(x => x.DateTime);
            var oldestCreationDate = creationDates.First();
            var foundStale = oldestCreationDate <= new DateTimeOffset(DateTime.UtcNow.AddDays(-365));
            var avgCreationDate = DateTimeOffset.FromUnixTimeSeconds((long)nodes.Average(x => x!.CreatedAt.ToUnixTimeSeconds()));
            var foundHighAverage = avgCreationDate <= new DateTimeOffset(DateTime.UtcNow.AddDays(-90));
            if (!foundStale && !foundHighAverage)
                return new RuleDiagnostics(Diagnosis.Info,
                    $"found {nodes.Count} open issues");

            var details = "Found old open issues.<br/>";
            if (foundStale)
                details += $"""
Oldest one was created {Shared.HowLong(DateTime.UtcNow - oldestCreationDate)} ago.
<br/>
""";
            if (foundHighAverage)
                details += $"""
Issues have been open on average for {Shared.HowLong(DateTime.UtcNow - avgCreationDate)}.
""";

            return new RuleDiagnostics(Diagnosis.Warning, "found old issues", details, new Link("oldest issues",
                Path.Combine(context.Repo.Url.ToString(), "issues?q=is%3Aissue+is%3Aopen+sort%3Acreated-asc")));
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
Issues let you categorize your work on GitHub, where development happens.
You may wish to turn issues off for your repository if you do not accept contributions or bug reports.
",
            AboutLink = new Link("about issues", "https://docs.github.com/en/issues")
        });
    }
}