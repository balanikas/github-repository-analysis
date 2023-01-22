using System.Text;
using RepositoryAnalysis.Internal.Rules;

namespace RepositoryAnalysis.Internal.TextGeneration;

public class PromptBuilder
{
    private readonly RuleGuidanceAttribute _meta;
    private readonly StringBuilder _prompt;

    public PromptBuilder(
        string prompt,
        RuleGuidanceAttribute meta)
    {
        _meta = meta;
        _prompt = new StringBuilder(prompt);
    }

    public PromptBuilder WithContext(string ctx = "Pretend you are a software development coach.")
    {
        _prompt.Insert(0, " ");
        _prompt.Insert(0, ctx);
        return this;
    }

    public PromptBuilder WithComplexity()
    {
        _prompt.Append(" ");
        var complexity = _meta.Complexity switch
        {
            Complexity.Simple => "Explain it like I am 10.",
            Complexity.FairlyComplex => "Explain it in simple terms.",
            Complexity.VeryComplex => "Explain it using complicated words.",
            _ => "Explain it like I am mentally challenged."
        };
        _prompt.Append(complexity);
        return this;
    }

    public PromptBuilder WithTone()
    {
        _prompt.Append(" ");
        var tone = _meta.Tone switch
        {
            Tone.Motivational => "Make it sound motivational.",
            Tone.Helpful => "Make it sound helpful.",
            Tone.Instructional => "Make it sound instructional.",
            _ => "Make it sound plain and boring."
        };
        _prompt.Append(tone);
        return this;
    }

    public PromptBuilder WithLength()
    {
        _prompt.Append(" ");
        _prompt.Append($"Make your response between {_meta.Range.Start} and {_meta.Range.End} characters.");
        return this;
    }

    public override string ToString() => _prompt.ToString();
}