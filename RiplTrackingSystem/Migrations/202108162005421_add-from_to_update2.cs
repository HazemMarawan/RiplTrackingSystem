namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfrom_to_update2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CompanyAssetRents", "to_location", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CompanyAssetRents", "to_location", c => c.Int(nullable: false));
        }
    }
}
