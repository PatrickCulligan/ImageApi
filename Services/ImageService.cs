using ImageApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Options;
using System.Linq; // make sure this is here

namespace ImageApi.Services
{
    public class ImageService
    {
        private readonly GridFSBucket _bucket;

        public ImageService(IMongoClient client, IOptions<MongoDbSettings> options)
        {
            var db = client.GetDatabase(options.Value.Database);
            _bucket = new GridFSBucket(db);
        }

        public async Task<string> UploadAsync(Stream stream, string filename, string contentType)
        {
            var uploadOptions = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    { "contentType", contentType }
                }
            };

            var id = await _bucket.UploadFromStreamAsync(filename, stream, uploadOptions);
            return id.ToString();
        }

        public async Task<(Stream Stream, string ContentType)?> DownloadAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return null;

            try
            {
                var downloadStream = await _bucket.OpenDownloadStreamAsync(objectId);

                var contentType = downloadStream.FileInfo.Metadata?["contentType"]?.AsString
                                  ?? "application/octet-stream";

                // Copy into a MemoryStream so ASP.NET can safely dispose it
                var ms = new MemoryStream();
                await downloadStream.CopyToAsync(ms);
                ms.Position = 0;
                await downloadStream.DisposeAsync();

                return (ms, contentType);
            }
            catch (GridFSFileNotFoundException)
            {
                return null;
            }

        }
        public async Task<List<ImageInfo>> ListAsync()
        {
            var filter = Builders<GridFSFileInfo>.Filter.Empty;

            using var cursor = await _bucket.FindAsync(filter);
            var files = await cursor.ToListAsync();

            return files.Select(f => new ImageInfo
            {
                Id = f.Id.ToString(),
                Filename = f.Filename,
                Length = f.Length,
                UploadDate = DateTime.SpecifyKind(f.UploadDateTime, DateTimeKind.Utc),
                ContentType = f.Metadata?["contentType"]?.AsString ?? "application/octet-stream"
            }).ToList();
        }
        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var oid))
                return false;

            try
            {
                await _bucket.DeleteAsync(oid);
                return true;
            }
            catch (GridFSFileNotFoundException)
            {
                return false;
            }
        }

    }
}
