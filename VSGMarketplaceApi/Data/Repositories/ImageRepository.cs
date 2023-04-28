using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

using System.Data.SqlClient;
using Dapper;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Data.Repositories.Interfaces;

namespace VSGMarketplaceApi.Data.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly IConfiguration configuration;
        private readonly string cloudName;
        private readonly string apiKey;
        private readonly string apiSecret;

        private readonly Cloudinary cloudinary;

        public ImageRepository(IConfiguration configuration)
        {
            this.configuration = configuration;

            cloudName = this.configuration.GetValue<string>("CloudinarySettings:CloudName");
            apiKey = this.configuration.GetValue<string>("CloudinarySettings:ApiKey");
            apiSecret = this.configuration.GetValue<string>("CloudinarySettings:ApiSecret");


            cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }

        public async Task<string[]> UploadImageAsync(IFormFile image)
        {
            var result = await cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(image.FileName,
                      image.OpenReadStream()),
                PublicId = Guid.NewGuid().ToString(),
            });

            return new[] { result.SecureUrl.ToString(), result.PublicId.ToString() };
        }

        public async Task<string[]> UpdateImageAsync(IFormFile image, string publicId)
        {
            var result = await cloudinary.DestroyAsync(new DeletionParams(publicId));

            return await UploadImageAsync(image);
        }
    }
}
