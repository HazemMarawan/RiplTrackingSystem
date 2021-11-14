namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTranscationsAndLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TransactionFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        path = c.String(),
                        transcaction_id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Transcactions", t => t.transcaction_id)
                .Index(t => t.transcaction_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransactionFiles", "transcaction_id", "dbo.Transcactions");
            DropIndex("dbo.TransactionFiles", new[] { "transcaction_id" });
            DropTable("dbo.TransactionFiles");
        }
    }
}
