namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class locationpluckupdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "can_send_pluck", c => c.Int());
            DropColumn("dbo.Locations", "can_select_pluck");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Locations", "can_select_pluck", c => c.Int());
            DropColumn("dbo.Locations", "can_send_pluck");
        }
    }
}
