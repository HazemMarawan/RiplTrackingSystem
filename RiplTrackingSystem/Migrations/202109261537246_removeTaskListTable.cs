namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeTaskListTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tasks", "task_list_id", "dbo.TaskLists");
            DropIndex("dbo.Tasks", new[] { "task_list_id" });
            DropColumn("dbo.Tasks", "task_list_id");
            DropTable("dbo.TaskLists");
        }
        
        public override void Down()
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
            
            AddColumn("dbo.Tasks", "task_list_id", c => c.Int());
            CreateIndex("dbo.Tasks", "task_list_id");
            AddForeignKey("dbo.Tasks", "task_list_id", "dbo.TaskLists", "id");
        }
    }
}
