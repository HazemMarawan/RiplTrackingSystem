    using System;
using System.Data.Entity;
using System.Linq;

namespace RiplTrackingSystem.Models
{
    public class DBContext : DbContext
    {
        public DBContext()
            : base("name=DbContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
        public DbSet<User> users { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<Permission> permissions { get; set; }
        public DbSet<UserRole> userRoles { get; set; }
        public DbSet<RolePermission> rolePermissions { get; set; }
        public DbSet<Asset> assets { get; set; }
        public DbSet<CompanyAssetRent> companyAssetsRent { get; set; }
        public DbSet<Location> locations { get; set; }
        public DbSet<PermissionGroup> permisisonGroups { get; set; }
        public DbSet<RentOrder> rentOrders { get; set; }
        public DbSet<Request> requests { get; set; }
        public DbSet<Transcaction> transcactions { get; set; }
        public DbSet<CompanyAssetRentHistory> companyAssetRentHistories { get; set; }
        public DbSet<Email> emails { get; set; }
        public DbSet<EmailAttachment> emailAttachments { get; set; }
        public DbSet<Note> notes { get; set; }
        public DbSet<Task> tasks { get; set; }
        public DbSet<UserTask> userTasks { get; set; }
        public DbSet<Log> logs { get; set; }
        public DbSet<TransactionFile> transactionFiles { get; set; }
        public DbSet<LocationAttachment> locationAttachments { get; set; }


    }


}