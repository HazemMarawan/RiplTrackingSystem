namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateRoleModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Roles", "description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Roles", "description");
        }
    }
}
