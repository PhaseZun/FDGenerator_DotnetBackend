using Microsoft.AspNetCore.Mvc;
using AuthApi.Services;
using System.Threading.Tasks;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MinioController : ControllerBase
    {
        private readonly MinioService _minioService;

        public MinioController(MinioService minioService)
        {
            _minioService = minioService;
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadPdf(string fileName)
        {
            var stream = await _minioService.DownloadPdfAsync(fileName);

            if (stream == null)
                return NotFound("File not found in MinIO.");

            // Return as downloadable PDF
            return File(stream, "application/pdf", fileName);
        }
    }
}
