namespace RiplTrackingSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class createDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Assets",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        tag_id = c.String(),
                        type = c.String(),
                        tall = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
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
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Companies", t => t.company_id)
                .ForeignKey("dbo.Stores", t => t.store_id)
                .Index(t => t.store_id)
                .Index(t => t.company_id);
            
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
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Companies", t => t.company_id)
                .ForeignKey("dbo.Factories", t => t.factory_id)
                .Index(t => t.company_id)
                .Index(t => t.factory_id);
            
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
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Companies", t => t.company_id)
                .Index(t => t.company_id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        code = c.String(),
                        user_name = c.String(),
                        full_name = c.String(),
                        email = c.String(),
                        password = c.String(),
                        phone1 = c.String(),
                        phone2 = c.String(),
                        address1 = c.String(),
                        address2 = c.String(),
                        gender = c.Int(nullable: false),
                        nationality = c.String(),
                        gym_code = c.String(),
                        birthDate = c.DateTime(storeType: "date"),
                        image = c.String(),
                        type = c.Int(nullable: false),
                        active = c.Int(),
                        created_by = c.Int(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        company_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Companies", t => t.company_id)
                .Index(t => t.company_id);
            
            CreateTable(
                "dbo.CompanyAssetRents",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        start_data = c.DateTime(),
                        due_date = c.DateTime(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        company_id = c.Int(),
                        asset_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Assets", t => t.asset_id)
                .ForeignKey("dbo.Companies", t => t.company_id)
                .Index(t => t.company_id)
                .Index(t => t.asset_id);
            
            CreateTable(
                "dbo.Permissions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.RolePermissions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        permission_id = c.Int(),
                        role_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Permissions", t => t.permission_id)
                .ForeignKey("dbo.Roles", t => t.role_id)
                .Index(t => t.permission_id)
                .Index(t => t.role_id);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        created_at = c.DateTime(),
                        updated_at = c.DateTime(),
                        user_id = c.Int(),
                        role_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Roles", t => t.role_id)
                .ForeignKey("dbo.Users", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.role_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRoles", "user_id", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "role_id", "dbo.Roles");
            DropForeignKey("dbo.RolePermissions", "role_id", "dbo.Roles");
            DropForeignKey("dbo.RolePermissions", "permission_id", "dbo.Permissions");
            DropForeignKey("dbo.CompanyAssetRents", "company_id", "dbo.Companies");
            DropForeignKey("dbo.CompanyAssetRents", "asset_id", "dbo.Assets");
            DropForeignKey("dbo.Users", "company_id", "dbo.Companies");
            DropForeignKey("dbo.Distributors", "store_id", "dbo.Stores");
            DropForeignKey("dbo.Stores", "factory_id", "dbo.Factories");
            DropForeignKey("dbo.Factories", "company_id", "dbo.Companies");
            DropForeignKey("dbo.Stores", "company_id", "dbo.Companies");
            DropForeignKey("dbo.Distributors", "company_id", "dbo.Companies");
            DropIndex("dbo.UserRoles", new[] { "role_id" });
            DropIndex("dbo.UserRoles", new[] { "user_id" });
            DropIndex("dbo.RolePermissions", new[] { "role_id" });
            DropIndex("dbo.RolePermissions", new[] { "permission_id" });
            DropIndex("dbo.CompanyAssetRents", new[] { "asset_id" });
            DropIndex("dbo.CompanyAssetRents", new[] { "company_id" });
            DropIndex("dbo.Users", new[] { "company_id" });
            DropIndex("dbo.Factories", new[] { "company_id" });
            DropIndex("dbo.Stores", new[] { "factory_id" });
            DropIndex("dbo.Stores", new[] { "company_id" });
            DropIndex("dbo.Distributors", new[] { "company_id" });
            DropIndex("dbo.Distributors", new[] { "store_id" });
            DropTable("dbo.UserRoles");
            DropTable("dbo.Roles");
            DropTable("dbo.RolePermissions");
            DropTable("dbo.Permissions");
            DropTable("dbo.CompanyAssetRents");
            DropTable("dbo.Users");
            DropTable("dbo.Factories");
            DropTable("dbo.Stores");
            DropTable("dbo.Distributors");
            DropTable("dbo.Companies");
            DropTable("dbo.Assets");
        }
    }
}
