using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

namespace RepositoryAnalysis.Internal.TextGeneration;

internal sealed class ClientSideRateLimitedHandler
    : DelegatingHandler, IAsyncDisposable
{
    private readonly RateLimiter _rateLimiter;

    public ClientSideRateLimitedHandler(RateLimiter limiter)
        : base(new HttpClientHandler()) => _rateLimiter = limiter;

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _rateLimiter.DisposeAsync().ConfigureAwait(false);

        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var lease = await _rateLimiter.AcquireAsync(
            1, cancellationToken);

        if (lease.IsAcquired) return await base.SendAsync(request, cancellationToken);

        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (lease.TryGetMetadata(
                MetadataName.RetryAfter, out var retryAfter))
            response.Headers.Add(
                "Retry-After",
                ((int)retryAfter.TotalSeconds).ToString(
                    NumberFormatInfo.InvariantInfo));

        return response;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing) _rateLimiter.Dispose();
    }
}