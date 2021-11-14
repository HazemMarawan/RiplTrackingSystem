namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rename_table_PermissionGroup : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PermisisonGroups", newName: "PermissionGroups");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.PermissionGroups", newName: "PermisisonGroups");
        }
    }
}
