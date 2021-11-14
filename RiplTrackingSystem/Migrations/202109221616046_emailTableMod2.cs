namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emailTableMod2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Emails", "from_user", c => c.Int());
            AlterColumn("dbo.Emails", "to_user", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Emails", "to_user", c => c.Int(nullable: false));
            AlterColumn("dbo.Emails", "from_user", c => c.Int(nullable: false));
        }
    }
}
