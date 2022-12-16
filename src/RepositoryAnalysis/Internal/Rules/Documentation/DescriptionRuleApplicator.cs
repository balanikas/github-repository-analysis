using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

public class DescriptionRuleApplicator : IRuleApplicator
{
    public string RuleName => "description";
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
A repository description helps users to understand what the repository is about.
It can be edited in the About section.",
                AboutUrl = "https://docs.github.com/en/get-started/quickstart/create-a-repo",
                AboutHeader = "this guide on how to create a repository",
                GuidanceUrl = diagnosis == Diagnosis.Warning ? Path.Combine(context.Repo.Url, "community") : null,
                GuidanceHeader = "Community Standards"
            }
        };

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.Description is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Warning, "missing");
    }
}