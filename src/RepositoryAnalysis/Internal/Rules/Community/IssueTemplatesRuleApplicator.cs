using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class IssueTemplatesRuleApplicator : IRuleApplicator
{
    public string RuleName => "issue templates";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            if (context.Repo.HasIssuesEnabled)
                if (context.Repo.IssueTemplates != null && context.Repo.IssueTemplates.Any())
                {
                    var names = context.Repo.IssueTemplates.Select(x => x.Name);
                    var templates = $"Templates found: <br/>{string.Join("<br/>", names)}";
                    return new RuleDiagnostics(Diagnosis.Info, $"issues are enabled and found {context.Repo.IssueTemplates.Count} issue templates", templates);
                }
                else
                {
                    return new RuleDiagnostics(Diagnosis.Warning, "issues are enabled but missing issue templates");
                }

            return new RuleDiagnostics(Diagnosis.NotApplicable, "feature is disabled");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
Issues let you track your work on GitHub, where development happens.
You may wish to turn issues off for your repository if you do not accept contributions or bug reports.
",
            AboutLink = new Link("about issues", "https://docs.github.com/en/issues/tracking-your-work-with-issues/about-issues"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Warning ? context.GetCommunityLink() : null
        });
    }
}