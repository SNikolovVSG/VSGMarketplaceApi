using FluentMigrator;
using VSGMarketplaceApi.Data.Models;

namespace VSGMarketplaceApi.Data.Migrations
{
    [Migration(202304280001)]
    public class InitialTables_202304280001 : Migration
    {
        public override void Down()
        {
            Delete.Table("Orders");
            Delete.Table("Items");
        }

        public override void Up()
        {
            Create.Table("Items")
                .WithColumn("Code").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Price").AsInt16().WithDefaultValue(0)
                .WithColumn("Category").AsString(150).NotNullable()
                .WithColumn("Quantity").AsInt16().NotNullable()
                .WithColumn("QuantityForSale").AsInt16().WithDefaultValue(0)
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("ImageURL").AsString().Nullable()
                .WithColumn("ImagePublicId").AsString().Unique().Nullable();

            Create.Table("Orders")
                .WithColumn("Code").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("ItemCode").AsInt32().NotNullable().ForeignKey("Items", "Code")
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Quantity").AsInt16().NotNullable().WithDefaultValue(0)
                .WithColumn("OrderedBy").AsString(150).NotNullable()
                .WithColumn("OrderDate").AsDate().NotNullable()
                .WithColumn("Status").AsString().WithDefaultValue(Constants.Pending)
                .WithColumn("IsDeleted").AsBoolean().WithDefaultValue(false);


        }
    }
}
