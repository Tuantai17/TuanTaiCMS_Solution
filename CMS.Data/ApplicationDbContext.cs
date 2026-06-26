using Microsoft.EntityFrameworkCore;
using CMS.Data.Entities;

namespace CMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CategoryProduct> CategoriesProducts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<ProductFavorite> ProductFavorites { get; set; }

        // Entities ho tro Email, Notification, Password Reset
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Index cho PasswordResetToken
            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(t => t.TokenHash);
            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(t => t.CustomerId);

            // Index cho EmailLog
            modelBuilder.Entity<EmailLog>()
                .HasIndex(e => e.RecipientEmail);
            modelBuilder.Entity<EmailLog>()
                .HasIndex(e => e.ReferenceId);
            modelBuilder.Entity<EmailLog>()
                .HasIndex(e => new { e.EmailType, e.Status });

            // Index cho Notification
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.TargetUserId);
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.IsRead, n.CreatedAt });

            // Index cho Order
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.TransactionCode);

            // Index cho ProductFavorite
            modelBuilder.Entity<ProductFavorite>()
                .HasIndex(pf => new { pf.CustomerId, pf.ProductId })
                .IsUnique();
        }
    }
}

