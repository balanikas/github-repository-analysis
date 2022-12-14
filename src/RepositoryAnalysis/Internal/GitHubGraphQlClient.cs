using System.Net;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RepositoryAnalysis.Internal;

//todo configure request throttling
public class GitHubGraphQlClient
{
    private const string BaseUrl = "https://api.github.com/graphql";
    private readonly GraphQLHttpClient _graphQlClient;
    private readonly ILogger<GitHubGraphQlClient> _logger;

    public GitHubGraphQlClient(
        IOptions<GitHubOptions> githubOptions,
        HttpClient httpClient,
        ILogger<GitHubGraphQlClient> logger)
    {
        _logger = logger;
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + githubOptions.Value.Token);
        _graphQlClient = new GraphQLHttpClient(new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(BaseUrl)
            },
            new SystemTextJsonSerializer(),
            httpClient);
    }

    public async Task<ListRepos.Topic> ListReposByTopic(
        string topic)
    {
        var query = $@"
{{
  topic(name: ""{topic}"") {{
    relatedTopics(first: 10) {{
      name
    }}
    repositories(first: 5) {{
      edges {{
        node {{
          url
        }}
      }}
    }}
  }}
}}

";

        await Task.Delay(1000);
        var response = await Post<ListRepos.Data>(query);
        return response.topic;
    }


    public async Task<Repo?> GetUpdatedAt(
        string owner,
        string name)
    {
        var query = $@"
{{
  repository(name: ""{name}"", owner: ""{owner}"") {{
    updatedAt
    pushedAt
  }}
}}";

        var response = await Post<Data>(query);
        return response.Repository;
    }


    public async Task<Repo> GetRepository(
        string owner,
        string name)
    {
        var query = $@"
{{
  repository(name: ""{name}"", owner: ""{owner}"") {{
     releases(first: 1) {{
      edges {{
        node {{
          name
          url
        }}
      }}
    }}
    diskUsage
    updatedAt
    pushedAt
    repositoryTopics {{
        totalCount
    }}
    codeOfConduct {{
      url
      name
    }}
    codeowners {{
      errors {{
        kind
      }}
    }}
    defaultBranchRef {{
      name
       target {{
        commitResourcePath
      }}
      branchProtectionRule {{
        allowsForcePushes
        dismissesStaleReviews
        lockBranch
        requiresApprovingReviews
        requiresConversationResolution
        requiresStatusChecks
        requiresStrictStatusChecks
      }}
    }}
    description
    hasDiscussionsEnabled
    hasIssuesEnabled
    homepageUrl
    isArchived
    isEmpty
    isLocked
    isSecurityPolicyEnabled
    issueTemplates {{
      name
      filename
    }}
    licenseInfo {{
      name
      url
    }}
    openGraphImageUrl
    primaryLanguage {{
      name
      color
    }}
    pullRequestTemplates {{
      filename
    }}
    securityPolicyUrl
    url
    vulnerabilityAlerts {{
      totalCount
    }}
    pullRequests {{
      totalCount
    }}
    issues {{
      totalCount
    }}
  }}
}}";

        var response = await Post<Data>(query);
        return response.Repository;
    }

    private async Task<T> Post<T>(
        string query)
    {
        var request = new GraphQLRequest
        {
            Query = query
        };

        GraphQLResponse<T> response;
        try
        {
            response = await _graphQlClient.SendQueryAsync<T>(request);
        }
        catch (GraphQLHttpRequestException e)
        {
            _logger.LogError(e, "Error posting query {Query}", query);
            if (e.StatusCode == HttpStatusCode.BadGateway) throw new Exception($"{e.StatusCode} for query \n {query}");

            throw;
        }

        var httpResponse = response.AsGraphQLHttpResponse();
        if (httpResponse.StatusCode != HttpStatusCode.OK)
            _logger.LogError("request failed for query {Query} with status code {StatusCode}", query, httpResponse.StatusCode);

        return response.Data;
    }

    public class ListRepos
    {
        public class Data
        {
            public Topic topic { get; init; }
        }

        public class Edge
        {
            public Node node { get; init; }
        }

        public class Node
        {
            public string url { get; init; }
        }

        public class Repositories
        {
            public List<Edge> edges { get; init; }
        }

        public class RelatedTopic
        {
            public string name { get; init; }
        }

        public class Topic
        {
            public string name { get; init; }
            public Repositories repositories { get; init; }
            public List<RelatedTopic> relatedTopics { get; init; }
        }
    }

    //todo fix nullable across solution
    public class CodeOfConduct
    {
        public string Url { get; init; }
        public string Name { get; init; }
    }

    public class Codeowners
    {
        public List<Error> Errors { get; init; }
    }

    public class Data
    {
        public Repo Repository { get; init; }
    }

    public class DefaultBranchRef
    {
        public string Name { get; init; }
        public Target Target { get; init; }
    }

    public class Target
    {
        public string CommitResourcePath { get; init; }
    }

    public class Error
    {
        public string Kind { get; init; }
    }

    public class Issues
    {
        public int TotalCount { get; init; }
    }

    public class IssueTemplate
    {
        public string Name { get; init; }
        public string Filename { get; init; }
    }

    public class Labels
    {
        public int TotalCount { get; init; }
    }

    public class LicenseInfo
    {
        public string Name { get; init; }
        public string Url { get; init; }
    }

    public class PrimaryLanguage
    {
        public string Name { get; init; }
        public string Color { get; init; }
    }

    public class PullRequests
    {
        public int TotalCount { get; init; }
    }

    public class PullRequestTemplate
    {
        public string Filename { get; init; }
    }

    public class Repo
    {
        public Releases Releases { get; init; }
        public DateTime UpdatedAt { get; init; }
        public DateTime PushedAt { get; init; }
        public RepositoryTopics RepositoryTopics { get; init; }

        public CodeOfConduct? CodeOfConduct { get; init; }
        public Codeowners Codeowners { get; init; }
        public DefaultBranchRef DefaultBranchRef { get; init; }
        public string? Description { get; init; }
        public bool DeleteBranchOnMerge { get; init; }
        public bool HasDiscussionsEnabled { get; init; }
        public bool HasIssuesEnabled { get; init; }
        public string? HomepageUrl { get; init; }
        public bool IsArchived { get; init; }
        public bool IsEmpty { get; init; }
        public bool IsLocked { get; init; }
        public bool IsSecurityPolicyEnabled { get; init; }
        public List<IssueTemplate> IssueTemplates { get; init; }
        public Labels Labels { get; init; }
        public LicenseInfo? LicenseInfo { get; init; }
        public string OpenGraphImageUrl { get; init; }
        public PrimaryLanguage? PrimaryLanguage { get; init; }
        public List<PullRequestTemplate> PullRequestTemplates { get; init; }
        public string SecurityPolicyUrl { get; init; }
        public string Url { get; set; }
        public VulnerabilityAlerts VulnerabilityAlerts { get; init; }
        public PullRequests PullRequests { get; init; }
        public Issues Issues { get; init; }
        public int DiskUsage { get; init; }
        public bool HasProjectsEnabled { get; init; }
    }

    public class RepositoryTopics
    {
        public int TotalCount { get; init; }
    }

    public class VulnerabilityAlerts
    {
        public int TotalCount { get; init; }
    }
    
    public class Releases
    {
        public List<Edge> Edges { get; init; }
    }

    public class Edge
    {
        public Node Node { get; init; }
    }

    public class Node
    {
        public string Name { get; init; }
        public string Url { get; init; }
    }
}

