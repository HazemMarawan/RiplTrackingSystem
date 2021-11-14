namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateLogs2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logs", "request_data", c => c.String());
            DropColumn("dbo.Logs", "request_date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Logs", "request_date", c => c.String());
            DropColumn("dbo.Logs", "request_data");
        }
    }
}
