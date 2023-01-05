using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class DescriptionRuleApplicator : IRuleApplicator
{
    public string RuleName => "description";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = context.Repo.Description is not null
            ? new RuleDiagnostics(Diagnosis.Info, "found")
            : new RuleDiagnostics(Diagnosis.Warning, "missing");

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
A repository description helps users to understand what the repository is about.
It can be edited in the About section.",
            AboutLink = new Link("this guide on how to create a repository", "https://docs.github.com/en/get-started/quickstart/create-a-repo"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Warning ? context.GetCommunityLink() : null
        });
    }
}