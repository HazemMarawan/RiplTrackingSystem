namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAttachmentTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocationAttachments",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        path = c.String(),
                        location_id = c.Int(),
                        created_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Locations", t => t.location_id)
                .Index(t => t.location_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocationAttachments", "location_id", "dbo.Locations");
            DropIndex("dbo.LocationAttachments", new[] { "location_id" });
            DropTable("dbo.LocationAttachments");
        }
    }
}
