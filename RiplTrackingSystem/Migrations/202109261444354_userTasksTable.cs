namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userTasksTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserTasks",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(),
                        task_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Tasks", t => t.task_id)
                .ForeignKey("dbo.Users", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.task_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserTasks", "user_id", "dbo.Users");
            DropForeignKey("dbo.UserTasks", "task_id", "dbo.Tasks");
            DropIndex("dbo.UserTasks", new[] { "task_id" });
            DropIndex("dbo.UserTasks", new[] { "user_id" });
            DropTable("dbo.UserTasks");
        }
    }
}
