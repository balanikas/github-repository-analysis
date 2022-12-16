using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

public class CodeOfConductRuleApplicator : IRuleApplicator
{
    public string RuleName => "code of conduct";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var entry = context.Repo.CodeOfConduct;

        var (diagnosis, note) = GetDiagnosis(entry);
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
Adopt a code of conduct to define community standards, signal a welcoming and inclusive project, and outline procedures for handling abuse.
A code of conduct defines standards for how to engage in a community. It signals an inclusive environment that respects all contributions. 
It also outlines procedures for addressing problems between members of your project's community. ",
                AboutUrl =
                    "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-a-code-of-conduct-to-your-project",
                AboutHeader = "about code of conduct",
                GuidanceUrl = diagnosis == Diagnosis.Warning ? Path.Combine(context.Repo.Url, "community") : null,
                GuidanceHeader = "Community Standards"
            },
            ResourceName = entry?.Name, ResourceUrl = entry?.Url
        };

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.CodeOfConduct? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing code of conduct file");
    }
}