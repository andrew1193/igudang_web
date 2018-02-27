namespace Nop.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201801121251_AddColEventBgPicId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Event", "BackgroundPictureId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Event", "BackgroundPictureId");
        }
    }
}
