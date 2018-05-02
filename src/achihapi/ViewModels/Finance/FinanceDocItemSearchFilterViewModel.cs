using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public enum GeneralFilterOperatorEnum
    {
        Equal       = 1,
        NotEqual    = 2,
        Between     = 3,
        LargerThan  = 4,
        LargerEqual = 5,
        LessThan    = 6,
        LessEqual   = 7,
    }

    public class GeneralFilterItem
    {
        public string FieldName { get; set; }
        public GeneralFilterOperatorEnum Operator { get; set; }
        public string LowValue { get; set; }
        public string HighValue { get; set; }

        public GeneralFilterItem()
        {
            this.Operator = GeneralFilterOperatorEnum.Equal;
        }

        public Boolean IsValid()
        {
            if (String.IsNullOrEmpty(FieldName))
                return false;

            switch(this.Operator)
            {
                case GeneralFilterOperatorEnum.Equal:
                case GeneralFilterOperatorEnum.NotEqual:
                case GeneralFilterOperatorEnum.LargerEqual:
                case GeneralFilterOperatorEnum.LargerThan:
                case GeneralFilterOperatorEnum.LessEqual:
                case GeneralFilterOperatorEnum.LessThan:
                    {
                        if (String.IsNullOrEmpty(LowValue))
                            return false;
                    }
                    break;

                case GeneralFilterOperatorEnum.Between:
                    {
                        if (String.IsNullOrEmpty(LowValue))
                            return false;
                        if (String.IsNullOrEmpty(HighValue))
                            return false;
                    }
                    break;

                default:
                    return false;
            }

            return true;
        }

        public string GenerateSql()
        {
            if (!this.IsValid())
                return String.Empty;

            String strRst = "";
            switch (this.Operator)
            {
                case GeneralFilterOperatorEnum.Equal:
                    {
                        strRst = FieldName + " = " + LowValue;
                    }
                    break;

                case GeneralFilterOperatorEnum.NotEqual:
                    {
                        strRst = FieldName + " <> " + LowValue;
                    }
                    break;

                case GeneralFilterOperatorEnum.LargerEqual:
                    {
                        strRst = FieldName + " >= " + LowValue;
                    }
                    break;

                case GeneralFilterOperatorEnum.LargerThan:
                    {
                        strRst = FieldName + " > " + LowValue;
                    }
                    break;

                case GeneralFilterOperatorEnum.LessEqual:
                    {
                        strRst = FieldName + " <= " + LowValue;
                    }
                    break;

                case GeneralFilterOperatorEnum.LessThan:
                    {
                        strRst = FieldName + " < " + LowValue;
                    }
                    break;

                case GeneralFilterOperatorEnum.Between:
                    {
                        strRst = FieldName + " >= " + LowValue + " AND " + FieldName + " <= " + HighValue;
                    }
                    break;

                default:
                    break;
            }

            return strRst;
        }
    }

    public class FinanceDocItemSearchFilterViewModel
    {
        public List<GeneralFilterItem> FieldList { get; private set; }

        public FinanceDocItemSearchFilterViewModel()
        {
            this.FieldList = new List<GeneralFilterItem>();
        }
    }
}
