namespace RepositoryAnalysis.Internal.TextGeneration;

public interface IGpt3Client
{
    Task<string> GetCompletion(string prompt);
    Task<IDictionary<string, string>> GetCompletions(params string[] prompts);
}