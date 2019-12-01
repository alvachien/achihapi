using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models
{
    [Table("T_FIN_CONTROLCENTER")]
    public class FinanceControlCenter: BaseModel
    {
        [Key]
        [Column("ID", TypeName="INT")]
        public Int32 ID { get; set; }

        [Required]
        [Column("HID", TypeName="INT")]
        public Int32 HID { get; set; }

        [Required]
        [StringLength(30)]
        public String Name { get; set; }

        public Int32? ParID { get; set; }

        [StringLength(45)]
        public String Comment { get; set; }

        [StringLength(40)]
        public String Owner { get; set; }
    }
}
