using AuthApi.Models;
using AuthApi.Services;
using Microsoft.AspNetCore.Mvc;


namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Username or password is missing"
                });
            }

            _logger.LogInformation("📩 Login attempt: {Username}", request.Username);

            var result = await _authService.ValidateUserAsync(request);

            if (!result.Success)
            {
                _logger.LogWarning("❌ Invalid login for user {Username}", request.Username);
                return Unauthorized(result);
            }

            var token = result.Token;

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                userId=request.Username
            });
        }
    }

        
   
}
