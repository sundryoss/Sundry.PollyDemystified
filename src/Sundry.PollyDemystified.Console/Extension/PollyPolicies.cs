using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Sundry.PollyDemystified.Console.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Sundry.PollyDemystified.Console.Extension;

public static class PollyRetryPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetConstantBackofffPolicy()
    {
        var delay = Backoff.ConstantBackoff(TimeSpan.FromMilliseconds(100), retryCount: 3);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(delay);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTokenRefresher(IServiceProvider provider, HttpRequestMessage request)
    {
        var delay = Backoff.ConstantBackoff(TimeSpan.FromMilliseconds(100), retryCount: 3);

        return Policy<HttpResponseMessage>
                .HandleResult(msg => msg.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                .WaitAndRetryAsync(delay, async (_, _, _, _) =>
            {
                await provider.GetRequiredService<ITokenService>().RefreshToken();
                request.SetPolicyExecutionContext(new Context());
            });
    }
    public static IAsyncPolicy<HttpResponseMessage> GetExponentialBackoffPolicy()
    {
        var delay = Backoff.ExponentialBackoff(TimeSpan.FromMilliseconds(100), retryCount: 3);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetDecorrelatedJitterBackoffV2Policy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);
    }
}
