namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rentOrder : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CompanyAssetRents", "location_id", "dbo.Locations");
            DropIndex("dbo.CompanyAssetRents", new[] { "location_id" });
            CreateTable(
                "dbo.RentOrders",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        assetes_count = c.Int(nullable: false),
                        status = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        start_date = c.DateTime(),
                        due_date = c.DateTime(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        location_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Locations", t => t.location_id)
                .Index(t => t.location_id);
            
            AddColumn("dbo.CompanyAssetRents", "start_date", c => c.DateTime());
            AddColumn("dbo.CompanyAssetRents", "rent_order_id", c => c.Int());
            CreateIndex("dbo.CompanyAssetRents", "rent_order_id");
            AddForeignKey("dbo.CompanyAssetRents", "rent_order_id", "dbo.RentOrders", "id");
            DropColumn("dbo.CompanyAssetRents", "start_data");
            DropColumn("dbo.CompanyAssetRents", "location_id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CompanyAssetRents", "location_id", c => c.Int());
            AddColumn("dbo.CompanyAssetRents", "start_data", c => c.DateTime());
            DropForeignKey("dbo.CompanyAssetRents", "rent_order_id", "dbo.RentOrders");
            DropForeignKey("dbo.RentOrders", "location_id", "dbo.Locations");
            DropIndex("dbo.RentOrders", new[] { "location_id" });
            DropIndex("dbo.CompanyAssetRents", new[] { "rent_order_id" });
            DropColumn("dbo.CompanyAssetRents", "rent_order_id");
            DropColumn("dbo.CompanyAssetRents", "start_date");
            DropTable("dbo.RentOrders");
            CreateIndex("dbo.CompanyAssetRents", "location_id");
            AddForeignKey("dbo.CompanyAssetRents", "location_id", "dbo.Locations", "id");
        }
    }
}
