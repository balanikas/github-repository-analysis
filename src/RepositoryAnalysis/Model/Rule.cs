using System.Text;
using RepositoryAnalysis.Internal.Rules;

namespace RepositoryAnalysis.Model;

public record Rule(
    RuleDiagnostics Diagnostics)
{
    private Guid Id { get; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required Explanation Explanation { get; init; }
    public required RuleCategory Category { get; init; }

    public virtual bool Equals(
        Rule? other) =>
        other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    internal static Rule Create(
        IRuleApplicator ruleApplicator,
        RuleDiagnostics diagnostics,
        Explanation explanation) =>
        new(diagnostics)
        {
            Category = ruleApplicator.Category,
            Name = ruleApplicator.RuleName,
            Explanation = explanation
        };

    protected virtual bool PrintMembers(
        StringBuilder builder)
    {
        builder.Append($"{nameof(Name)} = {Name}");
        builder.Append($"{nameof(Category)} = {Category}");
        return true;
    }
}