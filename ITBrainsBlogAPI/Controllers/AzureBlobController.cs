using ITBrainsBlogAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITBrainsBlogAPI.Controllers
{
    [Route("blog/[controller]")]
    [ApiController]
    public class AzureBlobController : ControllerBase
    {
        public AzureBlobService _service;

        public AzureBlobController(AzureBlobService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            var response = await _service.UploadFiles(files);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetBlobList()
        {
            var response = await _service.GetUploadedBlob();
            return Ok(response);
        }
    }
}
