namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addColumnsFK : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Transcactions", "recieved_by", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Transcactions", "recieved_by", c => c.Int(nullable: false));
        }
    }
}
