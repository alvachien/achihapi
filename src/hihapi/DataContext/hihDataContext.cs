using System;
using Microsoft.EntityFrameworkCore;

namespace hihapi.Models
{
    public class hihDataContext : DbContext
    {
        public hihDataContext()
        {
            TestingMode = false;
        }
        
        public hihDataContext(DbContextOptions<hihDataContext> options, bool testingMode = false) : base(options)
        {
            TestingMode = testingMode;
        }

        // Testing mode
        public Boolean TestingMode { get; private set; }

        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<HomeDefine> HomeDefines { get; set; }
        public DbSet<HomeMember> HomeMembers { get; set; }
        public DbSet<DBVersion> DBVersions { get; set; }
        public DbSet<FinanceAccountCategory> FinAccountCategories { get; set; }
        public DbSet<FinanceAssetCategory> FinAssetCategories { get; set; }
        public DbSet<FinanceDocumentType> FinDocumentTypes { get; set; }
        public DbSet<FinanceTransactionType> FinTransactionType { get; set; }
        public DbSet<FinanceAccount> FinanceAccount { get; set; }
        public DbSet<FinanceAccountExtraDP> FinanceAccountExtraDP { get; set; }
        public DbSet<FinanceAccountExtraAS> FinanceAccountExtraAS { get; set; }
        public DbSet<FinanceAccountExtraLoan> FinanceAccountExtraLoan { get; set; }
        public DbSet<FinanceDocument> FinanceDocument { get; set; }
        public DbSet<FinanceDocumentItem> FinanceDocumentItem { get; set; }
        public DbSet<FinanceTmpDPDocument> FinanceTmpDPDocument { get; set; }
        public DbSet<FinanceTmpLoanDocument> FinanceTmpLoanDocument { get; set; }
        public DbSet<FinanceControlCenter> FinanceControlCenter { get; set; }
        public DbSet<FinanceOrder> FinanceOrder { get; set; }
        public DbSet<FinanceOrderSRule> FinanceOrderSRule { get; set; }
        public DbSet<FinanceDocumentItemView> FinanceDocumentItemView { get; set; }
        public DbSet<FinanceReporAccountGroupView> FinanceReporAccountGroupView { get; set; }
        public DbSet<FinanceReporAccountGroupAndExpenseView> FinanceReporAccountGroupAndExpenseView { get; set; }
        public DbSet<FinanceReportAccountBalanceView> FinanceReportAccountBalanceView { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                // entity.Property(e => e.Curr)
                //     .HasColumnName("CURR")
                //     .ValueGeneratedNever();
            });
            modelBuilder.Entity<Language>(entity =>
            {
                entity.Property(e => e.Lcid)
                    .HasColumnName("LCID")
                    .ValueGeneratedNever();
                if (!TestingMode)
                {
                    entity.Property(e => e.AppFlag)
                        .HasDefaultValueSql("((0))");
                }
            });
            modelBuilder.Entity<DBVersion>(entity =>
            {
                entity.Property(e => e.VersionID)
                    .ValueGeneratedNever();
                if (!TestingMode)
                {
                    entity.Property(e => e.AppliedDate)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    entity.Property(e => e.AppliedDate)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
            });

            modelBuilder.Entity<HomeDefine>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();
                        //.ValueGeneratedOnAdd();
                }
                else
                {
//                    entity.Property(e => e.ID).HasConversion(v => v, v => v);            
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                }

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
            // [t_homemsg]

