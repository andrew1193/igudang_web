namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerPicture : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customer_Picture_Mapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        PictureId = c.Int(nullable: false),
                        Published = c.Boolean(nullable: false),
                        UploadDateTimeUtc = c.DateTime(),
                        PublishDateTimeUtc = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Picture", t => t.PictureId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.PictureId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Customer_Picture_Mapping", "PictureId", "dbo.Picture");
            DropForeignKey("dbo.Customer_Picture_Mapping", "CustomerId", "dbo.Customer");
            DropIndex("dbo.Customer_Picture_Mapping", new[] { "PictureId" });
            DropIndex("dbo.Customer_Picture_Mapping", new[] { "CustomerId" });
            DropTable("dbo.Customer_Picture_Mapping");
        }
    }
}
