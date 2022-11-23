using System.Net;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RepositoryAnalysis;

//todo configure request throttling
public class GitHubGraphQlClient
{
    private readonly ILogger<GitHubGraphQlClient> _logger;
    private const string BaseUrl = "https://api.github.com/graphql";
    private readonly GraphQLHttpClient _graphQlClient;

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

    public async Task<IReadOnlyList<string>> ListReposByTopic(
        string topic)
    {
        var request = new GraphQLRequest
        {
            Query = $@"
{{
  topic(name: ""{topic}"") {{
    name
    repositories(first: 100) {{
      edges {{
        node {{
          url
        }}
      }}
    }}
  }}
}}

        "
        };

        var response = await _graphQlClient.SendQueryAsync<ListRepos.Data>(request);
        if (response.AsGraphQLHttpResponse().StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("listing repos failed for {Topic}", topic);
        }

        return response.Data.topic.repositories.edges.Select(x => x.node.url).ToArray();
    }

    public async Task<Repo> GetRepoData(
        string owner,
        string name)
    {
        var request = new GraphQLRequest
        {
            Query = $@"
{{
  repository(name: ""{name}"", owner: ""{owner}"") {{

    diskUsage
    updatedAt
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
}}
        "
        };

        var response = await _graphQlClient.SendQueryAsync<Data>(request);
        if (response.AsGraphQLHttpResponse().StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("get repo data failed for {Owner}{Name}", owner, name);
        }

        return response.Data.Repository;
    }

    public async Task<Repo> GetRepoTree(
        string owner,
        string name,
        string branch,
        string treePath,
        int diskusage)
    {
        var query = GetTreeQuery(owner, name, branch, treePath, diskusage);
        var request = new GraphQLRequest
        {
            Query = query
        };

        var response = await _graphQlClient.SendQueryAsync<Data>(request);
        if (response.AsGraphQLHttpResponse().StatusCode != HttpStatusCode.OK) 
        {
            _logger.LogError("get repo tree failed for {Owner}{Name}", owner, name);
        }
        return response.Data.Repository;
    }


    private string GetTreeQuery(
        string owner,
        string name,
        string branch,
        string treePath,
        int diskUsage)
    {
        var deepTreeQueryPart = diskUsage <= 100000
            ? @"
                    object {
                    ... on Tree {
                        entries {
                          path
                          size
                          type
                          object {
                            ... on Tree {
                                entries {
                                  path
                                  size
                                  type
                                  object {
                                        ... on Tree {
                                            entries {
                                              path
                                              size
                                              type
                                              }
                                            }
                                          }
                                  }
                                }
                              }
                          }
                        }
                      }
"
            : "";

        var shallowTreeQuery = $@"
{{
  repository(name: ""{name}"", owner: ""{owner}"") {{
     gitIgnore: object(expression: ""{branch}:.gitignore"") {{
      ... on Blob {{
        text
      }}
    }}
    object(expression: ""{branch}:{treePath}"") {{
      ... on Tree {{
        entries {{
          path
          size
          type
          object {{
            ... on Tree {{
                entries {{
                  path
                  size
                  type
                    object {{
                    ... on Tree {{
                        entries {{
                          path
                          size
                          type
                          {deepTreeQueryPart}
                      }}
                    }}
                  }}
              }}
            }}
          }}
        }}
      }}
    }}
  }}
}}
        ";

        return shallowTreeQuery;
    }

    public class ListRepos
    {
        public class Data
        {
            public Topic topic { get; set; }
        }

        public class Edge
        {
            public Node node { get; set; }
        }

        public class Node
        {
            public string url { get; set; }
        }

        public class Repositories
        {
            public List<Edge> edges { get; set; }
        }

        public class Root
        {
            public Data data { get; set; }
        }

        public class Topic
        {
            public string name { get; set; }
            public Repositories repositories { get; set; }
        }
    }

    //todo fix nullable across solution
    public class CodeOfConduct
    {
        public string Url { get; set; }
        public string Name { get; set; }
    }

    public class Codeowners
    {
        public List<Error> Errors { get; set; }
    }

    public class Data
    {
        public Repo Repository { get; set; }
    }

    public class DefaultBranchRef
    {
        public string Name { get; set; }
        public object BranchProtectionRule { get; set; }
    }

    public class Entry
    {
        public string Path { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }

        public Objects Object { get; set; }
    }

    public class Error
    {
        public string Kind { get; set; }
    }

    public class Issues
    {
        public int TotalCount { get; set; }
    }

    public class IssueTemplate
    {
        public string Name { get; set; }
        public string Filename { get; set; }
    }

    public class Labels
    {
        public int TotalCount { get; set; }
    }

    public class LicenseInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class Objects
    {
        public List<Entry> Entries { get; set; }
    }

    public class PrimaryLanguage
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class PullRequests
    {
        public int TotalCount { get; set; }
    }

    public class PullRequestTemplate
    {
        public string Filename { get; set; }
    }

    public class Repo
    {
        public DateTime UpdatedAt { get; set; }
        public RepositoryTopics RepositoryTopics { get; set; }

        public CodeOfConduct? CodeOfConduct { get; set; }
        public Codeowners Codeowners { get; set; }
        public DefaultBranchRef DefaultBranchRef { get; set; }
        public string? Description { get; set; }
        public bool DeleteBranchOnMerge { get; set; }
        public bool HasDiscussionsEnabled { get; set; }
        public bool HasIssuesEnabled { get; set; }
        public string? HomepageUrl { get; set; }
        public bool IsArchived { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsLocked { get; set; }
        public bool IsSecurityPolicyEnabled { get; set; }
        public List<IssueTemplate> IssueTemplates { get; set; }
        public Labels Labels { get; set; }
        public LicenseInfo? LicenseInfo { get; set; }
        public string OpenGraphImageUrl { get; set; }
        public PrimaryLanguage? PrimaryLanguage { get; set; }
        public List<PullRequestTemplate> PullRequestTemplates { get; set; }
        public string SecurityPolicyUrl { get; set; }
        public string Url { get; set; }
        public VulnerabilityAlerts VulnerabilityAlerts { get; set; }
        public PullRequests PullRequests { get; set; }
        public Issues Issues { get; set; }
        public Objects Object { get; set; }
        public int DiskUsage { get; set; }
        public bool HasProjectsEnabled { get; set; }
        public GitIgnore? GitIgnore { get; set; }
    }

    public class GitIgnore
    {
        public string Text { get; set; }
    }

    public class RepositoryTopics
    {
        public int TotalCount { get; set; }
    }


    public class Root
    {
        public Data Data { get; set; }
    }

    public class VulnerabilityAlerts
    {
        public int TotalCount { get; set; }
    }
}