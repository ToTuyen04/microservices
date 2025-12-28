using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients;
public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDTO> GetUserByUserID(Guid userID)
    {
        try
        {
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"/api/users/{userID}");
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
                    //throw new HttpRequestException($"Http request fail with status code: {httpResponseMessage.StatusCode}");
                    return new UserDTO(
                        PersonName: "Temporarily Unavailable",
                        Email: "Temporarily Unavailable",
                        Gender: "Temporarily Unavailable",
                        UserID: Guid.Empty
                        );
                }
            }

            UserDTO? user = await httpResponseMessage.Content.ReadFromJsonAsync<UserDTO>();
            if (user == null)
                throw new ArgumentException("Invalid user ID");
            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request fail Circuit breaker is in OPEN state. Returning dummy data.");
            return new UserDTO(
                        PersonName: "Temporarily Unavailable (Circuit breaker)",
                        Email: "Temporarily Unavailable (Circuit breaker)",
                        Gender: "Temporarily Unavailable (Circuit breaker)",
                        UserID: Guid.Empty
                        );
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Error fetching data due to time out rejected exception. Returning dummy data.");
            return new UserDTO(
                        PersonName: "Temporarily Unavailable (Timeout)",
                        Email: "Temporarily Unavailable (Timeout)",
                        Gender: "Temporarily Unavailable (Timeout)",
                        UserID: Guid.Empty
                        );
        }
    }
}

