using HomestayBookingAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HomestayBookingAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Place> Places { get; set; }
        public DbSet<TopRatePlaces> TopRatePlaces { get; set; }
        public DbSet<PlaceImage> PlaceImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PlaceAvailable> PlaceAvailables { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<TestCaseModel> TestCases { get; set; }
        public DbSet<Favourite> Favourites { get; set; } 
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentImage> CommentImages { get; set; }
        public DbSet<Contact> Contacts { get; set; } 
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Xóa Place thì xóa cả PlaceImage
            modelBuilder.Entity<Place>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Place)
                .HasForeignKey(i => i.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Code của voucher là duy nhât
            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique();

            // Ràng buộc cho bảng User
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.IdentityCard)
                .IsUnique();

            // Khóa chính cho Favourite
            modelBuilder.Entity<Favourite>()
                .HasKey(f => new { f.UserId, f.PlaceId });

            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favourites)
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.Place)
                .WithMany(p => p.Favourites)
                .HasForeignKey(f => f.PlaceId);

            modelBuilder.Entity<TopRatePlaces>()
                .Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid") // loại dữ liệu thực tế của `xmin`
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();


            modelBuilder.Entity<Comment>()
               .HasMany(c => c.Images)
               .WithOne(i => i.Comment)
               .HasForeignKey(i => i.CommentId)
               .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
            // Xóa Asp
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(), // Convert to UTC when saving
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));         // Set Kind = Utc when reading
                    }

                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime?, DateTime?>(
                            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()) : v,
                            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v));
                    }
                }
            }


        }

        override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
        }
    }
}
