using System;
using Microsoft.EntityFrameworkCore;

namespace hihapi.Models
{
    public class hihDataContext : DbContext
    {
        public hihDataContext()
        {            
        }
        
        public hihDataContext(DbContextOptions<hihDataContext> options) : base(options)
        { 
        }

        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Language> Languages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>(entity =>
            {
                entity.Property(e => e.Curr)
                    .HasColumnName("CURR")
                    .ValueGeneratedNever();
            });
            modelBuilder.Entity<Language>(entity =>
            {
                entity.Property(e => e.Lcid)
                    .HasColumnName("LCID")
                    .ValueGeneratedNever();
            });
        }
    }
}
