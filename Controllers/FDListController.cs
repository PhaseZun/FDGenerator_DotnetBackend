using Microsoft.AspNetCore.Mvc;
using AuthApi.Services;
using AuthApi.Models;
using StackExchange.Redis;//redis library
using System.Text.Json;


namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FDListController : ControllerBase
    {
        private readonly PdfService _pdfService;
        private readonly ILogger<FDListController> _logger;
        private readonly IDatabase _redisDb;
        
        private static readonly List<FDModel> fdList = new List<FDModel>
        {
            new FDModel
            {
                
                fdId = 1, Amount = 50000, InterestRate = 7.5,
                InvestedDate = new DateTime(2023, 01, 15),
                MaturityDate = new DateTime(2026, 01, 15),
                TenureMonths = 36,
                BankName = "HDFC Bank",
                AccountNumber = "XXXX1234",
                Status = "Active"
            },
            new FDModel
            {
                fdId = 2, Amount = 75000, InterestRate = 7.2,
                InvestedDate = new DateTime(2024, 03, 22),
                MaturityDate = new DateTime(2027, 03, 22),
                TenureMonths = 36,
                BankName = "ICICI Bank",
                AccountNumber = "XXXX5678",
                Status = "Active"
            },
            new FDModel
            {
                fdId = 3, Amount = 100000, InterestRate = 7.8,
                InvestedDate = new DateTime(2023, 08, 10),
                MaturityDate = new DateTime(2026, 08, 10),
                TenureMonths = 36,
                BankName = "SBI",
                AccountNumber = "XXXX9876",
                Status = "Active"
            },
            new FDModel
            {
                fdId = 4, Amount = 65000, InterestRate = 6.9,
                InvestedDate = new DateTime(2022, 12, 01),
                MaturityDate = new DateTime(2025, 12, 01),
                TenureMonths = 36,
                BankName = "Axis Bank",
                AccountNumber = "XXXX6543",
                Status = "Matured"
            },
            new FDModel
            {
                fdId = 5, Amount = 85000, InterestRate = 7.0,
                InvestedDate = new DateTime(2024, 06, 15),
                MaturityDate = new DateTime(2027, 06, 15),
                TenureMonths = 36,
                BankName = "Kotak Mahindra Bank",
                AccountNumber = "XXXX1122",
                Status = "Active"
            }
        };

        public FDListController(PdfService pdfService,ILogger<FDListController> logger,IConnectionMultiplexer redis)
        {
            _pdfService = pdfService;
            _logger = logger;
            _redisDb = redis.GetDatabase();//redis databse call in object 
        }

        // Step 1: Get FD list as JSON
        [HttpGet("list/{userId}/{token}")]
        public async Task<IActionResult> GetFdList(string userId,string token)
        {   
                string cacheKey = $"FDListKey:{userId}";
                var jsonData = await _redisDb.StringGetAsync(cacheKey);
                if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid or missing token.");
                }
            if (jsonData.HasValue)
            {
                using var ms = new MemoryStream(jsonData);
                var redisFdList = await JsonSerializer.DeserializeAsync<List<FDModel>>(ms);
                //var redisFdList = JsonSerializer.Deserialize<List<FDModel>>(jsonData.ToString());
                return Ok(redisFdList); 
            }
                _logger.LogInformation("✅ Token valid. Returning {Count} FDs and Stored the data into the redis", fdList.Count);
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(fdList), TimeSpan.FromMinutes(5)  // ✅ TTL = 1 minutes
                );
                
                return Ok(fdList);
        }

         [HttpGet("listid/{userId}/{fdId}/{token}")]
        public async Task<IActionResult> GetFdListID(string userId,string fdId,string token)
        {   
                string cacheKey = $"FDListKey:{userId}-{fdId}";
                var jsonData = await _redisDb.StringGetAsync(cacheKey);
                if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid or missing token.");
                }
            if (jsonData.HasValue)
            {
                //using var ms = new MemoryStream(jsonData);
                // var redisFdList = await JsonSerializer.DeserializeAsync<List<FDModel>>(ms);
                // var fdredis = redisFdList?.FirstOrDefault(f => Convert.ToString(f.fdId) == fdId);
                var fdredis = JsonSerializer.Deserialize<FDModel>(jsonData);
                return Ok(fdredis); 
            }
                _logger.LogInformation("✅ Token valid. Returning {Count} FDs and Stored the data into the redis", fdList.Count);
                var fd = fdList?.FirstOrDefault(f => Convert.ToString(f.fdId) == fdId);
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(fd), TimeSpan.FromMinutes(5)  // ✅ TTL = 1 minutes
                );
                await Task.Delay(53);
                
                return Ok(fd);
        }

        // Step 2: Generate PDF from FD list and return
       [HttpGet("downloadpdf/{userId}/{fdId}/{token}")]
        public async Task<IActionResult> DownloadPdf(string userId,int fdId,string token)
        {
            FDModel fd = null;
            string cacheKey = $"FDListKey:{userId}-{fdId}";
            var jsonData = await _redisDb.StringGetAsync(cacheKey);
            if(string.IsNullOrEmpty(token))
            {
                    return Unauthorized("Invalid or missing token.");
            }
            else
            {
                if (jsonData.HasValue)
                {
                    fd = JsonSerializer.Deserialize<FDModel>(jsonData);
                }
                _logger.LogInformation("📩 Received request to download PDF for FD ID: {Id}", fdId);
                 fd = fdList.FirstOrDefault(f => f.fdId == fdId);
                if (fd == null) return NotFound();
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(fd), TimeSpan.FromMinutes(5) );
                using var memoryStream = new MemoryStream();
                await _pdfService.GenerateFdPdfAsync(new List<FDModel> { fd }, memoryStream);
                
              _logger.LogInformation("✅ PDF successfully generated for FD ID: {Id}", fdId);
            return File(memoryStream.ToArray(), "application/pdf", $"FD_{fdId}.pdf"); 
            }
            
        }
        



    }
}
