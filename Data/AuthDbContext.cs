using System.Reflection.Emit;
using CycleAPI.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CycleAPI.Data
{
    public class AuthDbContext :DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.Address).HasMaxLength(200);
                entity.Property(b => b.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
                entity.Property(b => b.UpdatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.RoleName).IsRequired().HasMaxLength(100);
            });

            // Create reader and Writer Roles
            var EmployeeRoleId = "51927f62-814b-4e0f-85f5-63859d5771b9";
            var AdminRoleId = "572a2e6b-0de5-4648-8daf-139bd9a9be44";
            var UserRoleId = "d6c3a159-c934-41f8-b5a1-17032f736b87";
            var roles = new List<Role>
            {
                new Role()
                {
                    RoleId = Guid.Parse(EmployeeRoleId),
                    RoleName ="Employee",
                    
                },
                new Role()
                {
                    RoleId = Guid.Parse(AdminRoleId),
                    RoleName ="Admin",
                },
                new Role()
                {
                    RoleId = Guid.Parse(UserRoleId),
                    RoleName ="User",
                }
            };

            //Seed the roles
            builder.Entity<Role>().HasData(roles);


            //Create an Admin User
            var hashedPassword = "$2a$11$YQWxuDh3HRftSDCHmfBpEu05XLTPbnvGnlfnkgTfRZ8DpxMsldEkC";
            //string hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            var adminUserId = "61aa79a1-539b-4f68-905b-4856cf0a6bbe";
            var fixedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var admin = new User
            {
                UserId = Guid.Parse(adminUserId),
                Username = "admin@me.com",
                Email = "admin@me.com",
                PasswordHash = hashedPassword,
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "1234567890", // Added missing required field
                Address = "Admin Address", // Added missing required field
                RoleId = Guid.Parse(AdminRoleId),
                IsActive = true,
                CreatedAt = fixedDate,
                UpdatedAt = fixedDate
            };

            builder.Entity<User>().HasData(admin);

        }
    }
}
