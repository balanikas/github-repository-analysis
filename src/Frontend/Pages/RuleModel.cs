using Microsoft.AspNetCore.Components;
using RepositoryAnalysis.Model;

namespace Frontend.Pages;

public record RuleModel(
    Rule Rule)
{
    public bool ShowDetails { get; set; }

    public MarkupString? ResourceUrl => new MarkupString(Rule.ResourceUrl ?? "");
    public string Name => Rule.Name;

    public virtual bool Equals(
        RuleModel? other) =>
        other is not null && Rule.Id == other.Rule.Id;

    public override int GetHashCode() => Rule.Id.GetHashCode();
}