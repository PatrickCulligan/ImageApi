using Microsoft.AspNetCore.Http;

namespace ImageApi.Models
{
    public class ImageUploadRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}
