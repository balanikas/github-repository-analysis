using System.Text;
using RepositoryAnalysis.Internal.Rules;

namespace RepositoryAnalysis.Model;

public record Rule
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Name { get; init; }
    public string Note { get; init; } = "";
    public string? ResourceName { get; init; }
    public string? ResourceUrl { get; init; }
    public required Explanation Explanation { get; init; }
    public Diagnosis Diagnosis { get; init; } = Diagnosis.Info;
    public RuleCategory Category { get; init; }


    public virtual bool Equals(
        Rule? other) =>
        other is not null && Id == other.Id;

    protected virtual bool PrintMembers(
        StringBuilder builder)
    {
        builder.Append($"Id = {Id}");
        return true;
    }

    public override int GetHashCode() => Id.GetHashCode();
}