using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Models;
using System;

namespace RubenClothingStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ClothingItem> ClothingItems { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed User first
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Ruben Loganathan",
                Email = "ruben04@gmail.com",
                Password = "12345", 
                IsAdmin = true
            });

            // seed UserProfile
            modelBuilder.Entity<UserProfile>().HasData(new UserProfile
            {   
                Id = 1,
                UserId = 1, // FK to Users.Id
                FullName = "Ruben Loganathan",
                PhoneNumber = "98765432",
                Address = "123 Fashion Ave, Singapore",
                DateOfBirth = new DateTime(2000, 1, 1)
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
