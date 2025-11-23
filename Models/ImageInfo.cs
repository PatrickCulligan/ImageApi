namespace ImageApi.Models
{
    public class ImageInfo
    {
        public string Id { get; set; } = null!;
        public string Filename { get; set; } = null!;
        public long Length { get; set; }
        public DateTime UploadDate { get; set; }
        public string ContentType { get; set; } = null!;
    }
}
