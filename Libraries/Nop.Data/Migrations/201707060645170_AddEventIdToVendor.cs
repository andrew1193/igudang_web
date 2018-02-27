namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEventIdToVendor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vendor", "EventId", c => c.Int(nullable: false));
            DropColumn("dbo.Customer", "EventId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customer", "EventId", c => c.Int(nullable: false));
            DropColumn("dbo.Vendor", "EventId");
        }
    }
}
