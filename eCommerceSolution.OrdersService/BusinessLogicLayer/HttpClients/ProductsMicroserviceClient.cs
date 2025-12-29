using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.HttpClients;
public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        try
        {
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"/api/products/search/product-id/{productID}");
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
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
