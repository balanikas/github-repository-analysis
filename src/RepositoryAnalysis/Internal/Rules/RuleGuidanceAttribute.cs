namespace RepositoryAnalysis.Internal.Rules;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class RuleGuidanceAttribute : Attribute
{
    public RuleGuidanceAttribute(string prompt) => Prompt = prompt;

    public string Prompt { get; }
}