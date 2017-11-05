using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LearnQuestionBankViewModel: BaseViewModel
    {
        public Int32 ID { get; set; }
        [Required]
        public Int32 HID { get; set; }
        [Required]
        public Byte QuestionType { get; set; }
        [Required]
        [StringLength(100)]
        public String Question { get; set; }
        [StringLength(100)]
        public String BriefAnswer { get; set; }

        public List<String> TagTerms { get; }

        public List<LearnQuestionBankSubItemViewModel> SubItemList { get; }

        public LearnQuestionBankViewModel() : base()
        {
            SubItemList = new List<LearnQuestionBankSubItemViewModel>();
            TagTerms = new List<String>();
        }
    }

    public sealed class LearnQuestionBankSubItemViewModel
    {
        [Required]
        [StringLength(20)]
        public String SubItem { get; set; }
        [Required]
        [StringLength(100)]
        public String Detail { get; set; }
        [StringLength(50)]
        public String Others { get; set; }
    }
}
