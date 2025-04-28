using CycleAPI.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CycleAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Brand> Brands { get; set; }

        public DbSet<CycleType> CycleTypes { get; set; }

        public DbSet<Cycle> Cycles { get; set; }

        public DbSet<StockMovement> StockMovement { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<CartActivityLog> CartActivityLogs { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<SalesAnalytics> SalesAnalytics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CycleType>(entity =>
            {
                entity.HasKey(e => e.TypeId);

                entity.Property(e => e.TypeId)
                      .IsRequired();

                entity.Property(e => e.TypeName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(b => b.BrandId);

                entity.Property(b => b.BrandId)
                      .IsRequired();

                entity.Property(b => b.BrandName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(b => b.Description)
                      .HasMaxLength(500);

                entity.Property(b => b.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Cycle>(entity =>
            {
                entity.ToTable("cycles");

                entity.HasKey(c => c.CycleId);

                entity.Property(c => c.ModelName)
                      .IsRequired()
                      .HasMaxLength(255); // You can adjust as needed

                entity.Property(c => c.Description)
                      .HasColumnType("text");

                entity.Property(c => c.Price)
                      .IsRequired()
                      .HasColumnType("decimal(10,2)");

                entity.Property(c => c.CostPrice)
                      .IsRequired()
                      .HasColumnType("decimal(10,2)");

                entity.Property(c => c.StockQuantity)
                      .IsRequired()
                      .HasDefaultValue(0);

                entity.Property(c => c.ReorderLevel)
                      .IsRequired()
                      .HasDefaultValue(5);

                entity.Property(c => c.ImageUrl)
                      .HasMaxLength(2048); // Reasonable default for URL

                entity.Property(c => c.IsActive)
                      .HasDefaultValue(true);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(c => c.UpdatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(c => c.Brand)
                      .WithMany()
                      .HasForeignKey(c => c.BrandId)
                      .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(c => c.CycleType)
                      .WithMany()
                      .HasForeignKey(c => c.TypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.ToTable("stock_movements");

                entity.HasKey(sm => sm.MovementId);

                entity.Property(sm => sm.CycleId)
                      .IsRequired();

                entity.Property(sm => sm.Quantity)
                      .IsRequired();

                entity.Property(sm => sm.MovementType)
                       .HasConversion<string>() // Store as text in DB
                       .IsRequired()
                       .HasMaxLength(20);

                entity.Property(sm => sm.ReferenceId)
                      .IsRequired(false); // Nullable

                entity.Property(sm => sm.UserId)
                      .IsRequired();

                entity.Property(sm => sm.Notes)
                      .HasColumnType("text");

                entity.Property(sm => sm.MovementDate)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationships
                entity.HasOne(sm => sm.Cycle)
                      .WithMany()
                      .HasForeignKey(sm => sm.CycleId)
                      .OnDelete(DeleteBehavior.Restrict);

                
                entity.HasOne(sm => sm.User)
                      .WithMany()
                      .HasForeignKey(sm => sm.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customers");

                entity.HasKey(c => c.CustomerId);

                entity.Property(c => c.FirstName)
                      .IsRequired();

                entity.Property(c => c.LastName)
                      .IsRequired();

                entity.Property(c => c.Email)
                      .HasMaxLength(150);

                entity.HasIndex(c => c.Email)
                      .IsUnique();

                entity.Property(c => c.Phone)
                      .HasMaxLength(20);

                entity.Property(c => c.Address);
                entity.Property(c => c.City)
                      .HasMaxLength(100);

                entity.Property(c => c.State)
                      .HasMaxLength(100);

                entity.Property(c => c.PostalCode)
                      .HasMaxLength(20);

                entity.Property(c => c.RegistrationDate)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(c => c.UpdatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasMany(c => c.Carts)
                      .WithOne(cart => cart.Customer)
                      .HasForeignKey(cart => cart.CustomerId);
            });

            // ───── Carts ─────
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("carts");

                entity.HasKey(c => c.CartId);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(c => c.UpdatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(c => c.IsActive)
                      .HasDefaultValue(true);

                entity.Property(c => c.SessionId)
                      .HasMaxLength(200);

                entity.Property(c => c.Notes)
                      .IsRequired(false)
                      .HasDefaultValue("");

                entity.Property(c => c.LastAccessedAt);

                entity.HasOne(c => c.LastAccessedByUser)
                      .WithMany()
                      .HasForeignKey(c => c.LastAccessedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.CartItems)
                      .WithOne(i => i.Cart)
                      .HasForeignKey(i => i.CartId);
            });

            // ───── CartItems ─────
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("cart_items");

                entity.HasKey(ci => ci.CartItemId);

                entity.Property(ci => ci.Quantity)
                      .HasDefaultValue(1)
                      .IsRequired();

                entity.Property(ci => ci.AddedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(ci => ci.UpdatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(ci => ci.Cycle)
                      .WithMany()
                      .HasForeignKey(ci => ci.CycleId);

                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(ci => ci.CartId);
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");

                entity.HasKey(e => e.RoleId);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(100);

            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);
                
                entity.Property(e => e.Address)
                    .HasMaxLength(200);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CartActivityLog configuration
            modelBuilder.Entity<CartActivityLog>(entity =>
            {
                entity.ToTable("cart_activity_logs");

                entity.HasKey(e => e.LogId);

                entity.Property(e => e.LogId)
                    .IsRequired();

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(50);

                entity.Property(e => e.Notes)
                    .HasColumnType("text");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationships
                entity.HasOne(e => e.Cart)
                    .WithMany()
                    .HasForeignKey(e => e.CartId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Cycle)
                    .WithMany()
                    .HasForeignKey(e => e.CycleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");

                entity.HasKey(e => e.OrderId);

                entity.Property(e => e.OrderNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.TotalAmount)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.ShippingAddress)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ShippingCity)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ShippingState)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ShippingPostalCode)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Notes)
                    .HasColumnType("text");

                entity.Property(e => e.OrderDate)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationships
                entity.HasOne(o => o.Customer)
                    .WithMany()
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.ProcessedByUser)
                    .WithMany()
                    .HasForeignKey(o => o.ProcessedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");

                entity.HasKey(e => e.OrderItemId);

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.UnitPrice)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Subtotal)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Notes)
                    .HasColumnType("text");

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationships
                entity.HasOne(oi => oi.Cycle)
                    .WithMany()
                    .HasForeignKey(oi => oi.CycleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");

                entity.HasKey(e => e.PaymentId);

                entity.Property(e => e.RazorpayOrderId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.RazorpayPaymentId)
                    .HasMaxLength(100);

                entity.Property(e => e.RazorpaySignature)
                    .HasMaxLength(200);

                entity.Property(e => e.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(p => p.Order)
                    .WithMany()
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SalesAnalytics>(entity =>
            {
                entity.ToTable("sales_analytics");
                
                entity.HasKey(e => e.AnalyticsId);
                
                entity.Property(e => e.Date)
                    .IsRequired();
                
                entity.Property(e => e.DailyRevenue)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.MonthlyRevenue)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.YearlyRevenue)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.GrossProfit)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.NetProfit)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.ProfitMargin)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();
                
                entity.Property(e => e.AverageOrderValue)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();
                
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .IsRequired();

                entity.HasOne(e => e.TopSellingCycle)
                    .WithMany()
                    .HasForeignKey(e => e.TopSellingCycleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TopSellingBrand)
                    .WithMany()
                    .HasForeignKey(e => e.TopSellingBrandId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
