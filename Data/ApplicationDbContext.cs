using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Models;

namespace RubenClothingStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define tables (DbSets)
        public DbSet<User> Users { get; set; }
        public DbSet<ClothingItem> ClothingItems { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>().HasData(
                new UserProfile
                {
                    Id = 1,
                    UserId = "demo-user-id",    
                    FullName = "Ruben Loganathan",
                    PhoneNumber = "98765432",
                    Address = "123 Fashion Ave, Singapore",
                    DateOfBirth = new DateTime(2000, 1, 1)
                });

            base.OnModelCreating(modelBuilder);
        }

    }
}
