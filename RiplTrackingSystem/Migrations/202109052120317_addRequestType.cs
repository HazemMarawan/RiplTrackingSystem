namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRequestType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "type");
        }
    }
}
