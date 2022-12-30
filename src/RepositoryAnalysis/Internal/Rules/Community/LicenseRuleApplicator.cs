using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class LicenseRuleApplicator : IRuleApplicator
{
    public string RuleName => "license";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var diagnostics = context.Repo.LicenseInfo is not null
            ? new(Diagnosis.Info, "found", null, new(context.Repo.LicenseInfo.Name, context.Repo.LicenseInfo.Url?.ToString() ?? string.Empty))
            : new RuleDiagnostics(Diagnosis.Error, "missing");

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
Public repositories on GitHub are often used to share open source software. 
For your repository to truly be open source, you'll need to license it so that others are free to use, change, and distribute the software.",
            AboutLink = new("about open source licensing",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Error ? context.GetCommunityLink() : null
        });
    }
}