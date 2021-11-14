namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfrom_to_update : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CompanyAssetRents", "from_location", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CompanyAssetRents", "from_location", c => c.Int(nullable: false));
        }
    }
}
