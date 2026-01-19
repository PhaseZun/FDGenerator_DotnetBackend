using System.Text;
using System.Text.Json;
using AuthApi.Models;

namespace AuthApi.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient,IConfiguration configuration )
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
      
   public async Task<LoginResponse> ValidateUserAsync(LoginRequest request)
  {
    var baseUrl = _configuration["ExternalApis:AuthApiBaseUrl"];

    if (string.IsNullOrEmpty(baseUrl))
        throw new Exception("Auth API URL not configured");

    var apiUrl = $"{baseUrl}login";
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync(apiUrl, content);

    if (!response.IsSuccessStatusCode)
    {
        throw new Exception("Invalid userid and password");
    }
    var responseJson = await response.Content.ReadAsStringAsync();
    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(
    responseJson,
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
    return loginResponse!;
  }

    }
}
