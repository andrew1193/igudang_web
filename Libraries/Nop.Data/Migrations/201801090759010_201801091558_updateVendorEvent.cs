namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201801091558_updateVendorEvent : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Vendor_Event_Mapping", "Vendor_Id", "dbo.Vendor");
            DropForeignKey("dbo.Vendor_Event_Mapping", "VendorId", "dbo.Vendor");
            DropIndex("dbo.Vendor_Event_Mapping", new[] { "VendorId" });
            DropIndex("dbo.Vendor_Event_Mapping", new[] { "Vendor_Id" });
            DropColumn("dbo.Vendor_Event_Mapping", "Vendor_Id");
            DropColumn("dbo.Vendor_Event_Mapping", "VendorId");
            //RenameColumn(table: "dbo.Vendor_Event_Mapping", name: "Vendor_Id", newName: "VendorId");
            //AlterColumn("dbo.Vendor_Event_Mapping", "VendorId", c => c.Int(nullable: false));
            AddColumn("dbo.Vendor_Event_Mapping", "VendorId", c => c.Int(nullable: false));
            CreateIndex("dbo.Vendor_Event_Mapping", "VendorId");
            AddForeignKey("dbo.Vendor_Event_Mapping", "VendorId", "dbo.Vendor", "Id", cascadeDelete: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Vendor_Event_Mapping", "VendorId", "dbo.Vendor");
            DropIndex("dbo.Vendor_Event_Mapping", new[] { "VendorId" });
            AlterColumn("dbo.Vendor_Event_Mapping", "VendorId", c => c.Int());
            RenameColumn(table: "dbo.Vendor_Event_Mapping", name: "VendorId", newName: "Vendor_Id");
            AddColumn("dbo.Vendor_Event_Mapping", "VendorId", c => c.Int(nullable: false));
            CreateIndex("dbo.Vendor_Event_Mapping", "Vendor_Id");
            CreateIndex("dbo.Vendor_Event_Mapping", "VendorId");
            AddForeignKey("dbo.Vendor_Event_Mapping", "Vendor_Id", "dbo.Vendor", "Id");
        }
    }
}
