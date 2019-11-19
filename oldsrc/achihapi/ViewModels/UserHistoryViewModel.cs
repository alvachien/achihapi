using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class UserHistoryViewModel
    {
        [Required]
        [StringLength(40)]
        public String UserID { get; set; }
        public Int32 SeqNo { get; set; }
        [Required]
        public Byte HistType { get; set; }
        public DateTime TimePoint { get; set; }
        [StringLength(50)]
        public String Others { get; set; }
    }
}
