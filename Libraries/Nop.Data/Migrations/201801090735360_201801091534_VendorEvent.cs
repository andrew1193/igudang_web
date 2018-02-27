namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201801091534_VendorEvent : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Vendor_Event_Mapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VendorId = c.Int(nullable: false),
                        EventId = c.Int(nullable: false),
                        Vendor_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Event", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Vendor", t => t.VendorId, cascadeDelete: true)
                .ForeignKey("dbo.Vendor", t => t.Vendor_Id)
                .Index(t => t.VendorId)
                .Index(t => t.EventId)
                .Index(t => t.Vendor_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Vendor_Event_Mapping", "Vendor_Id", "dbo.Vendor");
            DropForeignKey("dbo.Vendor_Event_Mapping", "VendorId", "dbo.Vendor");
            DropForeignKey("dbo.Vendor_Event_Mapping", "EventId", "dbo.Event");
            DropIndex("dbo.Vendor_Event_Mapping", new[] { "Vendor_Id" });
            DropIndex("dbo.Vendor_Event_Mapping", new[] { "EventId" });
            DropIndex("dbo.Vendor_Event_Mapping", new[] { "VendorId" });
            DropTable("dbo.Vendor_Event_Mapping");
        }
    }
}
