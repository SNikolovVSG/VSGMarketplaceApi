using FluentMigrator;

namespace Data.Migrations
{
    [Migration(202306050001)]
    public class LoanUpdate_202306050001 : Migration
    {
        public override void Down()
        {
            Delete.Table("Loans");
        }

        public override void Up()
        {
            Create.Table("Loans")
               .WithColumn("Id").AsInt32().Unique().PrimaryKey().NotNullable().Identity()
               .WithColumn("ItemId").AsInt32().NotNullable().ForeignKey("Items", "Id").OnDeleteOrUpdate(System.Data.Rule.Cascade)
               .WithColumn("OrderedBy").AsString().NotNullable()
               .WithColumn("Quantity").AsInt32().NotNullable()
               .WithColumn("LoanStartDate").AsDateTime2().NotNullable()
               .WithColumn("LoanEndDate").AsDateTime2().Nullable();

            Alter.Table("Items")
                .AddColumn("AvailableQuantity").AsInt32().WithDefaultValue(0).Nullable();

            Insert.IntoTable("Items")
               .Row(new
               {
                   Code = 323,
                   Name = "Name",
                   Price = 323,
                   Category = "Laptops",
                   Quantity = 35,
                   QuantityForSale = 12,
                   AvailableQuantity = 13,
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
                    AvailableQuantity = 10,
                    Description = "Second Description",
                    ImageURL = "https://res.cloudinary.com/dd4yoo4sl/image/upload/v1683810267/e51e0a72-6f69-4f8d-a594-1ebf9ca19e76.jpg",
                    ImagePublicId = "e51e0a72-6f69-4f8d-a594-1ebf9ca19e76",
                    Location = "Plovdiv"
                });
        }
    }
}
