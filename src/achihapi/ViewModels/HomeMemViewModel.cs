using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class HomeMemViewModel: BaseViewModel
    {
        [Required]
        public Int32 HomeID { get; set; }
        [Required]
        [MaxLength(50)]
        public String User { get; set; }
        [MaxLength(40)]
        public String UserID { get; set; }
        [MaxLength(10)]
        public String Priv_LearnObject { get; set; }
        [MaxLength(10)]
        public String Priv_LearnHist { get; set; }
        [MaxLength(10)]
        public String Priv_LearnAward { get; set; }
        [MaxLength(10)]
        public String Priv_LearnPlan { get; set; }
        [MaxLength(10)]
        public String Priv_LearnCategory { get; set; }
        [MaxLength(10)]
        public String Priv_FinanceSetting { get; set; }
        [MaxLength(10)]
        public String Priv_FinanceCurrency { get; set; }
        [MaxLength(10)]
        public String Priv_FinanceAccount { get; set; }
        [MaxLength(10)]
        public String Priv_FinanceDocument { get; set; }
        [MaxLength(10)]
        public String Priv_FinanceControlCenter { get; set; }
        [MaxLength(10)]
        public String Priv_FinanceOrder { get; set; }
        [MaxLength(10)]
        public String Priv_FinanceReport { get; set; }
        [MaxLength(10)]
        public String Priv_Event { get; set; }
        [MaxLength(10)]
        public String Priv_LibBook { get; set; }
        [MaxLength(10)]
        public String Priv_LibMovie { get; set; }        
    }
}
