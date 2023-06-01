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
            Create.Table("Logs")
                .WithColumn("Id").AsInt32().Unique().PrimaryKey().NotNullable().Identity()
                .WithColumn("Date").AsDate()
                .WithColumn("Level").AsString()
                .WithColumn("Message").AsString()
                .WithColumn("MachineName").AsString()
                .WithColumn("Logger").AsString();

            Create.Table("Items")
                .WithColumn("Id").AsInt32().Unique().PrimaryKey().NotNullable().Identity()
                .WithColumn("Code").AsString().NotNullable()
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Price").AsDecimal().WithDefaultValue(0)
                .WithColumn("Category").AsString(150).NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable()
                .WithColumn("QuantityForSale").AsInt32().WithDefaultValue(0)
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("Location").AsString().NotNullable()
                .WithColumn("ImageURL").AsString().Nullable()
                .WithColumn("ImagePublicId").AsString().Nullable();

            Create.UniqueConstraint("ItemsCodeAndLocation").OnTable("Items").Columns("Code", "Location");

            Create.Index("ItemsImagePublicIdNullsCheck").OnTable("Items").OnColumn("ImagePublicId").Ascending().NullsNotDistinct();

            Insert.IntoTable("Items")
                .Row(new 
                {
                    Code = 323,
                    Name = "Name",
                    Price = 323,
                    Category = "Laptops",
                    Quantity = 35,
                    QuantityForSale = 12,
                    Description = "Description",
                    Location = "Veliko Tarnovo",
                    ImageURL = "https://res.cloudinary.com/dd4yoo4sl/image/upload/v1683809132/69642aa9-d6fb-421f-96b4-6873a06ee26b.jpg",
                    ImagePublicId = "69642aa9-d6fb-421f-96b4-6873a06ee26b",
                });

            Insert.IntoTable("Items")
                .Row(new 
                {
                    Code = 328,
                    Name = "NameV2",
                    Price = 22323,
                    Category = "Furniture",
                    Quantity = 27,
                    QuantityForSale = 17,
                    Description = "Second Description",
                    ImageURL = "https://res.cloudinary.com/dd4yoo4sl/image/upload/v1683810267/e51e0a72-6f69-4f8d-a594-1ebf9ca19e76.jpg",
                    ImagePublicId = "e51e0a72-6f69-4f8d-a594-1ebf9ca19e76",
                    Location = "Plovdiv"
                });

            Create.Table("Orders")
                .WithColumn("Id").AsInt32().Unique().PrimaryKey().NotNullable().Identity()
                .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("Items", "Id").OnUpdate(System.Data.Rule.Cascade)
                .WithColumn("ItemCode").AsInt32().NotNullable()
                .WithColumn("Location").AsString().NotNullable()
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("OrderPrice").AsDecimal().NotNullable().WithDefaultValue(0)
                .WithColumn("OrderedBy").AsString(150).NotNullable()
                .WithColumn("OrderDate").AsDateTime2().NotNullable()
                .WithColumn("Status").AsString().WithDefaultValue(Constants.Pending)
                .WithColumn("IsDeleted").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.ForeignKey("OrderItemCodeAndLocation");

            Delete.Table("Logs");
            Delete.Table("Orders");
            Delete.Table("Items");
        }
    }
}
