using Microsoft.EntityFrameworkCore;
using poplensMediaApi.Models;

namespace poplensMediaApi.Data {
    public class MediaDbContext : DbContext {
        public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options) { }

        public DbSet<Media> Media { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.HasPostgresExtension("vector");
            // Specify the schema
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<Media>()
                .Property(m => m.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation

            modelBuilder.Entity<Media>()
                .Property(m => m.CreatedDate)
                .HasDefaultValueSql("NOW()"); // Default value in PostgreSQL

            modelBuilder.Entity<Media>()
                .Property(m => m.LastUpdatedDate)
                .HasDefaultValueSql("NOW()"); // Default value in PostgreSQL
            
            modelBuilder.Entity<Media>()
                .Property(r => r.Embedding)
                .HasColumnType("vector(384)");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseNpgsql("Host=postgresMedia;Port=5432;Username=postgre;Password=postgre;Database=Media", o => o.UseVector());

            base.OnConfiguring(optionsBuilder);
        }
    }
}
