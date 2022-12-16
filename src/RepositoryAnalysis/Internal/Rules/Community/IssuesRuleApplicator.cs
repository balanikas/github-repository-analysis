using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

public class IssuesRuleApplicator : IRuleApplicator
{
    public string RuleName => "issues";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var (diagnosis, note) = GetDiagnosis();

        var templates = "";
        if (context.Repo.IssueTemplates.Any())
        {
            var names = context.Repo.IssueTemplates.Select(x => x.Name);
            templates = "Templates found: <br/>" + string.Join("<br/>", names);
        }

        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = templates,
                Text = @"
Issues let you track your work on GitHub, where development happens.
You may wish to turn issues off for your repository if you do not accept contributions or bug reports.
",
                AboutUrl = "https://docs.github.com/en/issues/tracking-your-work-with-issues/about-issues",
                AboutHeader = "about issues",
                GuidanceUrl = diagnosis == Diagnosis.Warning ? Path.Combine(context.Repo.Url, "community") : null,
                GuidanceHeader = "Community Standards"
            }
        };

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.HasIssuesEnabled
                ? context.Repo.IssueTemplates.Any()
                    ? (Diagnosis.Info, $"issues are enabled and found {context.Repo.IssueTemplates.Count} issue templates")
                    : (Diagnosis.Warning, "issues are enabled but missing issue templates")
                : (Diagnosis.Info, "feature is disabled");
    }
}