            modelBuilder.Entity<FinanceAccountCategory>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();
                        //.ValueGeneratedOnAdd();
                    entity.Property(e => e.AssetFlag)
                        .HasColumnType<Boolean>("BIT")
                        .HasDefaultValueSql("((1))");
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    // Workaround for Sqlite in testing mode
                    // entity.Property(e => e.ID).HasConversion(v => v, v => v);
                    entity.Property(e => e.AssetFlag)
                        .HasColumnType<Boolean>("INTEGER");
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.FinanceAccountCategories)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_account_ctgy_HID");
            });
            modelBuilder.Entity<FinanceAssetCategory>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();
                        //.ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    // Workaround for Sqlite in testing mode
                    // entity.Property(e => e.ID).HasConversion(v => v, v => v);
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.FinanceAssetCategories)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_fin_asset_ctgy_HID");
            });
            modelBuilder.Entity<FinanceDocumentType>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .HasColumnType("SMALLINT")
                        .UseIdentityColumn();
                        //.ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    // Workaround for Sqlite in testing mode
                    // entity.Property(e => e.ID).HasConversion(v => v, v => v);
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.FinanceDocumentTypes)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_fin_doctype_HID");
            });
            modelBuilder.Entity<FinanceTransactionType>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    // Workaround for Sqlite in testing mode
                    // entity.Property(e => e.ID).HasConversion(v => v, v => v);
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.FinanceTransactionTypes)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_fin_trantype_HID");
            });
       
            modelBuilder.Entity<FinanceAccount> (entity => {
                if (!TestingMode) 
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }

                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.FinanceAccounts)
                    .HasForeignKey(d => d.HomeID)
                    .HasConstraintName("FK_t_account_HID");
            });
            modelBuilder.Entity<FinanceAccountExtraAS>(entity => {
                if (TestingMode)
                {
                    entity.Property(e => e.AccountID).HasConversion(v => v, v => v);
                }

                entity.HasOne(d => d.AccountHeader)
                    .WithOne(p => p.ExtraAsset)
                    .HasForeignKey<FinanceAccountExtraAS>(p => p.AccountID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_fin_account_ext_as_ACNT");
                entity.HasOne(d => d.AssetCategory)
                    .WithMany(p => p.AccountExtraAsset)
                    .HasForeignKey(p => p.CategoryID)
                    .HasConstraintName("FK_t_fin_account_ext_as_CTGY");
            });
            modelBuilder.Entity<FinanceAccountExtraDP>(entity => {
                if (TestingMode)
                {
                    entity.Property(e => e.AccountID).HasConversion(v => v, v => v);
                }
                entity.HasOne(d => d.AccountHeader)
                    .WithOne(p => p.ExtraDP)
                    .HasForeignKey<FinanceAccountExtraDP>(p => p.AccountID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_fin_account_ext_dp_ACNT");
            });
            modelBuilder.Entity<FinanceAccountExtraLoan>(entity => {
                if (TestingMode)
                {
                    entity.Property(e => e.AccountID).HasConversion(v => v, v => v);
                }
                entity.HasOne(d => d.AccountHeader)
                    .WithOne(p => p.ExtraLoan)
                    .HasForeignKey<FinanceAccountExtraLoan>(p => p.AccountID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_fin_account_ext_loan_ACNT");
            });

            modelBuilder.Entity<FinanceDocument>(entity => {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }

                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.FinanceDocuments)
                    .HasForeignKey(d => d.HomeID)
                    .HasConstraintName("FK_t_fin_document_HID");
            });
            modelBuilder.Entity<FinanceDocumentItem>(entity => {
                entity.HasKey(p => new { p.DocID, p.ItemID });

                entity.HasOne(d => d.DocumentHeader)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.DocID)
                    .HasConstraintName("FK_t_fin_document_header");
            });
            modelBuilder.Entity<FinanceTmpLoanDocument>(entity => 
            {
                //if (!TestingMode)
                //{
                //    entity.Property(e => e.DocumentID)
                //        .UseIdentityColumn();
                //}
                //else
                //{
                //    entity.Property(e => e.DocumentID)
                //        .HasColumnType("INTEGER")
                //        .ValueGeneratedOnAdd();
                //}
                entity.HasKey(p => new { p.DocumentID, p.AccountID, p.HomeID });

                entity.HasOne(d => d.AccountExtra)
                    .WithMany(p => p.LoanTmpDocs)
                    .HasForeignKey(d => d.AccountID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_fin_tmpdoc_loan_ACCOUNTEXT");
            });
            modelBuilder.Entity<FinanceTmpDPDocument>(entity => 
            {
                //if (!TestingMode)
                //{
                //    entity.Property(e => e.DocumentID)
                //        .UseIdentityColumn();
                //}
                //else
                //{
                //    entity.Property(e => e.DocumentID)
                //        .HasColumnType("INTEGER")
                //        .ValueGeneratedOnAdd();
                //}
                entity.HasKey(p => new { p.DocumentID, p.AccountID, p.HomeID });

                entity.HasOne(d => d.AccountExtra)
                    .WithMany(p => p.DPTmpDocs)
                    .HasForeignKey(d => d.AccountID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_fin_tmpdocdp_ACCOUNTEXT");
            });
            modelBuilder.Entity<FinanceControlCenter>(entity => {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();

                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.FinanceControlCenters)
                    .HasForeignKey(d => d.HomeID)
                    .HasConstraintName("FK_t_fin_cc_HID");
            });
            modelBuilder.Entity<FinanceOrder>(entity => {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("(getdate())");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("(getdate())");
                }
                else
                {
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }

                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.FinanceOrders)
                    .HasForeignKey(d => d.HomeID)
                    .HasConstraintName("FK_t_fin_order_HID");
            });
            modelBuilder.Entity<FinanceOrderSRule>(entity => {
                entity.HasKey(p => new { p.OrderID, p.RuleID });

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.SRule)
                    .HasForeignKey(d => d.OrderID)
                    .HasConstraintName("FK_t_fin_order_srule_order");
            });

            modelBuilder.Entity<FinanceDocumentItemView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_DOCUMENT_ITEM");
            });
            modelBuilder.Entity<FinanceReporAccountGroupView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("v_fin_grp_acnt");
            });

            modelBuilder.Entity<FinanceReporAccountGroupAndExpenseView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("v_fin_grp_acnt_tranexp");
            });

            modelBuilder.Entity<FinanceReportAccountBalanceView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("v_fin_report_bs");
            });
        }
    }
}
