using Microsoft.AspNetCore.Mvc;
using AuthApi.Services;
using AuthApi.Models;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FDListController : ControllerBase
    {
        private static readonly List<FDModel> fdList = new List<FDModel>
        {
            new FDModel { Id = 1, Amount = 50000, InterestRate = 7.5, MaturityDate = new DateTime(2026, 1, 15) },
            new FDModel { Id = 2, Amount = 75000, InterestRate = 7.2, MaturityDate = new DateTime(2027, 3, 22) },
            new FDModel { Id = 3, Amount = 100000, InterestRate = 7.8, MaturityDate = new DateTime(2026, 8, 10) },
            new FDModel { Id = 4, Amount = 65000, InterestRate = 6.9, MaturityDate = new DateTime(2025, 12, 1) }
        };

        private readonly PdfService _pdfService;

        public FDListController(PdfService pdfService)
        {
            _pdfService = pdfService;
        }

        // Step 1: Get FD list as JSON
        [HttpGet("list")]
        public IActionResult GetFdList()
        {
            return Ok(fdList);
        }

        // Step 2: Generate PDF from FD list and return
       [HttpGet("downloadpdf")]
        public async Task<IActionResult> DownloadPdf()
        {
            var pdfFileName = "FDList.pdf";

            using var memoryStream = new MemoryStream();
            await _pdfService.GenerateFdPdfAsync(fdList, memoryStream);

            return File(memoryStream.ToArray(), "application/pdf", pdfFileName);
        }


    }
}
