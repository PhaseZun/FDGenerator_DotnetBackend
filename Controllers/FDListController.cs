using Microsoft.AspNetCore.Mvc;
using AuthApi.Services;
using AuthApi.Models;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FDListController : ControllerBase
    {
        private readonly PdfService _pdfService;
        private readonly ILogger<FDListController> _logger;
        
        private static readonly List<FDModel> fdList = new List<FDModel>
        {
            new FDModel
            {
                Id = 1, Amount = 50000, InterestRate = 7.5,
                InvestedDate = new DateTime(2023, 01, 15),
                MaturityDate = new DateTime(2026, 01, 15),
                TenureMonths = 36,
                BankName = "HDFC Bank",
                AccountNumber = "XXXX1234",
                Status = "Active"
            },
            new FDModel
            {
                Id = 2, Amount = 75000, InterestRate = 7.2,
                InvestedDate = new DateTime(2024, 03, 22),
                MaturityDate = new DateTime(2027, 03, 22),
                TenureMonths = 36,
                BankName = "ICICI Bank",
                AccountNumber = "XXXX5678",
                Status = "Active"
            },
            new FDModel
            {
                Id = 3, Amount = 100000, InterestRate = 7.8,
                InvestedDate = new DateTime(2023, 08, 10),
                MaturityDate = new DateTime(2026, 08, 10),
                TenureMonths = 36,
                BankName = "SBI",
                AccountNumber = "XXXX9876",
                Status = "Active"
            },
            new FDModel
            {
                Id = 4, Amount = 65000, InterestRate = 6.9,
                InvestedDate = new DateTime(2022, 12, 01),
                MaturityDate = new DateTime(2025, 12, 01),
                TenureMonths = 36,
                BankName = "Axis Bank",
                AccountNumber = "XXXX6543",
                Status = "Matured"
            },
            new FDModel
            {
                Id = 5, Amount = 85000, InterestRate = 7.0,
                InvestedDate = new DateTime(2024, 06, 15),
                MaturityDate = new DateTime(2027, 06, 15),
                TenureMonths = 36,
                BankName = "Kotak Mahindra Bank",
                AccountNumber = "XXXX1122",
                Status = "Active"
            }
        };

        public FDListController(PdfService pdfService,ILogger<FDListController> logger)
        {
            _pdfService = pdfService;
            _logger = logger;
        }

        // Step 1: Get FD list as JSON
        [HttpGet("list/{userid}/{token}")]
        public IActionResult GetFdList(string token,string userid)
        {   
                _logger.LogInformation("📩 Received GET request for FD list with token: {Token}", token);
                if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(userid))
                {
                    return Unauthorized("Invalid or missing token.");
                }
                _logger.LogInformation("✅ Token valid. Returning {Count} FDs", fdList.Count);
                return Ok(fdList);
        }

        // Step 2: Generate PDF from FD list and return
       [HttpGet("downloadpdf/{id}/{token}")]
        public async Task<IActionResult> DownloadPdf(int id,string token)
        {
            if(string.IsNullOrEmpty(token))
            {
                    return Unauthorized("Invalid or missing token.");
            }
            else
            {
              _logger.LogInformation("📩 Received request to download PDF for FD ID: {Id}", id);
                var fd = fdList.FirstOrDefault(f => f.Id == id);
                if (fd == null) return NotFound();

                using var memoryStream = new MemoryStream();
                await _pdfService.GenerateFdPdfAsync(new List<FDModel> { fd }, memoryStream);
                
              _logger.LogInformation("✅ PDF successfully generated for FD ID: {Id}", id);
            return File(memoryStream.ToArray(), "application/pdf", $"FD_{id}.pdf"); 
            }
            
        }



    }
}
