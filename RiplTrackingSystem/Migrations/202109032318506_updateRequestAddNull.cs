namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateRequestAddNull : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Requests", "location_id", "dbo.Locations");
            DropIndex("dbo.Requests", new[] { "location_id" });
            AlterColumn("dbo.Requests", "location_id", c => c.Int());
            CreateIndex("dbo.Requests", "location_id");
            AddForeignKey("dbo.Requests", "location_id", "dbo.Locations", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requests", "location_id", "dbo.Locations");
            DropIndex("dbo.Requests", new[] { "location_id" });
            AlterColumn("dbo.Requests", "location_id", c => c.Int(nullable: false));
            CreateIndex("dbo.Requests", "location_id");
            AddForeignKey("dbo.Requests", "location_id", "dbo.Locations", "id", cascadeDelete: true);
        }
    }
}
