using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace achihapi.Models
{
    public partial class achihdbContext : DbContext
    {
        public achihdbContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnPost>(entity =>
            {
                entity.HasKey(e => new { e.Posabb, e.LangId })
                    .HasName("PK_EnPOST");

                entity.ToTable("EnPOST");

                entity.Property(e => e.Posabb)
                    .HasColumnName("POSAbb")
                    .HasMaxLength(10);

                entity.Property(e => e.LangId)
                    .HasColumnName("LangID")
                    .HasMaxLength(5);

                entity.Property(e => e.Posname)
                    .IsRequired()
                    .HasColumnName("POSName")
                    .HasMaxLength(50);

                entity.HasOne(d => d.PosabbNavigation)
                    .WithMany(p => p.EnPost)
                    .HasForeignKey(d => d.Posabb)
                    .HasConstraintName("FK_EnPOST_POS");
            });

            modelBuilder.Entity<EnSentence>(entity =>
            {
                entity.HasKey(e => e.SentenceId)
                    .HasName("PK_EnSentence");

                entity.Property(e => e.SentenceId)
                    .HasColumnName("SentenceID")
                    .ValueGeneratedNever();

                entity.Property(e => e.SentenceString)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Tags).HasMaxLength(100);
            });

            modelBuilder.Entity<EnSentenceExplain>(entity =>
            {
                entity.HasKey(e => new { e.SentenceId, e.ExplainId })
                    .HasName("PK_EnSentenceExplain");

                entity.Property(e => e.SentenceId).HasColumnName("SentenceID");

                entity.Property(e => e.ExplainId).HasColumnName("ExplainID");

                entity.HasOne(d => d.Sentence)
                    .WithMany(p => p.EnSentenceExplain)
                    .HasForeignKey(d => d.SentenceId)
                    .HasConstraintName("FK_EnSentenceExplain_EnSentence");
            });

            modelBuilder.Entity<EnSentenceExplainT>(entity =>
            {
                entity.HasKey(e => new { e.SentenceId, e.ExplainId, e.LangId })
                    .HasName("PK_EnSentenceExplainT");

                entity.Property(e => e.SentenceId).HasColumnName("SentenceID");

                entity.Property(e => e.ExplainId).HasColumnName("ExplainID");

                entity.Property(e => e.LangId)
                    .HasColumnName("LangID")
                    .HasMaxLength(5);

                entity.Property(e => e.ExplainString)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.EnSentenceExplain)
                    .WithMany(p => p.EnSentenceExplainT)
                    .HasForeignKey(d => new { d.SentenceId, d.ExplainId })
                    .HasConstraintName("FK_EnSentenceExplainT_SentenceExplain");
            });

            modelBuilder.Entity<EnSentenceWord>(entity =>
            {
                entity.HasKey(e => new { e.SentenceId, e.WordId })
                    .HasName("PK_EnSentenceWord");

                entity.Property(e => e.SentenceId).HasColumnName("SentenceID");

                entity.Property(e => e.WordId).HasColumnName("WordID");

                entity.HasOne(d => d.Sentence)
                    .WithMany(p => p.EnSentenceWord)
                    .HasForeignKey(d => d.SentenceId)
                    .HasConstraintName("FK_EnSentenceWord_Sentence");

                entity.HasOne(d => d.Word)
                    .WithMany(p => p.EnSentenceWord)
                    .HasForeignKey(d => d.WordId)
                    .HasConstraintName("FK_EnSentenceWord_Word");
            });

            modelBuilder.Entity<EnWord>(entity =>
            {
                entity.HasKey(e => e.WordId)
                    .HasName("PK_EnWords");

                entity.Property(e => e.WordId).HasColumnName("WordID");

                entity.Property(e => e.Tags).HasMaxLength(50);

                entity.Property(e => e.WordString)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<EnWordExplain>(entity =>
            {
                entity.HasKey(e => new { e.WordId, e.ExplainId })
                    .HasName("PK_EnWordExplain");

                entity.Property(e => e.WordId).HasColumnName("WordID");

                entity.Property(e => e.ExplainId).HasColumnName("ExplainID");

                entity.Property(e => e.Posabb)
                    .HasColumnName("POSAbb")
                    .HasMaxLength(10);

                entity.HasOne(d => d.Word)
                    .WithMany(p => p.EnWordExplain)
                    .HasForeignKey(d => d.WordId)
                    .HasConstraintName("FK_EnWordExplain_Word");
            });

            modelBuilder.Entity<EnWordExplainT>(entity =>
            {
                entity.HasKey(e => new { e.WordId, e.ExplainId, e.LangId })
                    .HasName("PK_EnWordExplainT");

                entity.Property(e => e.WordId).HasColumnName("WordID");

                entity.Property(e => e.ExplainId).HasColumnName("ExplainID");

                entity.Property(e => e.LangId)
                    .HasColumnName("LangID")
                    .HasMaxLength(5);

                entity.Property(e => e.ExplainString)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.EnWordExplain)
                    .WithMany(p => p.EnWordExplainT)
                    .HasForeignKey(d => new { d.WordId, d.ExplainId })
                    .HasConstraintName("FK_EnWordExplainT_WordExplain");
            });

            modelBuilder.Entity<Enpos>(entity =>
            {
                entity.HasKey(e => e.Posabb)
                    .HasName("PK_ENPOS");

                entity.ToTable("ENPOS");

                entity.Property(e => e.Posabb)
                    .HasColumnName("POSAbb")
                    .HasMaxLength(10);

                entity.Property(e => e.Posname)
                    .IsRequired()
                    .HasColumnName("POSName")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GalleryAlbum>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("IX_GalleryAlbum_Name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Comment).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<GalleryAlbumItems>(entity =>
            {
                entity.HasKey(e => new { e.AlbumId, e.PhotoId })
                    .HasName("PK_GalleryAlbumItems");

                entity.Property(e => e.AlbumId).HasColumnName("AlbumID");

                entity.Property(e => e.PhotoId).HasColumnName("PhotoID");

                entity.HasOne(d => d.Album)
                    .WithMany(p => p.GalleryAlbumItems)
                    .HasForeignKey(d => d.AlbumId)
                    .HasConstraintName("FK_GalleryAlbumItems_Album");

                entity.HasOne(d => d.Photo)
                    .WithMany(p => p.GalleryAlbumItems)
                    .HasForeignKey(d => d.PhotoId)
                    .HasConstraintName("FK_GalleryAlbumItems_Photo");
            });

            modelBuilder.Entity<GalleryItem>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("IX_GalleryItem_Name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CameraInfo).HasMaxLength(50);

                entity.Property(e => e.Comment).HasMaxLength(100);

                entity.Property(e => e.Exifinfo)
                    .HasColumnName("EXIFInfo")
                    .HasColumnType("nchar(100)");

                entity.Property(e => e.FileFormat).HasColumnType("nchar(10)");

                entity.Property(e => e.FullUrl)
                    .HasColumnName("FullURL")
                    .HasMaxLength(100);

                entity.Property(e => e.LensInfo).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Tags).HasMaxLength(50);

                entity.Property(e => e.UploadedBy).HasMaxLength(50);

                entity.Property(e => e.UploadedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");
            });

            modelBuilder.Entity<Knowledge>(entity =>
            {
                entity.HasIndex(e => e.Title)
                    .HasName("IX_KnowledgeTitle")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Content).IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.ModifiedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.Tags).HasColumnType("nchar(100)");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<KnowledgeType>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("IX_KnowledgeTypeName")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Comment).HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");
            });

            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.HasKey(e => e.ToDoId)
                    .HasName("PK_TodoItem");

                entity.Property(e => e.ToDoId)
                    .HasColumnName("ToDoID")
                    .ValueGeneratedNever();

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

        public virtual DbSet<EnPost> EnPost { get; set; }
        public virtual DbSet<EnSentence> EnSentence { get; set; }
        public virtual DbSet<EnSentenceExplain> EnSentenceExplain { get; set; }
        public virtual DbSet<EnSentenceExplainT> EnSentenceExplainT { get; set; }
        public virtual DbSet<EnSentenceWord> EnSentenceWord { get; set; }
        public virtual DbSet<EnWord> EnWord { get; set; }
        public virtual DbSet<EnWordExplain> EnWordExplain { get; set; }
        public virtual DbSet<EnWordExplainT> EnWordExplainT { get; set; }
        public virtual DbSet<Enpos> Enpos { get; set; }
        public virtual DbSet<GalleryAlbum> GalleryAlbum { get; set; }
        public virtual DbSet<GalleryAlbumItems> GalleryAlbumItems { get; set; }
        public virtual DbSet<GalleryItem> GalleryItem { get; set; }
        public virtual DbSet<Knowledge> Knowledge { get; set; }
        public virtual DbSet<KnowledgeType> KnowledgeType { get; set; }
        public virtual DbSet<TodoItem> TodoItem { get; set; }

        // Unable to generate entity type for table 'dbo.TagRelation'. Please see the warning messages.
    }
}