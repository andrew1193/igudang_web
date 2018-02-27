namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNinjaVanStatusdToShipment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Shipment", "NinjaVanStatusId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Shipment", "NinjaVanStatusId");
        }
    }
}
