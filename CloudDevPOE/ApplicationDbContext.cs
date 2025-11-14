using Microsoft.EntityFrameworkCore;
using CloudDevPOE.Models;

namespace CloudDevPOE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties represent tables in the database
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId); // Primary key
                entity.HasIndex(e => e.Email).IsUnique(); // Email must be unique
                entity.HasIndex(e => e.Username).IsUnique(); // Username must be unique
                entity.Property(e => e.Role).HasMaxLength(50); // Limit Role string to 50 chars
            });

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId); // Primary key
                entity.HasOne(e => e.User) // One-to-one relationship with User
                      .WithOne(u => u.Customer)
                      .HasForeignKey<Customer>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Deleting a User deletes the Customer
            });

            // Configure Cart entity
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.CartId); // Primary key
                entity.HasOne(e => e.Customer) // One-to-one relationship with Customer
                      .WithOne(c => c.Cart)
                      .HasForeignKey<Cart>(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade); // Deleting a Customer deletes the Cart
            });

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.CartItemId); // Primary key
                entity.HasOne(e => e.Cart) // Many CartItems belong to one Cart
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(e => e.CartId)
                      .OnDelete(DeleteBehavior.Cascade); // Deleting a Cart deletes its items
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId); // Primary key
                entity.HasIndex(e => e.OrderNumber).IsUnique(); // OrderNumber must be unique
                entity.HasOne(e => e.Customer) // Many Orders belong to one Customer
                      .WithMany(c => c.Orders)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if orders exist
                entity.HasOne(e => e.ProcessedByUser) // Optional reference to User who processed the order
                      .WithMany()
                      .HasForeignKey(e => e.ProcessedBy)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if user processed orders
                entity.Property(e => e.Status).HasMaxLength(50); // Limit Status string to 50 chars
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.OrderItemId); // Primary key
                entity.HasOne(e => e.Order) // Many OrderItems belong to one Order
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade); // Deleting an Order deletes its items
            });
        }
    }
}
