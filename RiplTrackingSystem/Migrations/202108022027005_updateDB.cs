namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateDB : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Distributors", "company_id", "dbo.Companies");
            DropForeignKey("dbo.Stores", "company_id", "dbo.Companies");
            DropForeignKey("dbo.Factories", "company_id", "dbo.Companies");
            DropForeignKey("dbo.Stores", "factory_id", "dbo.Factories");
            DropForeignKey("dbo.Distributors", "store_id", "dbo.Stores");
            DropForeignKey("dbo.Users", "company_id", "dbo.Companies");
            DropForeignKey("dbo.CompanyAssetRents", "company_id", "dbo.Companies");
            DropIndex("dbo.Distributors", new[] { "store_id" });
            DropIndex("dbo.Distributors", new[] { "company_id" });
            DropIndex("dbo.Stores", new[] { "company_id" });
            DropIndex("dbo.Stores", new[] { "factory_id" });
            DropIndex("dbo.Factories", new[] { "company_id" });
            DropIndex("dbo.Users", new[] { "company_id" });
            DropIndex("dbo.CompanyAssetRents", new[] { "company_id" });
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        description = c.String(),
                        address = c.String(),
                        phone = c.String(),
                        parent_id = c.Int(),
                        type = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.Assets", "created_by", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "location_id", c => c.Int());
            AddColumn("dbo.CompanyAssetRents", "status", c => c.Int(nullable: false));
            AddColumn("dbo.CompanyAssetRents", "created_by", c => c.Int(nullable: false));
            AddColumn("dbo.CompanyAssetRents", "location_id", c => c.Int());
            CreateIndex("dbo.CompanyAssetRents", "location_id");
            CreateIndex("dbo.Users", "location_id");
            AddForeignKey("dbo.Users", "location_id", "dbo.Locations", "id");
            AddForeignKey("dbo.CompanyAssetRents", "location_id", "dbo.Locations", "id");
            DropColumn("dbo.Users", "company_id");
            DropColumn("dbo.CompanyAssetRents", "company_id");
            DropTable("dbo.Companies");
            DropTable("dbo.Distributors");
            DropTable("dbo.Stores");
            DropTable("dbo.Factories");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Factories",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        address = c.String(),
                        phone = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        company_id = c.Int(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Stores",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        address = c.String(),
                        phone = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        company_id = c.Int(),
                        factory_id = c.Int(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Distributors",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        address = c.String(),
                        phone = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        store_id = c.Int(),
                        company_id = c.Int(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        description = c.String(),
                        address = c.String(),
                        phone = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.CompanyAssetRents", "company_id", c => c.Int());
            AddColumn("dbo.Users", "company_id", c => c.Int());
            DropForeignKey("dbo.CompanyAssetRents", "location_id", "dbo.Locations");
            DropForeignKey("dbo.Users", "location_id", "dbo.Locations");
            DropIndex("dbo.Users", new[] { "location_id" });
            DropIndex("dbo.CompanyAssetRents", new[] { "location_id" });
            DropColumn("dbo.CompanyAssetRents", "location_id");
            DropColumn("dbo.CompanyAssetRents", "created_by");
            DropColumn("dbo.CompanyAssetRents", "status");
            DropColumn("dbo.Users", "location_id");
            DropColumn("dbo.Assets", "created_by");
            DropTable("dbo.Locations");
            CreateIndex("dbo.CompanyAssetRents", "company_id");
            CreateIndex("dbo.Users", "company_id");
            CreateIndex("dbo.Factories", "company_id");
            CreateIndex("dbo.Stores", "factory_id");
            CreateIndex("dbo.Stores", "company_id");
            CreateIndex("dbo.Distributors", "company_id");
            CreateIndex("dbo.Distributors", "store_id");
            AddForeignKey("dbo.CompanyAssetRents", "company_id", "dbo.Companies", "id");
            AddForeignKey("dbo.Users", "company_id", "dbo.Companies", "id");
            AddForeignKey("dbo.Distributors", "store_id", "dbo.Stores", "id");
            AddForeignKey("dbo.Stores", "factory_id", "dbo.Factories", "id");
            AddForeignKey("dbo.Factories", "company_id", "dbo.Companies", "id");
            AddForeignKey("dbo.Stores", "company_id", "dbo.Companies", "id");
            AddForeignKey("dbo.Distributors", "company_id", "dbo.Companies", "id");
        }
    }
}
