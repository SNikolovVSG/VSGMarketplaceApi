using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

using System.Data.SqlClient;
using Dapper;

namespace VSGMarketplaceApi.Repositories.Interfaces
{
    public class ImageRepository : IImageRepository
    {
        private readonly IConfiguration configuration;
        private readonly string cloudName;
        private readonly string apiKey;
        private readonly string apiSecret;

        private readonly string connectionString;

        public ImageRepository(IConfiguration configuration)
        {
            this.configuration = configuration;

            this.cloudName = this.configuration.GetValue<string>("CloudinarySettings:CloudName");
            this.apiKey = this.configuration.GetValue<string>("CloudinarySettings:ApiKey");
            this.apiSecret = this.configuration.GetValue<string>("CloudinarySettings:ApiSecret");

            this.connectionString = this.configuration.GetConnectionString("DefaultConnection");
        }


        public string GetImageURL(string itemCode)
        {
            using var connection = new SqlConnection(this.connectionString);
            var ImageURL = connection.QueryFirst<string>("select imageURL from Images where itemCode = @ItemCode", new { ItemCode = itemCode });

            return ImageURL;
        }

        public string UploadImage(IFormFile image, string publicId, int itemCode)
        {
            var cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
            var result = cloudinary.Upload(new ImageUploadParams
            {
                File = new FileDescription(image.FileName,
                      image.OpenReadStream()),
                PublicId = publicId
            });

            using var connection = new SqlConnection(connectionString);
            var addingImageToDbSQL = "INSERT INTO Images (imageURL, itemCode) VALUES (@ImageURL, @ItemCode)";
            var addingImageToDb = connection.Execute(addingImageToDbSQL, new { ImageURL = result.SecureUrl.ToString(), ItemCode = itemCode });

            return result.SecureUrl.ToString();
        }
    }
}
