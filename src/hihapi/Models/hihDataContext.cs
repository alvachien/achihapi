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
        public DbSet<HomeDefine> HomeDefines { get; set; }
        public DbSet<HomeMember> HomeMembers { get; set; }
        public DbSet<DBVersion> DBVersions { get; set; }
        public DbSet<FinanceAccountCategory> FinAccountCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DBVersion>(entity =>
            {
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                // entity.Property(e => e.Curr)
                //     .HasColumnName("CURR")
                //     .ValueGeneratedNever();
            });
            modelBuilder.Entity<Language>(entity =>
            {
                entity.Property(e => e.Lcid)
                    .HasColumnName("LCID")
                    .ValueGeneratedNever();
            });
            modelBuilder.Entity<HomeDefine>(entity => 
            {
                entity.Property(e => e.ID).ValueGeneratedOnAdd();

                entity.HasIndex(e => e.Name)
                    .HasName("UK_t_homedef_NAME")
                    .IsUnique();
            });
            modelBuilder.Entity<HomeMember>(entity => 
            {
                entity.HasKey(e => new { e.HomeID, e.User });

                entity.HasIndex(e => new { e.HomeID, e.User })
                    .HasName("UK_t_homemem_USER")
                    .IsUnique();

                entity.HasOne(d => d.HomeDefinition)
                    .WithMany(p => p.HomeMembers)
                    .HasForeignKey(d => d.HomeID)
                    .HasConstraintName("FK_t_homemem_HID");
            });

            modelBuilder.Entity<FinanceAccountCategory>(entity =>
            {

            });
        }
    }
}
