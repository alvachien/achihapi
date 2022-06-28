using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.OData.Edm;

namespace hihapi.Models
{
    public class FinanceDocumentItemView
    {
        [Key]
        [Column("DOCID", TypeName = "INT")]
        public Int32 DocumentID { get; set; }

        [Key]
        [Column("ITEMID", TypeName = "INT")]
        public Int32 ItemID { get; set; }

        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("TRANDATE", TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [Column("DOCDESP", TypeName = "nvarchar(50)")]
        public string DocumentDesp { get; set; }

        [Column("ACCOUNTID", TypeName = "INT")]
        public Int32 AccountID { get; set; }

        [Column("TRANTYPE", TypeName = "INT")]
        public Int32 TransactionType { get; set; }

        [Column("TRANTYPENAME", TypeName = "nvarchar(50)")]
        public string TransactionTypeName { get; set; }

        [Column("TRANTYPE_EXP", TypeName = "BIT")]
        public Boolean IsExpense { get; set; }

        [Column("TRANCURR", TypeName = "nvarchar(5)")]
        public string Currency { get; set; }

        [Column("TRANAMOUNT_ORG", TypeName = "Decimal(17, 2)")]
        public Decimal OriginAmount { get; set; }

        [Column("TRANAMOUNT", TypeName = "Decimal(17, 2)")]
        public Decimal Amount { get; set; }

        [Column("TRANAMOUNT_LC", TypeName = "Decimal(17, 2)")]
        public Decimal AmountInLocalCurrency { get; set; }

        [Column("CONTROLCENTERID", TypeName = "INT")]
        public Int32? ControlCenterID { get; set; }
        [Column("ORDERID", TypeName = "INT")]
        public Int32? OrderID { get; set; }

        [Column("DESP", TypeName = "nvarchar(50)")]
        public string ItemDesp { get; set; }
    }

    #region Account report
    public class FinanceReporAccountGroupView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("ACCOUNTID", TypeName = "INT")]
        public Int32 AccountID { get; set; }

        [Column("BALANCE_LC", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public class FinanceReporAccountGroupAndExpenseView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("ACCOUNTID", TypeName = "INT")]
        public Int32 AccountID { get; set; }

        [Column("TRANTYPE_EXP", TypeName = "BIT")]
        public Boolean IsExpense { get; set; }

        [Column("BALANCE_LC", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public class FinanceReportAccountBalanceView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("ACCOUNTID", TypeName = "INT")]
        public Int32 AccountID { get; set; }

        [Column("DEBIT_BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal DebitBalance { get; set; }

        [Column("CREDIT_BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal CreditBalance { get; set; }

        [Column("BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public class FinanceReportByAccount : FinanceReportAccountBalanceView
    {
    }

    public sealed class FinanceReportByAccountMOM : FinanceReportByAccount
    {
        public Int32 Month { get; set; }
    }

    //public sealed class FinanceReportByAccountMOMResult
    //{
    //    public List<FinanceReportByAccountMOM> ReportData { get; set; }
    //    public List<String> Months { get; set; }
    //}
    #endregion

    #region Control center report
    public class FinanceReportControlCenterGroupView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("CONTROLCENTERID", TypeName = "INT")]
        public Int32 ControlCenterID { get; set; }

        [Column("BALANCE_LC", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public class FinanceReportControlCenterGroupAndExpenseView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("CONTROLCENTERID", TypeName = "INT")]
        public Int32 ControlCenterID { get; set; }

        [Column("TRANTYPE_EXP", TypeName = "BIT")]
        public Boolean IsExpense { get; set; }

        [Column("BALANCE_LC", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public class FinanceReportControlCenterBalanceView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("CONTROLCENTERID", TypeName = "INT")]
        public Int32 ControlCenterID { get; set; }

        [Column("DEBIT_BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal DebitBalance { get; set; }

        [Column("CREDIT_BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal CreditBalance { get; set; }

        [Column("BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public class FinanceReportByControlCenter : FinanceReportControlCenterBalanceView
    {
    }

    public class FinanceReportByControlCenterMOM : FinanceReportByControlCenter
    {
        public Int32 Month { get; set; }
    }
    #endregion

    #region Order report
    public class FinanceReportOrderGroupView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("ORDERID", TypeName = "INT")]
        public Int32 OrderID { get; set; }

        [Column("BALANCE_LC", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public class FinanceReportOrderGroupAndExpenseView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("ORDERID", TypeName = "INT")]
        public Int32 OrderID { get; set; }

        [Column("TRANTYPE_EXP", TypeName = "BIT")]
        public Boolean IsExpense { get; set; }

        [Column("BALANCE_LC", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public class FinanceReportOrderBalanceView
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("ORDERID", TypeName = "INT")]
        public Int32 OrderID { get; set; }

        [Column("DEBIT_BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal DebitBalance { get; set; }

        [Column("CREDIT_BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal CreditBalance { get; set; }

        [Column("BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }

    public sealed class FinanceReportByOrder : FinanceReportOrderBalanceView
    {
    }
    #endregion

    #region Finance Report
    public class FinanceReport
    {
        [Key]
        public Int32 HomeID { get; set; }

        public Decimal InAmount { get; set; }
        public Decimal OutAmount { get; set; }
    }

    public class FinanceReportMOM: FinanceReport
    {
        public Int32 Month { get; set; }
    }

    public class FinanceReportPerDate: FinanceReport
    {
        public DateTime TransactionDate { get; set; }
    }
    #endregion

    #region Transaction type report
    public class FinanceReportByTransactionType : FinanceReport
    {
        public Int32 TransactionType { get; set; }

        public string TransactionTypeName { get; set; }
    }

    public class FinanceReportByTransactionTypeMOM : FinanceReportByTransactionType
    {
        public Int32 Month { get; set; }
    }

    //public sealed class FinanceReportByTransactionTypeMOMResult
    //{
    //    public List<FinanceReportByTransactionTypeMOM> ReportData { get; set; }
    //    public List<String> Months { get; set; }
    //}
    #endregion

    #region Finance Overview KPI
    public class FinanceOverviewKeyFigure
    {
        public Int32 HomeID { get; set; }
        public String Currency { get; set; }
        public Decimal CurrentMonthIncome { get; set; }
        public Decimal CurrentMonthOutgo { get; set; }
        public Decimal LastMonthIncome { get; set; }
        public Decimal LastMonthOutgo { get; set; }
        public Decimal IncomeYTD { get; set; }
        public Decimal OutgoYTD { get; set; }

        public Decimal CurrentMonthIncomePrecentage { get; set; }
        public Decimal CurrentMonthOutgoPrecentage { get; set; }
    }
    #endregion
}
