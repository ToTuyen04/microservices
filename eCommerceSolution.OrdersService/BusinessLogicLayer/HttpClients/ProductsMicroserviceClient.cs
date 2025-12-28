using BusinessLogicLayer.DTO;
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
    public ProductsMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
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
        if(product == null)
            throw new ArgumentException("Invalid product ID");
        return product;
    }
}
