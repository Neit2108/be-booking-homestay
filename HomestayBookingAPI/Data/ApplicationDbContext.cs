using HomestayBookingAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Place> Places { get; set; }
        public DbSet<TopRatePlaces> TopRatePlaces { get; set; }
        public DbSet<PlaceImage> PlaceImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PlaceAvailable> PlaceAvailables { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            //            foreach(var entityType in modelBuilder.Model.GetEntityTypes())
            //{
            //                Console.WriteLine($"EF is mapping: {entityType.Name} with CLR Type: {entityType.ClrType}");
            //            }
            base.OnModelCreating(modelBuilder);

            // Xóa ASPNET ở đầu tên bảng
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            // Xóa Place thì xóa cả PlaceImage
            modelBuilder.Entity<Place>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Place)
                .HasForeignKey(i => i.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

        }

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
        }
    }
}
