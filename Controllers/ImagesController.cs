using ImageApi.Models;
using ImageApi.Services;
using Microsoft.AspNetCore.Mvc;


namespace ImageApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ImageService _imageService;

        public ImagesController(ImageService imageService)
        {
            _imageService = imageService;
        }

        // NOTE: model + Consumes attribute fixes Swagger
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] ImageUploadRequest request)
        {
            var file = request.File;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            await using var stream = file.OpenReadStream();
            var id = await _imageService.UploadAsync(stream, file.FileName, file.ContentType);

            return Ok(new { id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(string id)
        {
            var result = await _imageService.DownloadAsync(id);
            if (result == null)
                return NotFound();

            var (stream, contentType) = result.Value;
            return File(stream, contentType);
        }
        // GET api/images
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var files = await _imageService.ListAsync();
            return Ok(files);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _imageService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Image not found" });

            return Ok(new { message = "Deleted" });
        }
    }
}
