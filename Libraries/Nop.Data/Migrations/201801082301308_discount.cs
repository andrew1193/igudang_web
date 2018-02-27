namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class discount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Discount", "OrderAbove", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Discount", "LimitationTimesPerCustomer", c => c.Int(nullable: false));
            AddColumn("dbo.Discount", "LimitationTimesPerTotal", c => c.Int(nullable: false));
            //DropColumn("dbo.Product", "IsBestSeller");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Product", "IsBestSeller", c => c.Boolean(nullable: false));
            DropColumn("dbo.Discount", "LimitationTimesPerTotal");
            DropColumn("dbo.Discount", "LimitationTimesPerCustomer");
            DropColumn("dbo.Discount", "OrderAbove");
        }
    }
}
