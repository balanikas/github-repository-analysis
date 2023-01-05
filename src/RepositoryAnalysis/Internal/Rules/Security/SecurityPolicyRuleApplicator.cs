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
            ? new(Diagnosis.Info, "found security policy", null, new("security policy", context.Repo.SecurityPolicyUrl?.ToString() ?? string.Empty))
            : new RuleDiagnostics(Diagnosis.Warning, "missing security policy file");

        var guidance = diagnostics.Diagnosis == Diagnosis.Warning
            ? new Link("how to add a security policy",
                Path.Combine(context.Repo.Url.ToString(), "security/policy"))
            : null;

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
You can give instructions for how to report a security vulnerability in your project by adding a security policy to your repository.
",
            AboutLink = new("about security policies",
                "https://docs.github.com/en/code-security/getting-started/adding-a-security-policy-to-your-repository#about-security-policies"),
            GuidanceLink = guidance
        });
    }
}