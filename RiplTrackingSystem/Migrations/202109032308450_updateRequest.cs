namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "number_of_assets", c => c.Int(nullable: false));
            DropColumn("dbo.Requests", "number_of_asstes");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Requests", "number_of_asstes", c => c.Int(nullable: false));
            DropColumn("dbo.Requests", "number_of_assets");
        }
    }
}
