using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

public class LicenseRuleApplicator : IRuleApplicator
{
    public string RuleName => "license";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var license = context.Repo.LicenseInfo;
        var (diagnosis, note) = GetDiagnosis(license);

        (Diagnosis, string) GetDiagnosis(
            GitHubGraphQlClient.LicenseInfo? e) =>
            e is not null
                ? (Diagnosis.Info, "found")
                : (Diagnosis.Error, "missing");

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
Public repositories on GitHub are often used to share open source software. 
For your repository to truly be open source, you'll need to license it so that others are free to use, change, and distribute the software.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository",
                AboutHeader = "about open source licensing",
                GuidanceUrl =
                    "https://choosealicense.com/",
                GuidanceHeader = "how to choose a license"
            },
            ResourceName = license?.Name, ResourceUrl = license?.Url
        };
    }
}