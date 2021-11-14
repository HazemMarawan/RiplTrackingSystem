namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateLogs3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Logs", "user_id", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Logs", "user_id", c => c.Int(nullable: false));
        }
    }
}
