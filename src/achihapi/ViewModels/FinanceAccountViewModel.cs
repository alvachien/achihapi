﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class FinanceAccountViewModel : BaseViewModel
    {
        public Int32 ID { get; set; }
        public Int32 CtgyID { get; set; }
        [StringLength(30)]
        public String Name { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
        [StringLength(40)]
        public String Owner { get; set; }
    }

    public class FinanceAccountExtDPViewModel
    {
        public Int32 AccountID { get; set; }
        public Boolean Direct { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Byte RptType { get; set; }
        public Int32 RefDocID { get; set; }
        [StringLength(100)]
        public String DefrrDays { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }
    }
}