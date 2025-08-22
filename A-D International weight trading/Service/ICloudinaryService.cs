namespace A_D_International_weight_trading.Service
{
    public interface ICloudinaryService
    {
        Task<(string imageUrl, string publicId)> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);
        Task<List<(string imageUrl, string publicId)>> UploadMultipleImagesAsync(List<IFormFile> files);
    }
}
