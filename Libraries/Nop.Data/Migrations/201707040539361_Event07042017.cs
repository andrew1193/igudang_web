namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Event07042017 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Event", "PictureId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Event", "PictureId", c => c.Int(nullable: false));
        }
    }
}
