using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Security;

internal class SecurityPolicyRuleApplicator : IRuleApplicator
{
    public string RuleName => "security policy";
    public RuleCategory Category => RuleCategory.Security;
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
            Diagnosis = diagnosis,
            Note = note,
            Explanation = new Explanation
            {
                Details = null,
                Text = @"
You can give instructions for how to report a security vulnerability in your project by adding a security policy to your repository.
",
                AboutUrl =
                    "https://docs.github.com/en/code-security/getting-started/adding-a-security-policy-to-your-repository#about-security-policies",
                AboutHeader = "about security policies",
                GuidanceUrl = "https://docs.github.com/en/code-security/getting-started/adding-a-security-policy-to-your-repository",
                GuidanceHeader = "how to add a security policy"
            },
            ResourceName = diagnosis == Diagnosis.Info ? "security policy" : null,
            ResourceUrl = diagnosis == Diagnosis.Info
                ? context.Repo.SecurityPolicyUrl?.ToString()
                : null
        };

        (Diagnosis, string) GetDiagnosis() =>
            context.Repo.IsSecurityPolicyEnabled is not null && context.Repo.IsSecurityPolicyEnabled.Value
                ? (Diagnosis.Info, "found security policy")
                : (Diagnosis.Warning, "no security policy found");
    }
}