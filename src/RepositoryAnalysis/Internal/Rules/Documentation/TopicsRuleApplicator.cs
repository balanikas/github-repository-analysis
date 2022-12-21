using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class TopicsRuleApplicator : IRuleApplicator
{
    public string RuleName => "topics";
    public RuleCategory Category => RuleCategory.Documentation;
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
To help other people find and contribute to your project, you can add topics to your repository related to your project's intended purpose, subject area, affinity groups, or other important qualities.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics",
                AboutHeader = "about topics",
                GuidanceUrl = "https://docs.github",
                GuidanceHeader = "how to work with topics"
            }
        };

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.RepositoryTopics.TotalCount > 0
                ? (Diagnosis.Info, $"found {context.Repo.RepositoryTopics.TotalCount} topics")
                : (Diagnosis.Warning, "no topics found");
    }
}