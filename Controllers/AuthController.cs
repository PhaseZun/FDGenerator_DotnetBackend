using System.Text.Json;
using AuthApi.Models;
using AuthApi.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;


namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IDatabase _redisDb;

        public AuthController(AuthService authService, ILogger<AuthController> logger,IConnectionMultiplexer redis)
        {
            _authService = authService;
            _logger = logger;
            _redisDb = redis.GetDatabase();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
             string cacheKey = $"logindetails:{request.Username}";
             var jsonData = await _redisDb.StringGetAsync(cacheKey);
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
               throw new Exception("Invalid userid and password");
            }
            if(jsonData.HasValue)
            {
                using var ms = new MemoryStream(jsonData);
                var redisFdList = await JsonSerializer.DeserializeAsync<LoginResponse>(ms);
                return Ok(redisFdList); 
            }
            _logger.LogInformation("📩 Login attempt: {Username}", request.Username);

            var result = await _authService.ValidateUserAsync(request);

            await _redisDb.StringSetAsync(cacheKey,JsonSerializer.Serialize(result),TimeSpan.FromMinutes(30));
            return Ok(result);

            
        }
    }

        
   
}
