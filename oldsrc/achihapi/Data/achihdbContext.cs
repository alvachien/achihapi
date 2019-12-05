using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace achihapi.Models
{
    public partial class achihdbContext : DbContext
    {
        public achihdbContext()
        {
        }

        public achihdbContext(DbContextOptions<achihdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TEvent> TEvent { get; set; }
        public virtual DbSet<TEventHabit> TEventHabit { get; set; }
        public virtual DbSet<TEventHabitCheckin> TEventHabitCheckin { get; set; }
        public virtual DbSet<TEventHabitDetail> TEventHabitDetail { get; set; }
        public virtual DbSet<TEventRecur> TEventRecur { get; set; }
        public virtual DbSet<TFinAccount> TFinAccount { get; set; }
        public virtual DbSet<TFinAccountCtgy> TFinAccountCtgy { get; set; }
        public virtual DbSet<TFinAccountExtAs> TFinAccountExtAs { get; set; }
        public virtual DbSet<TFinAccountExtCc> TFinAccountExtCc { get; set; }
        public virtual DbSet<TFinAccountExtDp> TFinAccountExtDp { get; set; }
        public virtual DbSet<TFinAccountExtLoan> TFinAccountExtLoan { get; set; }
        public virtual DbSet<TFinAccountExtLoanH> TFinAccountExtLoanH { get; set; }
        public virtual DbSet<TFinAssetCtgy> TFinAssetCtgy { get; set; }
        public virtual DbSet<TFinDocument> TFinDocument { get; set; }
        public virtual DbSet<TFinDocumentItem> TFinDocumentItem { get; set; }
        public virtual DbSet<TFinPlan> TFinPlan { get; set; }
        public virtual DbSet<TFinTmpdocDp> TFinTmpdocDp { get; set; }
        public virtual DbSet<TFinTmpdocLoan> TFinTmpdocLoan { get; set; }
        public virtual DbSet<THomedef> THomedef { get; set; }
        public virtual DbSet<THomemem> THomemem { get; set; }
        public virtual DbSet<THomemsg> THomemsg { get; set; }
        public virtual DbSet<TLearnCtgy> TLearnCtgy { get; set; }
        public virtual DbSet<TLearnEnsent> TLearnEnsent { get; set; }
        public virtual DbSet<TLearnEnsentexp> TLearnEnsentexp { get; set; }
        public virtual DbSet<TLearnEnsentWord> TLearnEnsentWord { get; set; }
        public virtual DbSet<TLearnEnword> TLearnEnword { get; set; }
        public virtual DbSet<TLearnEnwordexp> TLearnEnwordexp { get; set; }
        public virtual DbSet<TLearnHist> TLearnHist { get; set; }
        public virtual DbSet<TLearnObj> TLearnObj { get; set; }
        public virtual DbSet<TLearnQtnBank> TLearnQtnBank { get; set; }
        public virtual DbSet<TLearnQtnBankSub> TLearnQtnBankSub { get; set; }
        public virtual DbSet<TLibBook> TLibBook { get; set; }
        public virtual DbSet<TLibBookCtgy> TLibBookCtgy { get; set; }
        public virtual DbSet<TLibLocation> TLibLocation { get; set; }
        public virtual DbSet<TLibLocationDetail> TLibLocationDetail { get; set; }
        public virtual DbSet<TLibPerson> TLibPerson { get; set; }
        public virtual DbSet<TTag> TTag { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TEvent>(entity =>
            {
                entity.ToTable("t_event");

                entity.HasIndex(e => new { e.Hid, e.Name })
                    .HasName("UX_t_event_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Assignee).HasMaxLength(40);

                entity.Property(e => e.CompleteTime).HasColumnType("datetime");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.IsPublic)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RefRecurId).HasColumnName("RefRecurID");

                entity.Property(e => e.StartTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TEvent)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_event_t_hid");
            });

            modelBuilder.Entity<TEventHabit>(entity =>
            {
                entity.ToTable("t_event_habit");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Assignee).HasMaxLength(40);

                entity.Property(e => e.Content).HasColumnName("CONTENT");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Rpttype).HasColumnName("RPTTYPE");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TEventHabit)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_event_habit_t_HID");
            });

            modelBuilder.Entity<TEventHabitCheckin>(entity =>
            {
                entity.ToTable("t_event_habit_checkin");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Comment).HasMaxLength(50);

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.HabitId).HasColumnName("HabitID");

                entity.Property(e => e.TranDate).HasColumnType("datetime");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.Habit)
                    .WithMany(p => p.TEventHabitCheckin)
                    .HasForeignKey(d => d.HabitId)
                    .HasConstraintName("FK_t_event_habit_checkin_HabitID");
            });

            modelBuilder.Entity<TEventHabitDetail>(entity =>
            {
                entity.ToTable("t_event_habit_detail");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.HabitId).HasColumnName("HabitID");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.HasOne(d => d.Habit)
                    .WithMany(p => p.TEventHabitDetail)
                    .HasForeignKey(d => d.HabitId)
                    .HasConstraintName("FK_t_event_habit_HabitID");
            });

            modelBuilder.Entity<TEventRecur>(entity =>
            {
                entity.ToTable("t_event_recur");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Assignee)
                    .HasColumnName("ASSIGNEE")
                    .HasMaxLength(40);

                entity.Property(e => e.Content).HasColumnName("CONTENT");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Enddate)
                    .HasColumnName("ENDDATE")
                    .HasColumnType("date");

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Ispublic)
                    .HasColumnName("ISPUBLIC")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("NAME")
                    .HasMaxLength(50);

                entity.Property(e => e.Rpttype).HasColumnName("RPTTYPE");

                entity.Property(e => e.Startdate)
                    .HasColumnName("STARTDATE")
                    .HasColumnType("date");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TEventRecur)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_event_recur_HID");
            });

            modelBuilder.Entity<TFinAccount>(entity =>
            {
                entity.ToTable("t_fin_account");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Comment)
                    .HasColumnName("COMMENT")
                    .HasMaxLength(45);

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(50);

                entity.Property(e => e.Ctgyid).HasColumnName("CTGYID");

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("NAME")
                    .HasMaxLength(30);

                entity.Property(e => e.Owner)
                    .HasColumnName("OWNER")
                    .HasMaxLength(50);

                entity.Property(e => e.Status).HasColumnName("STATUS");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(50);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TFinAccount)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_account_HID");
            });

            modelBuilder.Entity<TFinAccountExtCc>(entity =>
            {
                entity.HasKey(e => e.Accountid);

                entity.ToTable("t_fin_account_ext_cc");

                entity.Property(e => e.Accountid)
                    .HasColumnName("ACCOUNTID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Bank)
                    .HasColumnName("BANK")
                    .HasMaxLength(50);

                entity.Property(e => e.Billdayinmonth).HasColumnName("BILLDAYINMONTH");

                entity.Property(e => e.Cardnum)
                    .IsRequired()
                    .HasColumnName("CARDNUM")
                    .HasMaxLength(20);

                entity.Property(e => e.Others)
                    .HasColumnName("OTHERS")
                    .HasMaxLength(100);

                entity.Property(e => e.Repaydayinmonth).HasColumnName("REPAYDAYINMONTH");

                entity.Property(e => e.Validdate)
                    .HasColumnName("VALIDDATE")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.TFinAccountExtCc)
                    .HasForeignKey<TFinAccountExtCc>(d => d.Accountid)
                    .HasConstraintName("FK_t_fin_account_ext_cc_ID");
            });

            modelBuilder.Entity<TFinAccountExtDp>(entity =>
            {
                entity.HasKey(e => e.Accountid);

                entity.ToTable("t_fin_account_ext_dp");

                entity.Property(e => e.Accountid)
                    .HasColumnName("ACCOUNTID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Comment)
                    .HasColumnName("COMMENT")
                    .HasMaxLength(45);

                entity.Property(e => e.Defrrdays)
                    .HasColumnName("DEFRRDAYS")
                    .HasMaxLength(100);

                entity.Property(e => e.Direct).HasColumnName("DIRECT");

                entity.Property(e => e.Enddate)
                    .HasColumnName("ENDDATE")
                    .HasColumnType("date");

                entity.Property(e => e.Refdocid).HasColumnName("REFDOCID");

                entity.Property(e => e.Rpttype).HasColumnName("RPTTYPE");

                entity.Property(e => e.Startdate)
                    .HasColumnName("STARTDATE")
                    .HasColumnType("date");

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.TFinAccountExtDp)
                    .HasForeignKey<TFinAccountExtDp>(d => d.Accountid)
                    .HasConstraintName("FK_t_fin_account_ext_dp_id");
            });

            modelBuilder.Entity<TFinAccountExtLoan>(entity =>
            {
                entity.HasKey(e => e.Accountid);

                entity.ToTable("t_fin_account_ext_loan");

                entity.Property(e => e.Accountid)
                    .HasColumnName("ACCOUNTID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Annualrate)
                    .HasColumnName("ANNUALRATE")
                    .HasColumnType("decimal(17, 2)");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Interestfree).HasColumnName("INTERESTFREE");

                entity.Property(e => e.Others)
                    .HasColumnName("OTHERS")
                    .HasMaxLength(100);

                entity.Property(e => e.Partner)
                    .HasColumnName("PARTNER")
                    .HasMaxLength(50);

                entity.Property(e => e.Payingaccount).HasColumnName("PAYINGACCOUNT");

                entity.Property(e => e.Refdocid).HasColumnName("REFDOCID");

                entity.Property(e => e.Repaymethod).HasColumnName("REPAYMETHOD");

                entity.Property(e => e.Startdate)
                    .HasColumnName("STARTDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Totalmonth).HasColumnName("TOTALMONTH");

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.TFinAccountExtLoan)
                    .HasForeignKey<TFinAccountExtLoan>(d => d.Accountid)
                    .HasConstraintName("FK_t_fin_account_ext_loan_ID");
            });

            modelBuilder.Entity<TFinAccountExtLoanH>(entity =>
            {
                entity.HasKey(e => e.Accountid);

                entity.ToTable("t_fin_account_ext_loan_h");

                entity.Property(e => e.Accountid)
                    .HasColumnName("ACCOUNTID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Annualrate)
                    .HasColumnName("ANNUALRATE")
                    .HasColumnType("decimal(17, 2)");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Interestfree).HasColumnName("INTERESTFREE");

                entity.Property(e => e.Others)
                    .HasColumnName("OTHERS")
                    .HasMaxLength(100);

                entity.Property(e => e.Partner)
                    .HasColumnName("PARTNER")
                    .HasMaxLength(50);

                entity.Property(e => e.Payingaccount).HasColumnName("PAYINGACCOUNT");

                entity.Property(e => e.Refdocid).HasColumnName("REFDOCID");

                entity.Property(e => e.Repaymethod).HasColumnName("REPAYMETHOD");

                entity.Property(e => e.Startdate)
                    .HasColumnName("STARTDATE")
                    .HasColumnType("datetime");

                entity.Property(e => e.Totalmonth).HasColumnName("TOTALMONTH");

                entity.HasOne(d => d.Account)
                    .WithOne(p => p.TFinAccountExtLoanH)
                    .HasForeignKey<TFinAccountExtLoanH>(d => d.Accountid)
                    .HasConstraintName("FK_t_fin_account_ext_loan_h_ID");
            });

            modelBuilder.Entity<TFinDocument>(entity =>
            {
                entity.ToTable("t_fin_document");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Desp)
                    .IsRequired()
                    .HasColumnName("DESP")
                    .HasMaxLength(45);

                entity.Property(e => e.Doctype).HasColumnName("DOCTYPE");

                entity.Property(e => e.Exgrate)
                    .HasColumnName("EXGRATE")
                    .HasColumnType("decimal(17, 4)");

                entity.Property(e => e.Exgrate2)
                    .HasColumnName("EXGRATE2")
                    .HasColumnType("decimal(17, 4)");

                entity.Property(e => e.ExgratePlan).HasColumnName("EXGRATE_PLAN");

                entity.Property(e => e.ExgratePlan2).HasColumnName("EXGRATE_PLAN2");

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Trancurr)
                    .IsRequired()
                    .HasColumnName("TRANCURR")
                    .HasMaxLength(5);

                entity.Property(e => e.Trancurr2)
                    .HasColumnName("TRANCURR2")
                    .HasMaxLength(5);

                entity.Property(e => e.Trandate)
                    .HasColumnName("TRANDATE")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TFinDocument)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_fin_document_HID");
            });

            modelBuilder.Entity<TFinDocumentItem>(entity =>
            {
                entity.HasKey(e => new { e.Docid, e.Itemid });

                entity.ToTable("t_fin_document_item");

                entity.Property(e => e.Docid).HasColumnName("DOCID");

                entity.Property(e => e.Itemid).HasColumnName("ITEMID");

                entity.Property(e => e.Accountid).HasColumnName("ACCOUNTID");

                entity.Property(e => e.Controlcenterid).HasColumnName("CONTROLCENTERID");

                entity.Property(e => e.Desp)
                    .HasColumnName("DESP")
                    .HasMaxLength(45);

                entity.Property(e => e.Orderid).HasColumnName("ORDERID");

                entity.Property(e => e.Tranamount)
                    .HasColumnName("TRANAMOUNT")
                    .HasColumnType("decimal(17, 2)");

                entity.Property(e => e.Trantype).HasColumnName("TRANTYPE");

                entity.Property(e => e.Usecurr2).HasColumnName("USECURR2");

                entity.HasOne(d => d.Doc)
                    .WithMany(p => p.TFinDocumentItem)
                    .HasForeignKey(d => d.Docid)
                    .HasConstraintName("FK_t_fin_document_header");
            });

            modelBuilder.Entity<TFinPlan>(entity =>
            {
                entity.ToTable("t_fin_plan");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Accountid).HasColumnName("ACCOUNTID");

                entity.Property(e => e.Acntctgyid).HasColumnName("ACNTCTGYID");

                entity.Property(e => e.Ccid).HasColumnName("CCID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(50);

                entity.Property(e => e.Desp)
                    .IsRequired()
                    .HasColumnName("DESP")
                    .HasMaxLength(50);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Ptype).HasColumnName("PTYPE");

                entity.Property(e => e.Startdate)
                    .HasColumnName("STARTDATE")
                    .HasColumnType("date");

                entity.Property(e => e.Tgtbal)
                    .HasColumnName("TGTBAL")
                    .HasColumnType("decimal(17, 2)");

                entity.Property(e => e.Tgtdate)
                    .HasColumnName("TGTDATE")
                    .HasColumnType("date");

                entity.Property(e => e.Trancurr)
                    .IsRequired()
                    .HasColumnName("TRANCURR")
                    .HasMaxLength(5);

                entity.Property(e => e.Ttid).HasColumnName("TTID");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(50);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TFinPlan)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_fin_plan_HID");
            });

            modelBuilder.Entity<TFinTmpdocDp>(entity =>
            {
                entity.HasKey(e => e.Docid);

                entity.ToTable("t_fin_tmpdoc_dp");

                entity.Property(e => e.Docid).HasColumnName("DOCID");

                entity.Property(e => e.Accountid).HasColumnName("ACCOUNTID");

                entity.Property(e => e.Controlcenterid).HasColumnName("CONTROLCENTERID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Desp)
                    .HasColumnName("DESP")
                    .HasMaxLength(45);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Orderid).HasColumnName("ORDERID");

                entity.Property(e => e.Refdocid).HasColumnName("REFDOCID");

                entity.Property(e => e.Tranamount)
                    .HasColumnName("TRANAMOUNT")
                    .HasColumnType("decimal(17, 2)");

                entity.Property(e => e.Trandate)
                    .HasColumnName("TRANDATE")
                    .HasColumnType("date");

                entity.Property(e => e.Trantype).HasColumnName("TRANTYPE");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TFinTmpdocDp)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_fin_tmpdocdp_HID");
            });

            modelBuilder.Entity<TFinTmpdocLoan>(entity =>
            {
                entity.HasKey(e => e.Docid);

                entity.ToTable("t_fin_tmpdoc_loan");

                entity.Property(e => e.Docid).HasColumnName("DOCID");

                entity.Property(e => e.Accountid).HasColumnName("ACCOUNTID");

                entity.Property(e => e.Controlcenterid).HasColumnName("CONTROLCENTERID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Desp)
                    .HasColumnName("DESP")
                    .HasMaxLength(45);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Interestamount)
                    .HasColumnName("INTERESTAMOUNT")
                    .HasColumnType("decimal(17, 2)");

                entity.Property(e => e.Orderid).HasColumnName("ORDERID");

                entity.Property(e => e.Refdocid).HasColumnName("REFDOCID");

                entity.Property(e => e.Tranamount)
                    .HasColumnName("TRANAMOUNT")
                    .HasColumnType("decimal(17, 2)");

                entity.Property(e => e.Trandate)
                    .HasColumnName("TRANDATE")
                    .HasColumnType("date");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TFinTmpdocLoan)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_fin_tmpdocloan_HID");
            });

            modelBuilder.Entity<THomemsg>(entity =>
            {
                entity.ToTable("t_homemsg");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Content)
                    .HasColumnName("CONTENT")
                    .HasMaxLength(50);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Readflag).HasColumnName("READFLAG");

                entity.Property(e => e.RevDel)
                    .HasColumnName("REV_DEL")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.SendDel)
                    .HasColumnName("SEND_DEL")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Senddate)
                    .HasColumnName("SENDDATE")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("TITLE")
                    .HasMaxLength(20);

                entity.Property(e => e.Userfrom)
                    .IsRequired()
                    .HasColumnName("USERFROM")
                    .HasMaxLength(50);

                entity.Property(e => e.Userto)
                    .IsRequired()
                    .HasColumnName("USERTO")
                    .HasMaxLength(50);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.THomemsg)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_homemsg_HID");
            });

            modelBuilder.Entity<TLearnCtgy>(entity =>
            {
                entity.ToTable("t_learn_ctgy");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Comment)
                    .HasColumnName("COMMENT")
                    .HasMaxLength(50);

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("NAME")
                    .HasMaxLength(45);

                entity.Property(e => e.Parid).HasColumnName("PARID");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLearnCtgy)
                    .HasForeignKey(d => d.Hid)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_learn_ctgy_HID");
            });

            modelBuilder.Entity<TLearnEnsent>(entity =>
            {
                entity.ToTable("t_learn_ensent");

                entity.HasIndex(e => new { e.Hid, e.Sentence })
                    .HasName("IX_Table_ensent")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(50);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Sentence)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(50);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLearnEnsent)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_Table_learn_ensent_HID");
            });

            modelBuilder.Entity<TLearnEnsentexp>(entity =>
            {
                entity.HasKey(e => new { e.SentId, e.ExpId });

                entity.ToTable("t_learn_ensentexp");

                entity.Property(e => e.SentId).HasColumnName("SentID");

                entity.Property(e => e.ExpId).HasColumnName("ExpID");

                entity.Property(e => e.ExpDetail)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LangKey)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.HasOne(d => d.Sent)
                    .WithMany(p => p.TLearnEnsentexp)
                    .HasForeignKey(d => d.SentId)
                    .HasConstraintName("FK_Table_ensentexp_SentID");
            });

            modelBuilder.Entity<TLearnEnsentWord>(entity =>
            {
                entity.HasKey(e => new { e.SentId, e.WordId });

                entity.ToTable("t_learn_ensent_word");

                entity.Property(e => e.SentId).HasColumnName("SentID");

                entity.Property(e => e.WordId).HasColumnName("WordID");

                entity.HasOne(d => d.Sent)
                    .WithMany(p => p.TLearnEnsentWord)
                    .HasForeignKey(d => d.SentId)
                    .HasConstraintName("FK_t_learn_ensent_word_sent");
            });

            modelBuilder.Entity<TLearnEnword>(entity =>
            {
                entity.ToTable("t_learn_enword");

                entity.HasIndex(e => new { e.Hid, e.Word })
                    .HasName("IX_t_learn_enword")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(50);

                entity.Property(e => e.Word)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLearnEnword)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_Table_learn_word_HID");
            });

            modelBuilder.Entity<TLearnEnwordexp>(entity =>
            {
                entity.HasKey(e => new { e.Wordid, e.ExpId });

                entity.ToTable("t_learn_enwordexp");

                entity.Property(e => e.Wordid).HasColumnName("WORDID");

                entity.Property(e => e.ExpId).HasColumnName("ExpID");

                entity.Property(e => e.ExpDetail)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LangKey)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Posabb)
                    .HasColumnName("POSAbb")
                    .HasMaxLength(10);

                entity.HasOne(d => d.Word)
                    .WithMany(p => p.TLearnEnwordexp)
                    .HasForeignKey(d => d.Wordid)
                    .HasConstraintName("FK_Table_learn_wordexp_word");
            });

            modelBuilder.Entity<TLearnHist>(entity =>
            {
                entity.HasKey(e => new { e.Hid, e.Userid, e.Objectid, e.Learndate });

                entity.ToTable("t_learn_hist");

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Userid)
                    .HasColumnName("USERID")
                    .HasMaxLength(40);

                entity.Property(e => e.Objectid).HasColumnName("OBJECTID");

                entity.Property(e => e.Learndate)
                    .HasColumnName("LEARNDATE")
                    .HasColumnType("date");

                entity.Property(e => e.Comment)
                    .HasColumnName("COMMENT")
                    .HasMaxLength(45);

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLearnHist)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_learn_hist_HID");
            });

            modelBuilder.Entity<TLearnObj>(entity =>
            {
                entity.ToTable("t_learn_obj");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Category).HasColumnName("CATEGORY");

                entity.Property(e => e.Content).HasColumnName("CONTENT");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(45);

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLearnObj)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_learn_obj_HID");
            });

            modelBuilder.Entity<TLearnQtnBank>(entity =>
            {
                entity.ToTable("t_learn_qtn_bank");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BriefAnswer).HasMaxLength(200);

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(50);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Question)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(50);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLearnQtnBank)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_learn_qtn_bank_HID");
            });

            modelBuilder.Entity<TLearnQtnBankSub>(entity =>
            {
                entity.HasKey(e => new { e.Qtnid, e.Subitem });

                entity.ToTable("t_learn_qtn_bank_sub");

                entity.Property(e => e.Qtnid).HasColumnName("QTNID");

                entity.Property(e => e.Subitem)
                    .HasColumnName("SUBITEM")
                    .HasMaxLength(20);

                entity.Property(e => e.Detail)
                    .IsRequired()
                    .HasColumnName("DETAIL")
                    .HasMaxLength(200);

                entity.Property(e => e.Others)
                    .HasColumnName("OTHERS")
                    .HasMaxLength(50);

                entity.HasOne(d => d.Qtn)
                    .WithMany(p => p.TLearnQtnBankSub)
                    .HasForeignKey(d => d.Qtnid)
                    .HasConstraintName("FK_t_learn_qtn_bank_sub_QTNID");
            });

            modelBuilder.Entity<TLibBook>(entity =>
            {
                entity.ToTable("t_lib_book");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.EnglishName).HasMaxLength(50);

                entity.Property(e => e.ExtLink1).HasMaxLength(100);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.NativeName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ShortIntro).HasMaxLength(100);

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.CtgyNavigation)
                    .WithMany(p => p.TLibBook)
                    .HasForeignKey(d => d.Ctgy)
                    .HasConstraintName("FK_t_lib_book_ctgy");

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLibBook)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_lib_book_HID");
            });

            modelBuilder.Entity<TLibBookCtgy>(entity =>
            {
                entity.ToTable("t_lib_book_ctgy");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Others).HasMaxLength(50);

                entity.Property(e => e.ParId).HasColumnName("ParID");

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLibBookCtgy)
                    .HasForeignKey(d => d.Hid)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_t_lib_book_ctgy_HID");
            });

            modelBuilder.Entity<TLibLocation>(entity =>
            {
                entity.ToTable("t_lib_location");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.Desp).HasMaxLength(50);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLibLocation)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_lib_location_HID");
            });

            modelBuilder.Entity<TLibLocationDetail>(entity =>
            {
                entity.HasKey(e => new { e.Locid, e.Seqno });

                entity.ToTable("t_lib_location_detail");

                entity.Property(e => e.Locid).HasColumnName("LOCID");

                entity.Property(e => e.Seqno).HasColumnName("SEQNO");

                entity.Property(e => e.Contentid).HasColumnName("CONTENTID");

                entity.Property(e => e.Contenttype).HasColumnName("CONTENTTYPE");

                entity.Property(e => e.Others).HasMaxLength(50);

                entity.HasOne(d => d.Loc)
                    .WithMany(p => p.TLibLocationDetail)
                    .HasForeignKey(d => d.Locid)
                    .HasConstraintName("FK_t_lib_locationdet_LOCID");
            });

            modelBuilder.Entity<TLibPerson>(entity =>
            {
                entity.ToTable("t_lib_person");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Createdat)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Createdby)
                    .HasColumnName("CREATEDBY")
                    .HasMaxLength(40);

                entity.Property(e => e.EnglishName).HasMaxLength(50);

                entity.Property(e => e.ExtLink1).HasMaxLength(100);

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.NativeName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ShortIntro).HasMaxLength(100);

                entity.Property(e => e.Updatedat)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Updatedby)
                    .HasColumnName("UPDATEDBY")
                    .HasMaxLength(40);

                entity.HasOne(d => d.H)
                    .WithMany(p => p.TLibPerson)
                    .HasForeignKey(d => d.Hid)
                    .HasConstraintName("FK_t_lib_person_t_lib_HID");
            });

            modelBuilder.Entity<TTag>(entity =>
            {
                entity.HasKey(e => new { e.Hid, e.TagType, e.TagId, e.Term, e.TagSubId });

                entity.ToTable("t_tag");

                entity.Property(e => e.Hid).HasColumnName("HID");

                entity.Property(e => e.TagId).HasColumnName("TagID");

                entity.Property(e => e.Term).HasMaxLength(50);

                entity.Property(e => e.TagSubId).HasColumnName("TagSubID");
            });
        }
    }
}
