using Entities.EntityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class StripeIntegrationContext : IdentityDbContext<ApplicationUser>
    {
        public StripeIntegrationContext(DbContextOptions<StripeIntegrationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StripeTransactionHistory>()
                .Property(c => c.Id)
                .HasDefaultValueSql("LOWER(NEWID())");

            builder
            .Entity<ApplicationUser>()
            .HasMany(u => u.StripeTransactionHistories)
            .WithOne(au => au.User)
            .HasForeignKey(au => au.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            this.SeedRoles(builder);
            this.SeedUsers(builder);
            this.SeedUserRoles(builder);

        }

        private void SeedUsers(ModelBuilder builder)
        {
            ApplicationUser user = new ApplicationUser()
            {
                Id = "a74ddd14-6340-4840-95c2-db12554843e5",
                UserName = "Admin",
                Email = "admin@gmail.com",
                LockoutEnabled = false,
            };

            PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, "admin123");

            builder.Entity<ApplicationUser>().HasData(user);
        }

        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Id = "fab4fac1-c546-41de-aebc-a14da6895711", Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" }
                );
        }

        private void SeedUserRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>() { RoleId = "fab4fac1-c546-41de-aebc-a14da6895711", UserId = "a74ddd14-6340-4840-95c2-db12554843e5" }
                );
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<StripeTransactionHistory> Transactions { get; set; }
    }
}
