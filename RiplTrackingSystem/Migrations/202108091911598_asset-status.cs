namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class assetstatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Assets", "status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Assets", "status");
        }
    }
}
