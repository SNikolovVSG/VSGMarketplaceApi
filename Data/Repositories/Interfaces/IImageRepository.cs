using Microsoft.AspNetCore.Http;

namespace Data.Repositories.Interfaces
{
    public interface IImageRepository
    {
        Task<string[]> UploadImageAsync(IFormFile image);

        Task<string[]> UpdateImageAsync(IFormFile image, string imageURL);

        Task DeleteImageAsync(string publicId);
    }
}
