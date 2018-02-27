namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201801151046_AddColShipperId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vendor", "ShipperId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Vendor", "ShipperId");
        }
    }
}
