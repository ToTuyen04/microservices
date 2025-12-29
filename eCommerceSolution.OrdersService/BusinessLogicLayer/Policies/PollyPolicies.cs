
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace BusinessLogicLayer.Policies;
public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<UsersMicroserivcePolicies> _logger;
    public PollyPolicies(ILogger<UsersMicroserivcePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, // Number of retries
            durationOfBreak: durationOfBreak,
            onBreak: (outcome, timespan) =>
            {
                // TO DO: Add logs
                _logger.LogInformation($"Circuit breaker opened for {timespan.TotalMinutes} minutes due to consecutive 3 failures. The subsequent requests will be blocked");
            },
            onReset: () =>
            {
                _logger.LogInformation($"Circuit breaker closed. The subsequent will be allowed.");
            });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        //Microsoft.Extensions.Http.Polly NuGet package
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: retryCount, // Number of retries
            sleepDurationProvider: retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                // TO DO: Add logs
                _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
            });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);
        return policy;
    }
}
