namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tasksTables2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaskLists",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        title = c.String(),
                        description = c.String(),
                        created_by = c.Int(nullable: false),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        title = c.String(),
                        description = c.String(),
                        created_by = c.Int(nullable: false),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        task_list_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.TaskLists", t => t.task_list_id)
                .Index(t => t.task_list_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tasks", "task_list_id", "dbo.TaskLists");
            DropIndex("dbo.Tasks", new[] { "task_list_id" });
            DropTable("dbo.Tasks");
            DropTable("dbo.TaskLists");
        }
    }
}
