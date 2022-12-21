using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class DiscussionsRuleApplicator : IRuleApplicator
{
    public string RuleName => "discussions";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();
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
Use discussions to ask and answer questions, share information, make announcements, and conduct or participate in a conversation about a project on GitHub.
With GitHub Discussions, the community for your project can create and participate in conversations within the project's repository or organization. 
Discussions empower a project's maintainers, contributors, and visitors to gather and accomplish the following goals in a central location, without third-party tools.",
                AboutUrl = "https://docs.github.com/en/discussions/collaborating-with-your-community-using-discussions/about-discussions",
                AboutHeader = "about discussions"
            }
        };

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.HasDiscussionsEnabled
                ? (Diagnosis.Info, "feature is enabled")
                : (Diagnosis.Info, "feature is disabled");
    }
}