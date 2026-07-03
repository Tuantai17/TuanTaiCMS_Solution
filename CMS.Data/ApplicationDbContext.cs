using CMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductReviewImage> ProductReviewImages { get; set; }
        public DbSet<ProductReviewReply> ProductReviewReplies { get; set; }
        public DbSet<OrderItemIssue> OrderItemIssues { get; set; }
        public DbSet<OrderActivityLog> OrderActivityLogs { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportTicketMessage> SupportTicketMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(t => t.TokenHash);
            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(t => t.CustomerId);

            modelBuilder.Entity<EmailLog>()
                .HasIndex(e => e.RecipientEmail);
            modelBuilder.Entity<EmailLog>()
                .HasIndex(e => e.ReferenceId);
            modelBuilder.Entity<EmailLog>()
                .HasIndex(e => new { e.EmailType, e.Status });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.TargetUserId);
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.IsRead, n.CreatedAt });

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.TransactionCode);

            modelBuilder.Entity<ProductFavorite>()
                .HasIndex(pf => new { pf.CustomerId, pf.ProductId })
                .IsUnique();

            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => r.OrderDetailId)
                .IsUnique();
            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => new { r.ProductId, r.Status, r.CreatedAt });
            modelBuilder.Entity<ProductReviewImage>()
                .HasIndex(i => new { i.ProductReviewId, i.DisplayOrder });
            modelBuilder.Entity<ProductReviewReply>()
                .HasIndex(r => new { r.ProductReviewId, r.CreatedAt });

            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Order)
                .WithMany(o => o.ProductReviews)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.OrderDetail)
                .WithOne(od => od.ProductReview)
                .HasForeignKey<ProductReview>(r => r.OrderDetailId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.ProductReviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductReviewImage>()
                .HasOne(i => i.ProductReview)
                .WithMany(r => r.Images)
                .HasForeignKey(i => i.ProductReviewId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductReviewReply>()
                .HasOne(r => r.ProductReview)
                .WithMany(pr => pr.Replies)
                .HasForeignKey(r => r.ProductReviewId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductReviewReply>()
                .HasOne(r => r.AdminUser)
                .WithMany(u => u.ProductReviewReplies)
                .HasForeignKey(r => r.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItemIssue>()
                .HasIndex(i => i.OrderId);
            modelBuilder.Entity<OrderItemIssue>()
                .HasIndex(i => i.OrderDetailId);
            modelBuilder.Entity<OrderItemIssue>()
                .HasIndex(i => i.Status);
            modelBuilder.Entity<OrderItemIssue>()
                .HasOne(i => i.Order)
                .WithMany()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItemIssue>()
                .HasOne(i => i.OrderDetail)
                .WithMany()
                .HasForeignKey(i => i.OrderDetailId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItemIssue>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderActivityLog>()
                .HasIndex(l => l.OrderId);

            modelBuilder.Entity<SupportTicket>()
                .HasIndex(t => t.Code)
                .IsUnique();
            modelBuilder.Entity<SupportTicket>()
                .HasIndex(t => t.CustomerId);
            modelBuilder.Entity<SupportTicket>()
                .HasIndex(t => new { t.Status, t.UpdatedAt });
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicketMessage>()
                .HasIndex(m => m.TicketId);
            modelBuilder.Entity<SupportTicketMessage>()
                .HasOne(m => m.Ticket)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
