using RepositoryAnalysis.Model;

namespace RepositoryAnalysis;

public interface IAnalysisService
{
    Task<RepoAnalysis> GetAnalysis(string url);
}