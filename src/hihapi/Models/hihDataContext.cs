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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>(entity =>
            {
                entity.Property(e => e.Curr)
                    .HasColumnName("CURR")
                    .ValueGeneratedNever();
            });
        }
    }
}
