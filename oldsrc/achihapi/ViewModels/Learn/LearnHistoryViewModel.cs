using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace achihapi.ViewModels
{
    public class LearnHistoryViewModel : BaseViewModel
    {
        [Required]
        public Int32 HID { get; set; }
        [Required]
        [StringLength(40)]
        public String UserID { get; set; }
        [Required]
        public Int32 ObjectID { get; set; }
        [Required]
        public DateTime LearnDate { get; set; }
        [StringLength(45)]
        public String Comment { get; set; }

        public string GeneratedKey()
        {
            return this.HID.ToString() + "_" + this.UserID + "_" + this.ObjectID.ToString() + "_" + String.Format("0:yyyy-MM-dd", this.LearnDate);
        }

        public Boolean ParseGeneratedKey(String strKey)
        {
            String[] arKeys = strKey.Split('_');
            if (arKeys.Length != 4)
                return false;

            try
            {
                this.HID = Int32.Parse(arKeys[0]);
                this.UserID = arKeys[1];
                this.ObjectID = Int32.Parse(arKeys[2]);
                this.LearnDate = DateTime.Parse(arKeys[3]);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }

    public class LearnHistoryUIViewModel : LearnHistoryViewModel
    {
        public string UserDisplayAs { get; set; }
        public string ObjectName { get; set; }
    }
}
