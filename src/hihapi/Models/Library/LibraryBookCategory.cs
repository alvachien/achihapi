﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hihapi.Models.Library
{
    public enum LibraryBookCategoryEnum
    {
        OwnDefined = 0,
        Novel = 1
    }

    //[Table("T_LIB_BOOKCTGY_DEF")]
    public class LibraryBookCategory
    {
        [Key]
        [Required]
        [Column("ID", TypeName = "INT")]
        public int Id { get; set; }

        [Column("HID", TypeName = "INT")]
        public Int32? HomeID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("NAME", TypeName = "NVARCHAR(30)")]
        public String Name { get; set; }

        [StringLength(45)]
        [Column("VALUE", TypeName = "INT")]
        public LibraryBookCategoryEnum CategoryValue { get; set; }

        public HomeDefine CurrentHome { get; set; }

        public IList<LibraryBook> Books { get; set; }
    }
}
