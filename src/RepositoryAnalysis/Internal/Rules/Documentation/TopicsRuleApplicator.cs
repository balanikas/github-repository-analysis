using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class TopicsRuleApplicator : IRuleApplicator
{
    public string RuleName => "topics";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = context.Repo.RepositoryTopics.TotalCount > 0
            ? new RuleDiagnostics(Diagnosis.Info, $"found {context.Repo.RepositoryTopics.TotalCount} topics")
            : new RuleDiagnostics(Diagnosis.Warning, "no topics found");

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
To help other people find and contribute to your project, you can add topics to your repository related to your project's intended purpose, subject area, affinity groups, or other important qualities.",
            AboutLink = new Link("about topics",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics"),
            GuidanceLink = new Link("how to work with topics",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics")
        });
    }
}