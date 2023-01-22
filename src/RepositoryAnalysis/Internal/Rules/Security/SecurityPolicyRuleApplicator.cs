using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Security;

internal class SecurityPolicyRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string LearnMore = "How can I learn more about security-related topics and contribute to security tools and projects?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is a github repository security policy and why do I need it?";

    [RuleGuidance] private const string WhereToPlace = "Where should a security policy be placed in a github repository ?";

    [RuleGuidance] private const string WithoutPolicy =
        "What happens if a security vulnerability in my project is detected and I do not have a security policy?";

    private readonly IGpt3Client _gpt3Client;

    public SecurityPolicyRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "security policy";
    public RuleCategory Category => RuleCategory.Security;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = context.Repo.IsSecurityPolicyEnabled is not null && context.Repo.IsSecurityPolicyEnabled.Value
            ? new RuleDiagnostics(Diagnosis.Info, "found security policy", null,
                new Link("security policy", context.Repo.SecurityPolicyUrl?.ToString() ?? string.Empty))
            : new RuleDiagnostics(Diagnosis.Warning, "missing security policy file");

        var guidance = diagnostics.Diagnosis == Diagnosis.Warning
            ? new Link("how to add a security policy",
                Path.Combine(context.Repo.Url.ToString(), "security/policy"))
            : null;

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(WithoutPolicy, LearnMore, WhereToPlace),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about security policies",
                "https://docs.github.com/en/code-security/getting-started/adding-a-security-policy-to-your-repository#about-security-policies"),
            GuidanceLink = guidance
        });
    }
}