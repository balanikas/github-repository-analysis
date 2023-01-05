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
        var diagnostics = context.Repo.HasDiscussionsEnabled
            ? new RuleDiagnostics(Diagnosis.Info, "feature is enabled")
            : new RuleDiagnostics(Diagnosis.Info, "feature is disabled");

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
}