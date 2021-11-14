namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateLogs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logs", "notes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Logs", "notes");
        }
    }
}
