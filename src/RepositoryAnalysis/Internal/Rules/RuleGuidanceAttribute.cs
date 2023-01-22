namespace RepositoryAnalysis.Internal.Rules;

[AttributeUsage(AttributeTargets.Field)]
public class RuleGuidanceAttribute : Attribute
{
    public RuleGuidanceAttribute(
        int length = 500,
        Tone tone = Tone.Instructional,
        Complexity complexity = Complexity.FairlyComplex)
    {
        Range = (length - 50)..(length + 50);
        Tone = tone;
        Complexity = complexity;
    }

    public Range Range { get; }
    public Tone Tone { get; }
    public Complexity Complexity { get; }
}

public enum Tone
{
    Motivational,
    Helpful,
    Instructional
}

public enum Complexity
{
    Simple,
    FairlyComplex,
    VeryComplex
}