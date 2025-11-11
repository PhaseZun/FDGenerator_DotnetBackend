using AuthApi.Models;

namespace AuthApi.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
      
   public async Task<LoginResponse> ValidateUserAsync(LoginRequest request)
   {
      await Task.Delay(100);
    
        return new LoginResponse
        {
            Success = true,
            Token = "dummy-token-123",
            Message = "Login successful (mocked)"
        };
       
    }

    }
}
