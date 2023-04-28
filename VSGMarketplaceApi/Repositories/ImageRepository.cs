using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

using System.Data.SqlClient;
using Dapper;
using VSGMarketplaceApi.DTOs;

namespace VSGMarketplaceApi.Repositories.Interfaces
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

            this.cloudName = this.configuration.GetValue<string>("CloudinarySettings:CloudName");
            this.apiKey = this.configuration.GetValue<string>("CloudinarySettings:ApiKey");
            this.apiSecret = this.configuration.GetValue<string>("CloudinarySettings:ApiSecret");


            this.cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }

        public async Task<string[]> UploadImageAsync(IFormFile image)
        {
            var result = await this.cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(image.FileName,
                      image.OpenReadStream()),
                PublicId = Guid.NewGuid().ToString(),
            });

            return new[] { result.SecureUrl.ToString(), result.PublicId.ToString() };
        }

        public async Task<string[]> UpdateImageAsync(IFormFile image, string publicId)
        {
            var result = await this.cloudinary.DestroyAsync(new DeletionParams(publicId));

            return await UploadImageAsync(image);
        }
    }
}
