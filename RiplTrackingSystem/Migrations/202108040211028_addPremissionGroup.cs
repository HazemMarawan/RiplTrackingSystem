namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPremissionGroup : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PermisisonGroups",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        description = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.Permissions", "permission_group_id", c => c.Int());
            CreateIndex("dbo.Permissions", "permission_group_id");
            AddForeignKey("dbo.Permissions", "permission_group_id", "dbo.PermisisonGroups", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Permissions", "permission_group_id", "dbo.PermisisonGroups");
            DropIndex("dbo.Permissions", new[] { "permission_group_id" });
            DropColumn("dbo.Permissions", "permission_group_id");
            DropTable("dbo.PermisisonGroups");
        }
    }
}
