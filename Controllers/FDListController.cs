using Microsoft.AspNetCore.Mvc;
using AuthApi.Services;
using AuthApi.Models;
using StackExchange.Redis;//redis library
using System.Text.Json;
using System.Text;
using System.Net;


namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FDListController : ControllerBase
    {
        private readonly PdfService _pdfService;
        private readonly ILogger<FDListController> _logger;
        private readonly IDatabase _redisDb;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FDListController(PdfService pdfService,ILogger<FDListController> logger,IConnectionMultiplexer redis,IConfiguration configuration,HttpClient httpClient)
        {
            _pdfService = pdfService;
            _logger = logger;
            _redisDb = redis.GetDatabase();//redis databse call in object 
            _configuration = configuration;
            _httpClient = httpClient;
        }

        // Step 1: Get FD list as JSON
        [HttpGet("list/{userId}/{token}")]
        public async Task<IActionResult> GetFdList(string userId,string token)
        {   
                var baseUrl = _configuration["ExternalApis:FDListApiBaseUrl"];
                var apiUrl = $"{baseUrl}list";
                string cacheKey = $"FDListsKey:{userId}";
                var jsonData = await _redisDb.StringGetAsync(cacheKey);
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid or missing token.");
                }
            if (jsonData.HasValue)
            {
                using var ms = new MemoryStream(jsonData!);
                var redisFdList = await JsonSerializer.DeserializeAsync<List<FDModel>>(ms);
                //var redisFdList = JsonSerializer.Deserialize<List<FDModel>>(jsonData.ToString());
                return Ok(redisFdList); 
            }
             var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);
             requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);
            var responseJson = await response.Content.ReadAsStringAsync();
             var dataFdList = JsonSerializer.Deserialize<List<FDModel>>(
             responseJson,
             new JsonSerializerOptions
              {
             PropertyNameCaseInsensitive = true
             });
                await _redisDb.StringSetAsync(cacheKey,JsonSerializer.Serialize(dataFdList) , TimeSpan.FromMinutes(5)  // ✅ TTL = 30 minutes
                );
                
                return Ok(dataFdList);
        }

         [HttpGet("listid/{userId}/{fdId}/{token}")]
        public async Task<IActionResult> GetFdListID(string userId,string fdId,string token)
        {   
                var baseUrl = _configuration["ExternalApis:FDListApiBaseUrl"];
                var apiUrl = $"{baseUrl}list";
                string cacheKey = $"FDListIdKey:{userId}-{fdId}";
                var jsonData = await _redisDb.StringGetAsync(cacheKey);
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid or missing token.");
                }
            if (jsonData.HasValue)
            {
                var fdredis = JsonSerializer.Deserialize<FDModel>(jsonData!);
                return Ok(fdredis); 
            }
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);
               if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return StatusCode((int)response.StatusCode);        
                }
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode);
                }
                var responseJson = await response.Content.ReadAsStringAsync();
             var fd = JsonSerializer.Deserialize<FDModel>(
                      responseJson,
                     new JsonSerializerOptions
                    {
                     PropertyNameCaseInsensitive = true
                });
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(fd), TimeSpan.FromMinutes(5)  // ✅ TTL = 30 minutes
                );
                
                return Ok(fd);
        }

        // Step 2: Generate PDF from FD list and return
      [HttpGet("downloadpdf/{userId}/{fdId}/{token}")]
        public async Task<IActionResult> DownloadPdf(string userId, int fdId, string token)
        {
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Invalid or missing token.");

            var baseUrl = _configuration["ExternalApis:FDListApiBaseUrl"];
            var apiUrl = $"{baseUrl}fd/{fdId}";

            string cacheKey = $"FDListPDFKey:{userId}-{fdId}";
            List<FDModel>? fdList;

            var jsonData = await _redisDb.StringGetAsync(cacheKey);

            if (jsonData.HasValue)
            {
                fdList = JsonSerializer.Deserialize<List<FDModel>>(jsonData!);
            }
            else
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                requestMessage.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                        return StatusCode((int)response.StatusCode);
                }
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode);

                var responseJson = await response.Content.ReadAsStringAsync();

                fdList = JsonSerializer.Deserialize<List<FDModel>>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (fdList == null || !fdList.Any())
                    return NotFound();

                await _redisDb.StringSetAsync(
                    cacheKey,
                    JsonSerializer.Serialize(fdList),
                    TimeSpan.FromMinutes(5)
                );
            }

            using var memoryStream = new MemoryStream();
            await _pdfService.GenerateFdPdfAsync(fdList!, memoryStream);

            return File(memoryStream.ToArray(), "application/pdf", $"FD_{fdId}.pdf");
        }
    }
}
