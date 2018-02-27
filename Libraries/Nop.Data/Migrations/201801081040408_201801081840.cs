namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201801081840 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "IsBestSeller", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "IsBestSeller");
        }
    }
}
