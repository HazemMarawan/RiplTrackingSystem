namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Assets", "color", c => c.String());
            AddColumn("dbo.Transcactions", "recieved_by", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Transcactions", "recieved_by");
            DropColumn("dbo.Assets", "color");
        }
    }
}
