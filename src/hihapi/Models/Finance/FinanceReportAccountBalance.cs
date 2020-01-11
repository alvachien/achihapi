using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    public class FinanceReportAccountBalance
    {
        [Column("HID", TypeName = "INT")]
        public Int32 HomeID { get; set; }

        [Column("ACCOUNTID", TypeName = "INT")]
        public Int32 AccountID { get; set; }

        [Column("ACCOUNTNAME", TypeName = "NVARCHAR(50)")]
        public string AccountName { get; set; }

        [Column("ACCOUNTCTGYID", TypeName = "INT")]
        public Int32 AccountCategoryID { get; set; }

        [Column("ACCOUNTCTGYNAME", TypeName = "NVARCHAR(50)")]
        public string AccountCategoryName { get; set; }

        [Column("DEBIT_BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal DebitBalance { get; set; }

        [Column("CREDIT_BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal CreditBalance { get; set; }

        [Column("BALANCE", TypeName = "Decimal(17, 2)")]
        public Decimal Balance { get; set; }
    }
}
