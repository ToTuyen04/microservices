using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.Policies;
public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;
    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: 2,
            maxQueuingActions: 4,
            onBulkheadRejectedAsync: (context) =>
            {
                _logger.LogWarning("BulkheadIsolation triggered. Can't send any more requests since the queue is full");

                throw new BulkheadRejectedException("Bulkhead queue is full.");
            });
        return policy;
    }
    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r =>
        !r.IsSuccessStatusCode)
            .FallbackAsync(async (context) =>
            {
                _logger.LogWarning("Fallback triggered: The request failed, returning dummy dataa");

                ProductDTO productDTO = new ProductDTO(
                    ProductID: Guid.Empty,
                    ProductName: "Temporarily Unavailable (Fallback)",
                    Category: "Temporarily Unavailable (Fallback)",
                    UnitPrice: 0,
                    QuantityInStock: 0
                );

                HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(productDTO), Encoding.UTF8, "application/json")
                };
                return response;
            });
        return policy;
    }
}
