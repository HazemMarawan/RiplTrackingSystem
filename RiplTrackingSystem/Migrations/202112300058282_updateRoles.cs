namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateRoles : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Roles", "is_ripl", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Roles", "is_ripl");
        }
    }
}
