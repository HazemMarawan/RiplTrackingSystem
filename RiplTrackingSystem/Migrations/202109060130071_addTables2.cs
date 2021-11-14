namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTables2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Transcactions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        assetes_count = c.Int(nullable: false),
                        status = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        notes = c.String(),
                        from_location = c.Int(),
                        to_location = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.CompanyAssetRentHistories",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        status = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        from_location = c.Int(),
                        to_location = c.Int(),
                        start_date = c.DateTime(),
                        due_date = c.DateTime(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        asset_id = c.Int(),
                        rent_order_id = c.Int(),
                        transaction_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Assets", t => t.asset_id)
                .ForeignKey("dbo.RentOrders", t => t.rent_order_id)
                .ForeignKey("dbo.Transcactions", t => t.transaction_id)
                .Index(t => t.asset_id)
                .Index(t => t.rent_order_id)
                .Index(t => t.transaction_id);
            
            AddColumn("dbo.CompanyAssetRents", "transaction_id", c => c.Int());
            AddColumn("dbo.UserRoles", "created_by", c => c.Int(nullable: false));
            AddColumn("dbo.Roles", "created_by", c => c.Int(nullable: false));
            AddColumn("dbo.RolePermissions", "created_by", c => c.Int(nullable: false));
            AddColumn("dbo.Permissions", "created_by", c => c.Int(nullable: false));
            AddColumn("dbo.PermissionGroups", "created_by", c => c.Int(nullable: false));
            CreateIndex("dbo.CompanyAssetRents", "transaction_id");
            AddForeignKey("dbo.CompanyAssetRents", "transaction_id", "dbo.Transcactions", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CompanyAssetRents", "transaction_id", "dbo.Transcactions");
            DropForeignKey("dbo.CompanyAssetRentHistories", "transaction_id", "dbo.Transcactions");
            DropForeignKey("dbo.CompanyAssetRentHistories", "rent_order_id", "dbo.RentOrders");
            DropForeignKey("dbo.CompanyAssetRentHistories", "asset_id", "dbo.Assets");
            DropIndex("dbo.CompanyAssetRentHistories", new[] { "transaction_id" });
            DropIndex("dbo.CompanyAssetRentHistories", new[] { "rent_order_id" });
            DropIndex("dbo.CompanyAssetRentHistories", new[] { "asset_id" });
            DropIndex("dbo.CompanyAssetRents", new[] { "transaction_id" });
            DropColumn("dbo.PermissionGroups", "created_by");
            DropColumn("dbo.Permissions", "created_by");
            DropColumn("dbo.RolePermissions", "created_by");
            DropColumn("dbo.Roles", "created_by");
            DropColumn("dbo.UserRoles", "created_by");
            DropColumn("dbo.CompanyAssetRents", "transaction_id");
            DropTable("dbo.CompanyAssetRentHistories");
            DropTable("dbo.Transcactions");
        }
    }
}
