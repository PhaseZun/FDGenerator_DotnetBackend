using AuthApi.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthApi.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // public async Task<LoginResponse> ValidateUserAsync(LoginRequest request)
        // {
        //     // Replace with your Spring Boot API endpoint
        //     string springBootUrl = "http://localhost:8080/api/auth/login";

        //     var response = await _httpClient.PostAsJsonAsync(springBootUrl, request);

        //     if (response.IsSuccessStatusCode)
        //     {
        //         var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        //         return result ?? new LoginResponse { Success = false, Message = "Invalid response from Spring Boot API" };
        //     }

        //     return new LoginResponse
        //     {
        //         Success = false,
        //         Message = "Failed to call Spring Boot API"
        //     };
        // }

   public async Task<LoginResponse> ValidateUserAsync(LoginRequest request)
   {
    // Simulate some async delay like a real API call
      await Task.Delay(100);

      // Mocked/dummy response
    
        return new LoginResponse
        {
            Success = true,
            Token = "dummy-token-123",
            Message = "Login successful (mocked)"
        };
       
    }

    }
}
