namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class locationpluck : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "can_send_pluck", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Locations", "can_send_pluck");
        }
    }
}
