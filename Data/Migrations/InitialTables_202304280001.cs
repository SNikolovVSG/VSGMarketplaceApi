using FluentMigrator;
using FluentMigrator.SqlServer;
using Data.Models;

namespace Data.Migrations
{
    [Migration(202304280001)]
    public class InitialTables_202304280001 : Migration
    {
        public override void Up()
        {
            Create.Table("Items")
                .WithColumn("Code").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Price").AsDecimal().WithDefaultValue(0)
                .WithColumn("Category").AsString(150).NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable()
                .WithColumn("QuantityForSale").AsInt32().WithDefaultValue(0)
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("ImageURL").AsString().Nullable()
                .WithColumn("ImagePublicId").AsString().Unique().Nullable();

            Insert.IntoTable("Items")
                .Row(new Item
                {
                    Code = 323,
                    Name = "Name",
                    Price = 323,
                    Category = "Laptops",
                    Quantity = 35,
                    QuantityForSale = 12,
                    Description = "Description",
                    ImageURL = "https://res.cloudinary.com/dd4yoo4sl/image/upload/v1683809132/69642aa9-d6fb-421f-96b4-6873a06ee26b.jpg",
                    ImagePublicId = "69642aa9-d6fb-421f-96b4-6873a06ee26b"
                });

            Insert.IntoTable("Items")
                .Row(new Item
                {
                    Code = 328,
                    Name = "NameV2",
                    Price = 22323,
                    Category = "Desks",
                    Quantity = 27,
                    QuantityForSale = 17,
                    Description = "Second Description",
                    ImageURL = "https://res.cloudinary.com/dd4yoo4sl/image/upload/v1683810267/e51e0a72-6f69-4f8d-a594-1ebf9ca19e76.jpg",
                    ImagePublicId = "e51e0a72-6f69-4f8d-a594-1ebf9ca19e76"
                });

            Create.Table("Orders")
                .WithColumn("Code").AsInt32().NotNullable().PrimaryKey().Identity(1,1)
                .WithColumn("ItemCode").AsInt32().NotNullable().ForeignKey("Items", "Code")
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("OrderPrice").AsDecimal().NotNullable().WithDefaultValue(0)
                .WithColumn("OrderedBy").AsString(150).NotNullable()
                .WithColumn("OrderDate").AsDate().NotNullable()
                .WithColumn("UserId").AsInt32()
                .WithColumn("Status").AsString().WithDefaultValue(Constants.Pending)
                .WithColumn("IsDeleted").AsBoolean().WithDefaultValue(false);

            Create.Table("Users")
                .WithColumn("Id").AsInt32().Unique().PrimaryKey().NotNullable().Identity()
                .WithColumn("Email").AsString().NotNullable()
                .WithColumn("Role").AsString()
                .WithColumn("Password").AsString().NotNullable();

            Insert.IntoTable("Users")
                .WithIdentityInsert()
                .Row(new User
                {
                    Id = 1,
                    Email = "SNikolov@vsgbg.com",
                    Password = "123456",
                    Role = "User"
                });
            Insert.IntoTable("Users")
                .WithIdentityInsert()
                .Row(new User
                {
                    Id = 2,
                    Email = "SVanev@vsgbg.com",
                    Password = "123456",
                    Role = "Admin"
                });

            Create.Table("Logs")
                .WithColumn("Id").AsInt32().Unique().PrimaryKey().NotNullable().Identity()
                .WithColumn("Date").AsDate()
                .WithColumn("Level").AsString()
                .WithColumn("Message").AsString()
                .WithColumn("MachineName").AsString()
                .WithColumn("Logger").AsString();
        }

        public override void Down()
        {
            Delete.Table("Logs");
            Delete.Table("Users");
            Delete.Table("Orders");
            Delete.Table("Items");
        }
    }
}
