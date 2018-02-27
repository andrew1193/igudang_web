namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _21801101151_AddColMarkAsSellingFast : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "MarkAsSellingFast", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "MarkAsSellingFast");
        }
    }
}
