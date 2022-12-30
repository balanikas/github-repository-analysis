using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class DescriptionRuleApplicator : IRuleApplicator
{
    public string RuleName => "description";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var diagnostics = context.Repo.Description is not null
            ? new RuleDiagnostics(Diagnosis.Info, "found")
            : new RuleDiagnostics(Diagnosis.Warning, "missing");

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
A repository description helps users to understand what the repository is about.
It can be edited in the About section.",
            AboutUrl = "https://docs.github.com/en/get-started/quickstart/create-a-repo",
            AboutHeader = "this guide on how to create a repository",
            GuidanceUrl = diagnostics.Diagnosis == Diagnosis.Warning ? Path.Combine(context.Repo.Url.ToString(), "community") : null,
            GuidanceHeader = "Community Standards"
        });
    }
}