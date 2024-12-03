﻿namespace poplensMediaApi.Data {
    using Microsoft.EntityFrameworkCore;
    using poplensMediaApi.Models;
    using System.Collections.Generic;
    using System.Reflection.Emit;

    public class MediaDbContext : DbContext {
        public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options) { }

        public DbSet<Media> Media { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Media>()
                .Property(m => m.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation
        }
    }

}