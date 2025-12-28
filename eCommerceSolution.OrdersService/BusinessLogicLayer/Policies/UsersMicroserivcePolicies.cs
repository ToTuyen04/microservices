
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace BusinessLogicLayer.Policies;
public class UsersMicroserivcePolicies : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroserivcePolicies> _logger;
    public UsersMicroserivcePolicies(ILogger<UsersMicroserivcePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3, // Number of retries
            durationOfBreak: TimeSpan.FromMinutes(2),
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

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        //Microsoft.Extensions.Http.Polly NuGet package
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 5, // Number of retries
            sleepDurationProvider: retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                // TO DO: Add logs
                _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
            });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(1500));
        return policy;
    }
}
