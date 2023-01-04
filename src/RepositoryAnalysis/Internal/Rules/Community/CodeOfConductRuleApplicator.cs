using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class CodeOfConductRuleApplicator : IRuleApplicator
{
    public string RuleName => "code of conduct";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var diagnostics = context.Repo.CodeOfConduct is not null
            ? new(Diagnosis.Info, "found", null,
                new(context.Repo.CodeOfConduct.Name, context.Repo.CodeOfConduct.Url!.ToString()))
            : new RuleDiagnostics(Diagnosis.Warning, "missing code of conduct file");

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
Adopt a code of conduct to define community standards, signal a welcoming and inclusive project, and outline procedures for handling abuse.
A code of conduct defines standards for how to engage in a community. It signals an inclusive environment that respects all contributions. 
It also outlines procedures for addressing problems between members of your project's community. ",
            AboutLink = new("about code of conduct",
                "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-a-code-of-conduct-to-your-project"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Warning ? context.GetCommunityLink() : null
        });
    }
}