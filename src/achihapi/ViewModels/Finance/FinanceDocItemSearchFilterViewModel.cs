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

    public enum GeneralFilterValueEnum
    {
        Number = 1,
        String = 2,
        Date   = 3
    }

    public class GeneralFilterItem
    {
        public string FieldName { get; set; }
        public GeneralFilterOperatorEnum Operator { get; set; }
        public string LowValue { get; set; }
        public string HighValue { get; set; }
        public GeneralFilterValueEnum ValueType { get; set; }

        public GeneralFilterItem()
        {
            this.Operator = GeneralFilterOperatorEnum.Equal;
            this.ValueType = GeneralFilterValueEnum.String; // Default is string
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

            String strRst = " ";
            switch (this.Operator)
            {
                case GeneralFilterOperatorEnum.Equal:
                    {
                        switch(this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " = " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " = " + LowValue;
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " = N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.NotEqual:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " <> " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " <> " + LowValue;
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " <> N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.LargerEqual:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " >= " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " >= " + LowValue;
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " >= N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.LargerThan:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " > " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " > " + LowValue;
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " > N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.LessEqual:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " <= " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " <= " + LowValue;
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " <= N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.LessThan:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " < " + LowValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " < " + LowValue;
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " < N'" + LowValue + "' ";
                                break;
                        }
                    }
                    break;

                case GeneralFilterOperatorEnum.Between:
                    {
                        switch (this.ValueType)
                        {
                            case GeneralFilterValueEnum.Number:
                                strRst = FieldName + " >= " + LowValue + " AND " + FieldName + " <= " + HighValue;
                                break;

                            case GeneralFilterValueEnum.Date:
                                strRst = FieldName + " >= " + LowValue + " AND " + FieldName + " <= " + HighValue;
                                break;

                            case GeneralFilterValueEnum.String:
                            default:
                                strRst = FieldName + " >= N'" + LowValue + "' AND " + FieldName + " <= N'" + HighValue +"' ";
                                break;
                        }
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
