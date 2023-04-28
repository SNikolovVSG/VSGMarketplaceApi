using FluentMigrator;

namespace VSGMarketplaceApi.Data.Migrations
{
    [Migration(202304280002)]
    public class UserTableAdd_202304280002 : Migration
    {
        public override void Down()
        {
            Delete.Table("Users");
        }

        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("Id").AsInt32().Unique().PrimaryKey().NotNullable().Identity()
                .WithColumn("Email").AsString().NotNullable()
                .WithColumn("Role").AsString()
                .WithColumn("Password").AsString().NotNullable();
        }
    }
}
