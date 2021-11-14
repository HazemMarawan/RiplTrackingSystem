namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateAttachmentTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LocationAttachments", "description", c => c.String());
            AddColumn("dbo.LocationAttachments", "active", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LocationAttachments", "active");
            DropColumn("dbo.LocationAttachments", "description");
        }
    }
}
