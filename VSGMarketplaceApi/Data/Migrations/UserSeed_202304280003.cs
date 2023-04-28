using FluentMigrator;
using FluentMigrator.SqlServer;
using VSGMarketplaceApi.Data.Models;
using VSGMarketplaceApi.DTOs;

namespace VSGMarketplaceApi.Data.Migrations
{
    [Migration(202304280003)]
    public class UserSeed_202304280003 : Migration
    {
        public override void Down()
        {
            Delete.FromTable("Users")
                .Row(new User
                {
                    Id = 2,
                    Email = "SVanev@vsgbg.com",
                    Password = "123456",
                    Role = "Admin"
                });
            Delete.FromTable("Users")
                .Row(new User
                {
                    Id = 1,
                    Email = "SNikolov@vsgbg.com",
                    Password = "123456",
                    Role = "User"
                });
        }
        public override void Up()
        {
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
        }

    }
}
