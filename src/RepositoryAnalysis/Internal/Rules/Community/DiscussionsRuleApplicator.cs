using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class DiscussionsRuleApplicator : IRuleApplicator
{
    public string RuleName => "discussions";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            if (!context.Repo.HasDiscussionsEnabled)
                return new RuleDiagnostics(Diagnosis.NotApplicable, "feature is disabled");
            if (context.Repo.Discussions.Edges == null)
                return new RuleDiagnostics(
                    Diagnosis.Info,
                    $"found {context.Repo.Discussions.TotalCount} discussions",
                    "",
                    new Link("discussions", Path.Combine(context.Repo.Url.ToString(), "discussions")));

            var discussions = context.Repo.Discussions.Edges
                .Where(x => x is not null)
                .Select(x => x!.Node).ToArray();
            var discussionsWithoutAnswer = context.Repo.Discussions.Edges
                .Where(x => x?.Node?.Answer is null || x.Node?.Answer.IsAnswer == false)
                .Select(x => x.Node).ToArray();

            if (discussionsWithoutAnswer.Length == 0)
            {
                var details = context.Repo.Discussions.TotalCount is < 100 and > 1
                    ? "All discussions are answered"
                    : "Out of the 100 most recently updated discussions, all are answered";

                return new RuleDiagnostics(
                    Diagnosis.Info,
                    $"found {context.Repo.Discussions.TotalCount} discussions",
                    details,
                    new Link("discussions", Path.Combine(context.Repo.Url.ToString(), "discussions")));
            }
            else
            {
                var percent = (int)Math.Round((double)(100 * discussionsWithoutAnswer.Length) / discussions.Length);

                var details = context.Repo.Discussions.TotalCount < 100
                    ? $"found {percent}% of {discussions.Length} discussions unanswered"
                    : $"Out of the 100 most recently updated discussions, found {percent}% of {discussions.Length} discussions unanswered";

                details += $"""
<br/>
Sample of unanswered discussions: 
<br/>
{string.Join("<br/>", discussionsWithoutAnswer.Take(5).Select(x => GetUrl(x, context)))}
""";
                var diagnosis = percent > 50 ? Diagnosis.Warning : Diagnosis.Info;

                return new RuleDiagnostics(
                    diagnosis,
                    $"found {context.Repo.Discussions.TotalCount} discussions and some are unanswered",
                    details,
                    new Link("unanswered discussions", Path.Combine(context.Repo.Url.ToString(), "discussions?discussions_q=is%3Aunanswered")));
            }
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
Use discussions to ask and answer questions, share information, make announcements, and conduct or participate in a conversation about a project on GitHub.
With GitHub Discussions, the community for your project can create and participate in conversations within the project's repository or organization. 
Discussions empower a project's maintainers, contributors, and visitors to gather and accomplish the following goals in a central location, without third-party tools.",
            AboutLink = new Link("about discussions",
                "https://docs.github.com/en/discussions/collaborating-with-your-community-using-discussions/about-discussions")
        });
    }

    private string GetUrl(
        IGetRepo_Repository_Discussions_Edges_Node node,
        AnalysisContext context) =>
        Shared.CreateEmbeddedLink(Path.Combine(context.Repo.Url.ToString(), "discussions", node.Number.ToString()), $"discussion {node.Number}");
}