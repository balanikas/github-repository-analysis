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
        var diagnostics = context.Repo.IsSecurityPolicyEnabled is not null && context.Repo.IsSecurityPolicyEnabled.Value
            ? new RuleDiagnostics(Diagnosis.Info, "found security policy", null, "security policy", context.Repo.SecurityPolicyUrl?.ToString())
            : new RuleDiagnostics(Diagnosis.Warning, "no security policy found");

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = @"
You can give instructions for how to report a security vulnerability in your project by adding a security policy to your repository.
",
            AboutUrl =
                "https://docs.github.com/en/code-security/getting-started/adding-a-security-policy-to-your-repository#about-security-policies",
            AboutHeader = "about security policies",
            GuidanceUrl = "https://docs.github.com/en/code-security/getting-started/adding-a-security-policy-to-your-repository",
            GuidanceHeader = "how to add a security policy"
        });
    }
}