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
                .WithColumn("QuantityForSale").AsInt32().WithDefaultValue(0).Nullable()
                .WithColumn("Description").AsString().Nullable()
                .WithColumn("Location").AsString().NotNullable()
                .WithColumn("ImageURL").AsString().Nullable()
                .WithColumn("ImagePublicId").AsString().Nullable();

            Create.UniqueConstraint("ItemsCodeAndLocation").OnTable("Items").Columns("Code", "Location");

            Create.Index("ItemsImagePublicIdNullsCheck").OnTable("Items").OnColumn("ImagePublicId").Ascending().NullsNotDistinct();

            Create.Table("Orders")
                .WithColumn("Id").AsInt32().Unique().PrimaryKey().NotNullable().Identity()
                .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("Items", "Id").OnDeleteOrUpdate(System.Data.Rule.Cascade)
                .WithColumn("ItemCode").AsString().NotNullable()
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
            Delete.Table("Orders");
            //Delete.Table("Loans");
            Delete.Table("Logs");
            Delete.Table("Items");
        }
    }
}
