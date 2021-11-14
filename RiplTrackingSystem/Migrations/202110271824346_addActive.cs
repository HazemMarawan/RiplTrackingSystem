namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addActive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Assets", "active", c => c.Int());
            AddColumn("dbo.RentOrders", "active", c => c.Int());
            AddColumn("dbo.Transcactions", "active", c => c.Int());
            AddColumn("dbo.Locations", "active", c => c.Int());
            AddColumn("dbo.Roles", "active", c => c.Int());
            AddColumn("dbo.Permissions", "active", c => c.Int());
            AddColumn("dbo.PermissionGroups", "active", c => c.Int());
            AddColumn("dbo.Tasks", "active", c => c.Int());
            AddColumn("dbo.Emails", "active", c => c.Int());
            AddColumn("dbo.Notes", "active", c => c.Int());
            AddColumn("dbo.Requests", "active", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "active");
            DropColumn("dbo.Notes", "active");
            DropColumn("dbo.Emails", "active");
            DropColumn("dbo.Tasks", "active");
            DropColumn("dbo.PermissionGroups", "active");
            DropColumn("dbo.Permissions", "active");
            DropColumn("dbo.Roles", "active");
            DropColumn("dbo.Locations", "active");
            DropColumn("dbo.Transcactions", "active");
            DropColumn("dbo.RentOrders", "active");
            DropColumn("dbo.Assets", "active");
        }
    }
}
