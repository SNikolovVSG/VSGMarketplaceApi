using VSGMarketplaceApi.DTOs;

namespace VSGMarketplaceApi.Repositories.Interfaces
{
    public interface IImageRepository
    {
        Task<string[]> UploadImageAsync(IFormFile image);

        Task<string[]> UpdateImageAsync(IFormFile image, string imageURL);
    }
}
