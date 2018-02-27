namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201801262012 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Shipment", "ShipperId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Shipment", "ShipperId");
        }
    }
}
