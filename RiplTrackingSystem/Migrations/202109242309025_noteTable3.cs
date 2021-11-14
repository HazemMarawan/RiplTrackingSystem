namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class noteTable3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notes", "isFavourite", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notes", "isFavourite");
        }
    }
}
