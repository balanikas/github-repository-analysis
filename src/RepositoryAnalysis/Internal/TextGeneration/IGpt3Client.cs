using RepositoryAnalysis.Internal.Rules;

namespace RepositoryAnalysis.Internal.TextGeneration;

public interface IGpt3Client
{
    Task<string> GetCompletion(string prompts);
    Task<IDictionary<string, string>> GetCompletions(params string[] prompts);

    void AddPromptMetaData(
        string prompt,
        RuleGuidanceAttribute promptMetaData);
}