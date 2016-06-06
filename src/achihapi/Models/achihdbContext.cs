using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace achihapi
{
    public partial class achihdbContext : DbContext
    {
        public achihdbContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ENPOS>(entity =>
            {
                entity.HasKey(e => e.POSAbb)
                    .HasName("PK_ENPOS");

                entity.Property(e => e.POSAbb).HasMaxLength(10);

                entity.Property(e => e.POSName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<EnPOST>(entity =>
            {
                entity.HasKey(e => new { e.POSAbb, e.LangID })
                    .HasName("PK_EnPOST");

                entity.Property(e => e.POSAbb).HasMaxLength(10);

                entity.Property(e => e.LangID).HasMaxLength(5);

                entity.Property(e => e.POSName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.POSAbbNavigation)
                    .WithMany(p => p.EnPOST)
                    .HasForeignKey(d => d.POSAbb)
                    .HasConstraintName("FK_EnPOST_POS");
            });

            modelBuilder.Entity<EnSentence>(entity =>
            {
                entity.HasKey(e => e.SentenceID)
                    .HasName("PK_EnSentence");

                entity.Property(e => e.SentenceID).ValueGeneratedNever();

                entity.Property(e => e.SentenceString)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Tags).HasMaxLength(100);
            });

            modelBuilder.Entity<EnSentenceExplain>(entity =>
            {
                entity.HasKey(e => new { e.SentenceID, e.ExplainID })
                    .HasName("PK_EnSentenceExplain");

                entity.HasOne(d => d.Sentence)
                    .WithMany(p => p.EnSentenceExplain)
                    .HasForeignKey(d => d.SentenceID)
                    .HasConstraintName("FK_EnSentenceExplain_EnSentence");
            });

            modelBuilder.Entity<EnSentenceExplainT>(entity =>
            {
                entity.HasKey(e => new { e.SentenceID, e.ExplainID, e.LangID })
                    .HasName("PK_EnSentenceExplainT");

                entity.Property(e => e.LangID).HasMaxLength(5);

                entity.Property(e => e.ExplainString)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.EnSentenceExplain)
                    .WithMany(p => p.EnSentenceExplainT)
                    .HasForeignKey(d => new { d.SentenceID, d.ExplainID })
                    .HasConstraintName("FK_EnSentenceExplainT_SentenceExplain");
            });

            modelBuilder.Entity<EnSentenceWord>(entity =>
            {
                entity.HasKey(e => new { e.SentenceID, e.WordID })
                    .HasName("PK_EnSentenceWord");

                entity.HasOne(d => d.Sentence)
                    .WithMany(p => p.EnSentenceWord)
                    .HasForeignKey(d => d.SentenceID)
                    .HasConstraintName("FK_EnSentenceWord_Sentence");

                entity.HasOne(d => d.Word)
                    .WithMany(p => p.EnSentenceWord)
                    .HasForeignKey(d => d.WordID)
                    .HasConstraintName("FK_EnSentenceWord_Word");
            });

            modelBuilder.Entity<EnWord>(entity =>
            {
                entity.HasKey(e => e.WordID)
                    .HasName("PK_EnWords");

                entity.Property(e => e.Tags).HasMaxLength(50);

                entity.Property(e => e.WordString)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<EnWordExplain>(entity =>
            {
                entity.HasKey(e => new { e.WordID, e.ExplainID })
                    .HasName("PK_EnWordExplain");

                entity.Property(e => e.POSAbb)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.HasOne(d => d.Word)
                    .WithMany(p => p.EnWordExplain)
                    .HasForeignKey(d => d.WordID)
                    .HasConstraintName("FK_EnWordExplain_Word");
            });

            modelBuilder.Entity<EnWordExplainT>(entity =>
            {
                entity.HasKey(e => new { e.WordID, e.ExplainID, e.LangID })
                    .HasName("PK_EnWordExplainT");

                entity.Property(e => e.LangID).HasMaxLength(5);

                entity.Property(e => e.ExplainString)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.EnWordExplain)
                    .WithMany(p => p.EnWordExplainT)
                    .HasForeignKey(d => new { d.WordID, d.ExplainID })
                    .HasConstraintName("FK_EnWordExplainT_WordExplain");
            });

            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.HasKey(e => e.ToDoID)
                    .HasName("PK_TodoItem");

                entity.Property(e => e.ToDoID).ValueGeneratedNever();

                entity.Property(e => e.Assignee).HasMaxLength(50);

                entity.Property(e => e.Dependence).HasMaxLength(50);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Tags).HasMaxLength(50);
            });
        }

        public virtual DbSet<ENPOS> ENPOS { get; set; }
        public virtual DbSet<EnPOST> EnPOST { get; set; }
        public virtual DbSet<EnSentence> EnSentence { get; set; }
        public virtual DbSet<EnSentenceExplain> EnSentenceExplain { get; set; }
        public virtual DbSet<EnSentenceExplainT> EnSentenceExplainT { get; set; }
        public virtual DbSet<EnSentenceWord> EnSentenceWord { get; set; }
        public virtual DbSet<EnWord> EnWord { get; set; }
        public virtual DbSet<EnWordExplain> EnWordExplain { get; set; }
        public virtual DbSet<EnWordExplainT> EnWordExplainT { get; set; }
        public virtual DbSet<TodoItem> TodoItem { get; set; }
    }
}