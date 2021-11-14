namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateRelation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Permissions", "nice_name", c => c.String());
            AddColumn("dbo.Permissions", "description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Permissions", "description");
            DropColumn("dbo.Permissions", "nice_name");
        }
    }
}
