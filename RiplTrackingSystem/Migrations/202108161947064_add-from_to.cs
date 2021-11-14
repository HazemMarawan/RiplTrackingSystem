namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfrom_to : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyAssetRents", "from_location", c => c.Int(nullable: false));
            AddColumn("dbo.CompanyAssetRents", "to_location", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompanyAssetRents", "to_location");
            DropColumn("dbo.CompanyAssetRents", "from_location");
        }
    }
}
