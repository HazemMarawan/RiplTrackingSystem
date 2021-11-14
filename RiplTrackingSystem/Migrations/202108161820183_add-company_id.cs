namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcompany_id : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "company_id", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Locations", "company_id");
        }
    }
}
