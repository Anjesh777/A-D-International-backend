
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace A_D_International_weight_trading.Service
{
    public class CloudinaryService : ICloudinaryService
    {

        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;


        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<(string imageUrl, string publicId)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty");

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Invalid file type. Only JPEG, PNG, and WebP are allowed.");

            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File size cannot exceed 10MB");

            try
            {
                using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "products", 
                    Transformation = new Transformation()
                        .Width(1200)
                        .Height(1200)
                        .Crop("limit")
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                    throw new Exception($"Cloudinary upload error: {result.Error.Message}");

                return (result.SecureUrl.ToString(), result.PublicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Cloudinary: {PublicId}", publicId);
                return false;
            }
        }

        public async Task<List<(string imageUrl, string publicId)>> UploadMultipleImagesAsync(List<IFormFile> files)
        {
            var results = new List<(string imageUrl, string publicId)>();

            if (files == null || !files.Any())
                return results;

            if (files.Count > 7)
                throw new ArgumentException("Maximum 7 images allowed per product");

            foreach (var file in files)
            {
                try
                {
                    var result = await UploadImageAsync(file);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image: {FileName}", file.FileName);
                }
            }

            return results;
        }
    }
}




    

