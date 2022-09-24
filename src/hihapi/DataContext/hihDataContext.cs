using System;
using Microsoft.EntityFrameworkCore;
using hihapi.Models;
using hihapi.Models.Library;
using System.Collections.Generic;

namespace hihapi
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
        public DbSet<FinancePlan> FinancePlan { get; set; }
        public DbSet<FinanceDocumentItemView> FinanceDocumentItemView { get; set; }
        public DbSet<FinanceReporAccountGroupView> FinanceReporAccountGroupView { get; set; }
        public DbSet<FinanceReporAccountGroupAndExpenseView> FinanceReporAccountGroupAndExpenseView { get; set; }
        //public DbSet<FinanceReportAccountBalanceView> FinanceReportAccountBalanceView { get; set; }
        public DbSet<FinanceReportControlCenterGroupView> FinanceReportControlCenterGroupView { get; set; }
        public DbSet<FinanceReportControlCenterGroupAndExpenseView> FinanceReportControlCenterGroupAndExpenseView { get; set; }
        // public DbSet<FinanceReportControlCenterBalanceView> FinanceReportControlCenterBalanceView { get; set; }
        public DbSet<FinanceReportOrderGroupView> FinanceReportOrderGroupView { get; set; }
        public DbSet<FinanceReportOrderGroupAndExpenseView> FinanceReportOrderGroupAndExpenseView { get; set; }
        // public DbSet<FinanceReportOrderBalanceView> FinanceReportOrderBalanceView { get; set; }
        public DbSet<BlogFormat> BlogFormats { get; set;  }
        public DbSet<BlogUserSetting> BlogUserSettings { get; set; }
        public DbSet<BlogCollection> BlogCollections { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogPostCollection> BlogPostCollections { get; set; }
        public DbSet<BlogPostTag> BlogPostTags { get; set; }
        //public DbSet<LibraryBookCategory> BookCategories { get; set; }
        //public DbSet<LibraryBook> Books { get; set; }
        public DbSet<LibraryPersonRole> PersonRoles { get; set; }
        public DbSet<LibraryPerson> Persons { get; set; }
        public DbSet<LibraryOrganizationType> OrganizationTypes { get; set; }
        public DbSet<LibraryOrganization> Organizations { get; set; }
        public DbSet<LibraryBookCategory> BookCategories { get; set; }
        public DbSet<LibraryBookLocation> BookLocations { get; set; }
        public DbSet<LibraryBook> Books { get; set; }
        public DbSet<LibraryBookBorrowRecord> BookBorrowRecords { get; set; }

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
                    .HasDatabaseName("UK_t_homedef_NAME")
                    .IsUnique();
            });
            modelBuilder.Entity<HomeMember>(entity =>
            {
                entity.HasKey(e => new { e.HomeID, e.User });

                entity.HasIndex(e => new { e.HomeID, e.User })
                    .HasDatabaseName("UK_t_homemem_USER")
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
            modelBuilder.Entity<FinancePlan>(entity => {
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
                    .WithMany(p => p.FinancePlans)
                    .HasForeignKey(d => d.HomeID)
                    .HasConstraintName("FK_t_fin_plan_HID");
            });
            modelBuilder.Entity<FinanceDocumentItemView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_DOCUMENT_ITEM");
            });
            modelBuilder.Entity<FinanceReporAccountGroupView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_GRP_ACNT");
            });
            modelBuilder.Entity<FinanceReporAccountGroupAndExpenseView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_GRP_ACNT_TRANEXP");
            });
            modelBuilder.Entity<FinanceReportAccountBalanceView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_REPORT_BS");
            });
            modelBuilder.Entity<FinanceReportControlCenterGroupView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_GRP_CC");
            });
            modelBuilder.Entity<FinanceReportControlCenterGroupAndExpenseView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_GRP_CC_TRANEXP");
            });
            modelBuilder.Entity<FinanceReportControlCenterBalanceView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_REPORT_CC");
            });
            modelBuilder.Entity<FinanceReportOrderGroupView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_GRP_ORD");
            });
            modelBuilder.Entity<FinanceReportOrderGroupAndExpenseView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_GRP_ORD_TRANEXP");
            });
            modelBuilder.Entity<FinanceReportOrderBalanceView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("V_FIN_REPORT_ORDER");
            });

            // Blogs part
            modelBuilder.Entity<BlogUserSetting>(entity =>
            {
            });
            modelBuilder.Entity<BlogFormat>(entity =>
            {
            });
            modelBuilder.Entity<BlogCollection>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.ID)
                        .UseIdentityColumn();
                }
                else
                {
                    // Workaround for Sqlite in testing mode
                    // entity.Property(e => e.ID).HasConversion(v => v, v => v);
                    entity.Property(e => e.ID)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                }
            });
            modelBuilder.Entity<BlogPost>(entity =>
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
            });

            modelBuilder.Entity<BlogPostCollection>(entity =>
            {
                entity.HasKey(e => new { e.PostID, e.CollectionID });

                entity.HasOne(d => d.BlogCollection)
                    .WithMany(p => p.BlogPostCollections)
                    .HasForeignKey(d => d.CollectionID)
                    .HasConstraintName("FK_t_blog_post_coll_coll");
                entity.HasOne(d => d.BlogPost)
                    .WithMany(p => p.BlogPostCollections)
                    .HasForeignKey(d => d.PostID)
                    .HasConstraintName("FK_t_blog_post_coll_post");
            });
            modelBuilder.Entity<BlogPostTag>(entity =>
            {
                entity.HasKey(e => new { e.PostID, e.Tag });

                entity.HasOne(d => d.BlogPost)
                    .WithMany(p => p.BlogPostTags)
                    .HasForeignKey(d => d.PostID)
                    .HasConstraintName("FK_t_blog_post_tag_post");
            });

            // Library part
            modelBuilder.Entity<LibraryPersonRole>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.Id)
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
                    entity.Property(e => e.Id)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.PersonRoles)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_lib_personrole_HID");
            });

            modelBuilder.Entity<LibraryPerson>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.Id)
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
                    entity.Property(e => e.Id)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.Persons)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_lib_person_HID");
                
                entity.HasMany(b => b.Roles)
                    .WithMany(c => c.Persons)
                    // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
                    .UsingEntity<LibraryPersonRoleLinkage>(
                        j => j
                            .HasOne(pt => pt.Role)
                            .WithMany(p => p.PersonRoles)
                            .HasForeignKey(pt => pt.RoleId),
                        j => j
                            .HasOne(pt => pt.Person)
                            .WithMany(t => t.PersonRoles)
                            .HasForeignKey(pt => pt.PersonId),
                        j =>
                        {
                            j.HasKey(t => new { t.PersonId, t.RoleId });
                        });
                //.UsingEntity<Dictionary<int, object>>(
                //    "PersonRole",
                //    l => l.HasOne<LibraryPersonRole>().WithMany().HasForeignKey("RoleId"),
                //    r => r.HasOne<LibraryPerson>().WithMany().HasForeignKey("PersonId"),
                //    j =>
                //    {
                //        j.HasKey("PersonId", "RoleId");
                //        j.ToTable("t_lib_person_role");
                //        //j.HasIndex(new[] { "TagsId" }, "IX_PostTag_TagsId");
                //    });

            });

            modelBuilder.Entity<LibraryOrganizationType>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.Id)
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
                    entity.Property(e => e.Id)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.OrganizationTypes)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_lib_orgtype_HID");
            });

            modelBuilder.Entity<LibraryOrganization>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.Id)
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
                    entity.Property(e => e.Id)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_lib_org_HID");

                entity.HasMany(b => b.Types)
                    .WithMany(c => c.Organizations)
                    // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
                    .UsingEntity<LibraryOrganizationTypeLinkage>(
                        j => j
                            .HasOne(pt => pt.OrganizationType)
                            .WithMany(p => p.OrganizationTypes)
                            .HasForeignKey(pt => pt.TypeId),
                        j => j
                            .HasOne(pt => pt.Organization)
                            .WithMany(t => t.OrganizationTypes)
                            .HasForeignKey(pt => pt.OrganizationId),
                        j =>
                        {
                            j.HasKey(t => new { t.OrganizationId, t.TypeId });
                        });
            });

            modelBuilder.Entity<LibraryBookCategory>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.Id)
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
                    entity.Property(e => e.Id)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.BookCategories)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_lib_bookctgy_HID");
            });

            modelBuilder.Entity<LibraryBookLocation>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.Id)
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
                    entity.Property(e => e.Id)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }
                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.BookLocations)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_lib_bookctgy_HID");
            });

            modelBuilder.Entity<LibraryBook>(entity =>
            {
                if (!TestingMode)
                {
                    entity.Property(e => e.Id)
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
                    entity.Property(e => e.Id)
                        .HasColumnType("INTEGER")
                        .ValueGeneratedOnAdd();
                    entity.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                    entity.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_DATE");
                }

                entity.HasOne(d => d.CurrentHome)
                    .WithMany(p => p.Books)
                    .HasForeignKey(d => d.HomeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_lib_book_HID");

                entity.HasMany(b => b.Categories)
                    .WithMany(c => c.Books)
                    // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
                    .UsingEntity<LibraryBookCategoryLinkage>(
                        j => j
                            .HasOne(pt => pt.Category)
                            .WithMany(p => p.BookCategories)
                            .HasForeignKey(pt => pt.CategoryId),
                        j => j
                            .HasOne(pt => pt.Book)
                            .WithMany(t => t.BookCategories)
                            .HasForeignKey(pt => pt.BookId),
                        j =>
                        {
                            j.HasKey(t => new { t.BookId, t.CategoryId });
                        });

                entity.HasMany(b => b.Authors)
                    .WithMany(c => c.WritenBooks)
                    // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
                    .UsingEntity<LibraryBookAuthorLinkage>(
                        j => j
                            .HasOne(pt => pt.Author)
                            .WithMany(p => p.WrittenBooksByAuthor)
                            .HasForeignKey(pt => pt.AuthorId),
                        j => j
                            .HasOne(pt => pt.Book)
                            .WithMany(t => t.BookAuthors)
                            .HasForeignKey(pt => pt.BookId),
                        j =>
                        {
                            j.HasKey(t => new { t.BookId, t.AuthorId });
                        });

                entity.HasMany(b => b.Translators)
                    .WithMany(c => c.TranslatedBooks)
                    // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
                    .UsingEntity<LibraryBookTranslatorLinkage>(
                        j => j
                            .HasOne(pt => pt.Translator)
                            .WithMany(p => p.TranslatedBooksByTranslator)
                            .HasForeignKey(pt => pt.TranslatorId),
                        j => j
                            .HasOne(pt => pt.Book)
                            .WithMany(t => t.BookTranslators)
                            .HasForeignKey(pt => pt.BookId),
                        j =>
                        {
                            j.HasKey(t => new { t.BookId, t.TranslatorId });
                        });

                entity.HasMany(b => b.Presses)
                    .WithMany(c => c.Books)
                    // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
                    .UsingEntity<LibraryBookPressLinkage>(
                        j => j
                            .HasOne(pt => pt.Press)
                            .WithMany(p => p.PressedBooks)
                            .HasForeignKey(pt => pt.PressId),
                        j => j
                            .HasOne(pt => pt.Book)
                            .WithMany(t => t.BookPresses)
                            .HasForeignKey(pt => pt.BookId),
                        j =>
                        {
                            j.HasKey(t => new { t.BookId, t.PressId });
                        });

                entity.HasMany(b => b.Locations)
                    .WithMany(c => c.Books)
                    // https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
                    .UsingEntity<LibraryBookLocationLinkage>(
                        j => j
                            .HasOne(pt => pt.Location)
                            .WithMany(p => p.BooksInLocation)
                            .HasForeignKey(pt => pt.LocationId),
                        j => j
                            .HasOne(pt => pt.Book)
                            .WithMany(t => t.BookLocations)
                            .HasForeignKey(pt => pt.BookId),
                        j =>
                        {
                            j.HasKey(t => new { t.BookId, t.LocationId });
                        });
            });
        }
    }
}
