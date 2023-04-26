namespace VSGMarketplaceApi.Repositories.Interfaces
{
    public interface IImageRepository
    {
        string UploadImage(IFormFile image, string publicId, int itemCode);

        string GetImageURL(string itemCode);
    }
}
