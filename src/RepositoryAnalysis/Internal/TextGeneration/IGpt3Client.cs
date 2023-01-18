namespace RepositoryAnalysis.Internal.TextGeneration;

public interface IGpt3Client
{
    Task<string> GetCompletion(string prompt);
}