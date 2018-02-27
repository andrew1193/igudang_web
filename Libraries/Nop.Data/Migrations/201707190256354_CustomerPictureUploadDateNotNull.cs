namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerPictureUploadDateNotNull : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customer_Picture_Mapping", "UploadDateTimeUtc", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customer_Picture_Mapping", "UploadDateTimeUtc", c => c.DateTime());
        }
    }
}
