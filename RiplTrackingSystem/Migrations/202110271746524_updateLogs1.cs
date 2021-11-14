namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateLogs1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Logs", "action", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Logs", "action", c => c.Int(nullable: false));
        }
    }
}
