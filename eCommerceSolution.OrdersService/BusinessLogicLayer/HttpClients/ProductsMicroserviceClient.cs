using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogicLayer.HttpClients;
public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        try
        {
            //kEY: product:123
            //Value: { "ProductName": "..",...}

            string cacheKey = $"product:{productID}";
            string? cachedProduct = await _distributedCache.GetStringAsync(cacheKey);

            // nếu Redis có lưu cache của product thì return data từ cache luon
            if(cachedProduct != null)
            {
                _logger.LogInformation("Product found in cache");
                ProductDTO? productFromCache = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                return productFromCache;
            }

            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"/gateway/products/search/product-id/{productID}");
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                if(httpResponseMessage.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    ProductDTO? productFromFallback = await httpResponseMessage.Content.ReadFromJsonAsync<ProductDTO>();
                    if (productFromFallback == null)
                        throw new NotImplementedException();
                    return productFromFallback;
                }
                else if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException($"Http request fail with status code: {httpResponseMessage.StatusCode}");
                }
            }
            ProductDTO? product = await httpResponseMessage.Content.ReadFromJsonAsync<ProductDTO>();
            if (product == null)
                throw new ArgumentException("Invalid product ID");

            //lưu product vào cache trước khi return
            string productString = JsonSerializer.Serialize(product);
            string cacheKeyToWrite = $"product:{productID}";
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            await _distributedCache.SetStringAsync(cacheKeyToWrite, productString, options);
            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "BulkheadRejectedException blocks the request since the request queue is full");
            return new ProductDTO(
                        ProductID: Guid.Empty,
                        ProductName: "Temporarily Unavailable (Bulkhead)",
                        Category: "Temporarily Unavailable (Bulkhead)",
                        UnitPrice: 0,
                        QuantityInStock: 0
                        );
        }
    }
}
