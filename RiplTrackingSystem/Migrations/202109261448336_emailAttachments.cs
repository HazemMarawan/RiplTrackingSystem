namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emailAttachments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmailAttachments",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        attachmentPath = c.String(),
                        email_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Emails", t => t.email_id)
                .Index(t => t.email_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmailAttachments", "email_id", "dbo.Emails");
            DropIndex("dbo.EmailAttachments", new[] { "email_id" });
            DropTable("dbo.EmailAttachments");
        }
    }
}
