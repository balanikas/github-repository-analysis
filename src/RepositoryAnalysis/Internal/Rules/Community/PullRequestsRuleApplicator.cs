using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class PullRequestsRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string HowTo = "How to write a great pull request?";
    [RuleGuidance] private const string Staleness = "When is a pull request considered stale and why is it important close it quickly?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is the purpose of a pull request?";

    private readonly IGpt3Client _gpt3Client;

    public PullRequestsRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "pull requests";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
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
Oldest one was created {Shared.HowLong(DateTime.UtcNow - oldestCreationDate)} ago.
<br/>
""";
            if (foundHighAverage)
                details += $"""
Pull requests have been open on average for {Shared.HowLong(DateTime.UtcNow - avgCreationDate)}.
""";

            return new RuleDiagnostics(Diagnosis.Warning, "found stale pull requests", details, new Link("stale pull requests",
                Path.Combine(context.Repo.Url.ToString(), "pulls?q=is%3Apr+is%3Aopen+sort%3Acreated-asc")));
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Staleness, HowTo),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about pull requests",
                "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests"),
            GuidanceLink = new Link("how to work effectively with pull requests",
                "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/about-pull-request-reviews")
        });
    }
}