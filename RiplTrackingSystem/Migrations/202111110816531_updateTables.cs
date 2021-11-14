namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateTables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyAssetRentHistories", "working_date", c => c.DateTime());
            AddColumn("dbo.CompanyAssetRents", "working_date", c => c.DateTime());
            AddColumn("dbo.Transcactions", "working_date", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Transcactions", "working_date");
            DropColumn("dbo.CompanyAssetRents", "working_date");
            DropColumn("dbo.CompanyAssetRentHistories", "working_date");
        }
    }
}
