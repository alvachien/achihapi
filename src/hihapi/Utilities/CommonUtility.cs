using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using hihapi.Exceptions;
using hihapi.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace hihapi.Utilities
{
    internal class HIHAPIConstants
    {
        public const String OnlyOwnerAndDispaly = "OnlyOwnerAndDisplay";
        public const String OnlyOwnerFullControl = "OnlyOwnerFullControl";
        public const String OnlyOwner = "OnlyOwner";
        public const String Display = "Display";
        public const String All = "All";

        internal const String HomeDefScope = "HomeDefScope";
        internal const String FinanceAccountScope = "FinanceAccountScope";
        internal const String FinanceDocumentScope = "FinanceDocumentScope";
        internal const String LearnHistoryScope = "LearnHistoryScope";
        internal const String LearnObjectScope = "LearnObjectScope";

        internal const String DateFormatPattern = "yyyy-MM-dd";
    }

    internal static class HIHAPIUtility
    {
        internal static void HandleModalStateError(ModelStateDictionary modelState)
        {
            string strModalError = "";
            foreach (var value in modelState.Values)
            {
                foreach (var err in value.Errors)
                {
                    strModalError = err.Exception != null ? err.Exception.Message : err.ErrorMessage;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Modal State Failed: " + strModalError);
#endif
                }
            }

            throw new BadRequestException("Modal State Failed: " + strModalError);
        }

        internal static String GetUserID(Microsoft.AspNetCore.Mvc.ControllerBase ctrl)
        {
            if (ctrl.User != null)
                return ctrl.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return String.Empty;
        }
    }

    public class CommonUtility
    {
        public static List<RepeatedDates> WorkoutRepeatedDates(RepeatDatesCalculationInput datInput)
        {
            List<RepeatedDates> listResults = new List<RepeatedDates>();

            // Input checks
            if (datInput == null)
                throw new Exception("Input the data!");
            var dtEnd = new DateTime(datInput.EndDate.Year, datInput.EndDate.Month, datInput.EndDate.Day);
            var dtStart = new DateTime(datInput.StartDate.Year, datInput.StartDate.Month, datInput.StartDate.Day);
            if (dtEnd < dtStart)
                throw new Exception("Invalid data range");

            switch (datInput.RepeatType)
            {
                case RepeatFrequency.Day:
                    {
                        var tspans = dtEnd - dtStart;
                        var tdays = (Int32)tspans.Days;

                        for (int i = 0; i <= tdays; i++)
                        {
                            listResults.Add(new RepeatedDates
                            {
                                StartDate = dtStart.AddDays(i),
                            });
                        }

                        for (int i = 0; i < listResults.Count; i++)
                        {
                            listResults[i].EndDate = listResults[i].StartDate;
                        }
                    }
                    break;

                case RepeatFrequency.Fortnight:
                    {
                        var tspans = dtEnd - dtStart;
                        var tdays = (Int32)tspans.Days;

                        var tfortnights = tdays / 14;

                        for (int i = 0; i <= tfortnights; i++)
                        {
                            listResults.Add(new RepeatedDates
                            {
                                StartDate = datInput.StartDate.AddDays(i * 14),
                            });
                        }

                        for (int i = 0; i < listResults.Count; i++)
                        {
                            if (i == listResults.Count - 1)
                            {
                                listResults[i].EndDate = listResults[i].StartDate.AddDays(13);
                            }
                            else
                            {
                                listResults[i].EndDate = listResults[i + 1].StartDate.AddDays(-1);
                            }
                        }
                    }
                    break;

                case RepeatFrequency.HalfYear:
                    {
                        var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);
                        var nhalfyear = nmonths / 6;

                        for (int i = 0; i <= nhalfyear; i++)
                        {
                            listResults.Add(new RepeatedDates
                            {
                                StartDate = datInput.StartDate.AddMonths(i * 6),
                            });
                        }

                        for (int i = 0; i < listResults.Count; i++)
                        {
                            if (i == listResults.Count - 1)
                            {
                                listResults[i].EndDate = listResults[i].StartDate.AddMonths(6);
                            }
                            else
                            {
                                listResults[i].EndDate = listResults[i + 1].StartDate.AddDays(-1);
                            }
                        }
                    }
                    break;

                case RepeatFrequency.Month:
                    {
                        var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);

                        for (int i = 0; i <= nmonths; i++)
                        {
                            listResults.Add(new RepeatedDates
                            {
                                StartDate = datInput.StartDate.AddMonths(i),
                            });
                        }

                        for (int i = 0; i < listResults.Count; i++)
                        {
                            if (i == listResults.Count - 1)
                            {
                                listResults[i].EndDate = listResults[i].StartDate.AddMonths(1).AddDays(-1);
                            }
                            else
                            {
                                listResults[i].EndDate = listResults[i + 1].StartDate.AddDays(-1);
                            }
                        }
                    }
                    break;

                case RepeatFrequency.Quarter:
                    {
                        var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);
                        var nquarters = nmonths / 3;

                        for (int i = 0; i <= nquarters; i++)
                        {
                            listResults.Add(new RepeatedDates
                            {
                                StartDate = datInput.StartDate.AddMonths(i * 3),
                            });
                        }

                        for (int i = 0; i < listResults.Count; i++)
                        {
                            if (i == listResults.Count - 1)
                            {
                                listResults[i].EndDate = listResults[i].StartDate.AddMonths(3).AddDays(-1);
                            }
                            else
                            {
                                listResults[i].EndDate = listResults[i + 1].StartDate.AddDays(-1);
                            }
                        }
                    }
                    break;

                case RepeatFrequency.Week:
                    {
                        var tspans = dtEnd - dtStart;
                        var tdays = (Int32)tspans.Days;

                        var tweeks = tdays / 7;

                        for (int i = 0; i <= tweeks; i++)
                        {
                            listResults.Add(new RepeatedDates
                            {
                                StartDate = datInput.StartDate.AddDays(i * 7),
                            });
                        }

                        for (int i = 0; i < listResults.Count; i++)
                        {
                            if (i == listResults.Count - 1)
                            {
                                listResults[i].EndDate = listResults[i].StartDate.AddDays(6);
                            }
                            else
                            {
                                listResults[i].EndDate = listResults[i + 1].StartDate.AddDays(-1);
                            }
                        }
                    }
                    break;

                case RepeatFrequency.Year:
                    {
                        var nyears = datInput.EndDate.Year - datInput.StartDate.Year;

                        for (int i = 0; i <= nyears; i++)
                        {
                            listResults.Add(new RepeatedDates
                            {
                                StartDate = datInput.StartDate.AddYears(i),
                            });
                        }

                        for (int i = 0; i < listResults.Count; i++)
                        {
                            if (i == listResults.Count - 1)
                            {
                                listResults[i].EndDate = listResults[i].StartDate.AddYears(1).AddDays(-1);
                            }
                            else
                            {
                                listResults[i].EndDate = listResults[i + 1].StartDate.AddDays(-1);
                            }
                        }
                    }
                    break;

                case RepeatFrequency.Manual:
                    {
                        // It shall return only entry out
                        listResults.Add(new RepeatedDates
                        {
                            StartDate = datInput.StartDate,
                            EndDate = datInput.EndDate
                        });
                    }
                    break;

                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            return listResults;
        }
 
        public static List<RepeatedDatesWithAmount> WorkoutRepeatedDatesWithAmount(RepeatDatesWithAmountCalculationInput datInput)
        {
            List<RepeatedDatesWithAmount> listResults = new List<RepeatedDatesWithAmount>();

            // Input checks
            if (datInput == null)
                throw new Exception("Input the data!");
            var dtEnd = new DateTime(datInput.EndDate.Year, datInput.EndDate.Month, datInput.EndDate.Day);
            var dtStart = new DateTime(datInput.StartDate.Year, datInput.StartDate.Month, datInput.StartDate.Day);
            if (dtEnd < dtStart)
                throw new Exception("Invalid data range");
            if (datInput.TotalAmount <= 0)
                throw new Exception("Invalid total amount");
            if (String.IsNullOrEmpty(datInput.Desp))
                throw new Exception("Invalid desp");

            switch (datInput.RepeatType)
            {
                case RepeatFrequency.Day:
                    {
                        var tspans = dtEnd - dtStart;
                        var tdays = (Int32)tspans.Days;

                        var tamt = Math.Round(datInput.TotalAmount / tdays, 2);
                        for (int i = 0; i < tdays; i++)
                        {
                            listResults.Add(new RepeatedDatesWithAmount
                            {
                                TranDate = datInput.StartDate.AddDays(i),
                                TranAmount = tamt,
                                Desp = datInput.Desp + " | " + (i + 1).ToString() + " / " + tdays.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Fortnight:
                    {
                        var tspans = dtEnd - dtStart;
                        var tdays = (Int32)tspans.Days;

                        var tfortnights = tdays / 14;
                        var tamt = Math.Round(datInput.TotalAmount / tfortnights, 2);

                        for (int i = 0; i < tfortnights; i++)
                        {
                            listResults.Add(new RepeatedDatesWithAmount
                            {
                                TranDate = datInput.StartDate.AddDays(i * 14),
                                TranAmount = tamt,
                                Desp = datInput.Desp + " | " + (i + 1).ToString() + " / " + tfortnights.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.HalfYear:
                    {
                        var tspans = dtEnd - dtStart;
                        var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);
                        var nhalfyear = nmonths / 6;
                        var tamt = Math.Round(datInput.TotalAmount / nhalfyear, 2);

                        for (int i = 0; i < nhalfyear; i++)
                        {
                            listResults.Add(new RepeatedDatesWithAmount
                            {
                                TranDate = datInput.StartDate.AddMonths(i * 6),
                                TranAmount = tamt,
                                Desp = datInput.Desp + " | " + (i + 1).ToString() + " / " + nhalfyear.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Month:
                    {
                        var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);

                        var tamt = Math.Round(datInput.TotalAmount / nmonths, 2);

                        for (int i = 0; i < nmonths; i++)
                        {
                            listResults.Add(new RepeatedDatesWithAmount
                            {
                                TranDate = datInput.StartDate.AddMonths(i),
                                TranAmount = tamt,
                                Desp = datInput.Desp + " | " + (i + 1).ToString() + " / " + nmonths.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Quarter:
                    {
                        var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);
                        var nquarters = nmonths / 3;
                        var tamt = Math.Round(datInput.TotalAmount / nquarters, 2);

                        for (int i = 0; i < nquarters; i++)
                        {
                            listResults.Add(new RepeatedDatesWithAmount
                            {
                                TranDate = datInput.StartDate.AddMonths(i * 3),
                                TranAmount = tamt,
                                Desp = datInput.Desp + " | " + (i + 1).ToString() + " / " + nquarters.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Week:
                    {
                        var tspans = dtEnd - dtStart;
                        var tdays = (Int32)tspans.Days;

                        var tweeks = tdays / 7;
                        var tamt = Math.Round(datInput.TotalAmount / tweeks, 2);

                        for (int i = 0; i < tweeks; i++)
                        {
                            listResults.Add(new RepeatedDatesWithAmount
                            {
                                TranDate = datInput.StartDate.AddDays(i * 7),
                                TranAmount = tamt,
                                Desp = datInput.Desp + " | " + (i + 1).ToString() + " / " + tweeks.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Year:
                    {
                        var nyears = datInput.EndDate.Year - datInput.StartDate.Year;

                        var tamt = Math.Round(datInput.TotalAmount / nyears, 2);

                        for (int i = 0; i < nyears; i++)
                        {
                            listResults.Add(new RepeatedDatesWithAmount
                            {
                                TranDate = datInput.StartDate.AddYears(i),
                                TranAmount = tamt,
                                Desp = datInput.Desp + " | " + (i + 1).ToString() + " / " + nyears.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Manual:
                    {
                        // It shall return only entry out
                        listResults.Add(new RepeatedDatesWithAmount
                        {
                            TranDate = datInput.EndDate,
                            TranAmount = datInput.TotalAmount,
                            Desp = datInput.Desp + " | 1 / 1"
                        });
                    }
                    break;
            }

            // Before return, ensure the tranamount is correct
            decimal realamt = 0;
            if (listResults.Count > 0)
            {
                listResults.ForEach(rst =>
                {
                    realamt += rst.TranAmount;
                });
                if (realamt != datInput.TotalAmount)
                {
                    listResults[0].TranAmount -= (realamt - datInput.TotalAmount);
                }
            }

            return listResults;
        }
    
        public static List<RepeatedDatesWithAmountAndInterest> WorkoutRepeatedDatesWithAmountAndInterest(RepeatDatesWithAmountAndInterestCalInput datInput)
        {
            List<RepeatedDatesWithAmountAndInterest> listResults = new List<RepeatedDatesWithAmountAndInterest>();

            // Input checks
            if (datInput == null)
                throw new Exception("Input the data!");
            datInput.doVerify();

            var realStartDate = datInput.StartDate;
            if (datInput.FirstRepayDate.HasValue)
                realStartDate = datInput.FirstRepayDate.Value;
            if (datInput.RepayDayInMonth.HasValue && datInput.RepayDayInMonth.Value != realStartDate.Day)
            {
                if (datInput.RepayDayInMonth.Value > realStartDate.Day)
                {
                    realStartDate = realStartDate.AddDays(datInput.RepayDayInMonth.Value - realStartDate.Day);
                }
                else
                {
                    realStartDate = realStartDate.AddMonths(1);
                    realStartDate = realStartDate.AddDays(datInput.RepayDayInMonth.Value - realStartDate.Day);
                }
            }
            var nInitDelay = (int)((DateTime)realStartDate - (DateTime)datInput.StartDate).TotalDays - 30;

            if (datInput.InterestFreeLoan)
            {
                switch (datInput.RepaymentMethod)
                {
                    case LoanRepaymentMethod.EqualPrincipal:
                    case LoanRepaymentMethod.EqualPrincipalAndInterset:
                        {

                            for (int i = 0; i < datInput.TotalMonths; i++)
                            {
                                listResults.Add(new RepeatedDatesWithAmountAndInterest
                                {
                                    TranDate = realStartDate.AddMonths(i),
                                    TranAmount = Math.Round(datInput.TotalAmount / datInput.TotalMonths, 2),
                                    InterestAmount = 0
                                });
                            }
                        }
                        break;

                    case LoanRepaymentMethod.DueRepayment:
                    default:
                        {
                            if (datInput.EndDate.HasValue)
                            {
                                listResults.Add(new RepeatedDatesWithAmountAndInterest
                                {
                                    TranDate = datInput.EndDate.Value,
                                    TranAmount = datInput.TotalAmount,
                                    InterestAmount = 0
                                });
                            }
                            else
                            {
                                listResults.Add(new RepeatedDatesWithAmountAndInterest
                                {
                                    TranDate = datInput.StartDate,
                                    TranAmount = datInput.TotalAmount,
                                    InterestAmount = 0
                                });
                            }
                        }
                        break;
                }
            }
            else
            {
                // Have interest rate inputted
                switch (datInput.RepaymentMethod)
                {
                    case LoanRepaymentMethod.EqualPrincipalAndInterset:
                        {
                            // Decimal dInitMonthIntere = 0;
                            Decimal monthRate = datInput.InterestRate / 12;
                            Decimal totalAmt = datInput.TotalAmount;
                            //if (nInitDelay > 0)
                            //    dInitMonthIntere = Math.Round(datInput.TotalAmount * (monthRate / 30) * nInitDelay, 2);
                            Decimal d3 = (Decimal)Math.Pow((double)(1 + monthRate), datInput.TotalMonths) - 1;
                            Decimal monthRepay = datInput.TotalAmount * monthRate * (Decimal)Math.Pow((double)(1 + monthRate), datInput.TotalMonths) / d3;

                            Decimal totalInterestAmt = 0;
                            for (int i = 0; i < datInput.TotalMonths; i++)
                            {
                                var rst = new RepeatedDatesWithAmountAndInterest
                                {
                                    TranDate = realStartDate.AddMonths(i),
                                    TranAmount = Math.Round(datInput.TotalAmount * monthRate * (Decimal)Math.Pow((double)(1 + monthRate), i) / d3, 2),
                                    InterestAmount = Math.Round(datInput.TotalAmount * monthRate * ((Decimal)Math.Pow((double)(1 + monthRate), datInput.TotalMonths) - (Decimal)Math.Pow((double)(1 + monthRate), i)) / d3, 2)
                                };

                                if (i == 0 && nInitDelay > 0)
                                    rst.InterestAmount = Math.Round(rst.InterestAmount + (nInitDelay - 1) * datInput.TotalAmount * monthRate / 30, 2);

                                totalAmt -= rst.TranAmount;
                                //var diff = rst.TranAmount + rst.InterestAmount - monthRepay;
                                //if (diff != 0)
                                //{
                                //    rst.TranAmount -= diff;
                                //    rst.TranAmount = Math.Round(rst.TranAmount, 2);
                                //}

                                totalInterestAmt += rst.InterestAmount;

                                listResults.Add(rst);
                            }
                            // Rounding
                            if (totalAmt != 0)
                            {
                                // Add it to first item
                                listResults[0].TranAmount += totalAmt;
                            }
                        }
                        break;

                    case LoanRepaymentMethod.EqualPrincipal:
                        {
                            Decimal monthRate = datInput.InterestRate / 12;
                            Decimal totalAmt = datInput.TotalAmount;
                            var monthPrincipal = datInput.TotalAmount / datInput.TotalMonths;

                            for (int i = 0; i < datInput.TotalMonths; i++)
                            {
                                var rst = new RepeatedDatesWithAmountAndInterest
                                {
                                    TranDate = realStartDate.AddMonths(i + 1),
                                    TranAmount = Math.Round(monthPrincipal, 2),
                                    InterestAmount = Math.Round(totalAmt * monthRate, 2)
                                };
                                if (i == 0 && nInitDelay > 0)
                                    rst.InterestAmount = Math.Round(rst.InterestAmount + (nInitDelay - 1) * datInput.TotalAmount * monthRate / 30, 2);

                                totalAmt -= rst.TranAmount;

                                listResults.Add(rst);
                            }
                            // Rounding
                            if (totalAmt != 0)
                            {
                                // Real paid is lower, substract fromfirst item
                                listResults[0].TranAmount += totalAmt;
                            } 
                        }
                        break;

                    case LoanRepaymentMethod.DueRepayment:
                        {
                            Decimal monthRate = datInput.InterestRate / 12;
                            Decimal amtInterest = 0;
                            if (datInput.EndDate.HasValue)
                            {
                                TimeSpan ts = (DateTime)datInput.EndDate.Value - (DateTime)datInput.StartDate;
                                amtInterest = datInput.TotalAmount * (Int32)Math.Round(ts.TotalDays / 30) * monthRate;
                            }
                            else if (datInput.TotalAmount > 0)
                            {
                                amtInterest = datInput.TotalAmount * datInput.TotalMonths * monthRate;
                            }

                            var rst = new RepeatedDatesWithAmountAndInterest
                            {
                                TranDate = datInput.StartDate.AddMonths(datInput.TotalMonths),
                                TranAmount = datInput.TotalAmount,
                                InterestAmount = amtInterest
                            };

                            listResults.Add(rst);
                        }
                        break;

                    default: throw new Exception("Unsupported repayment method");
                }
            }

            // Before return, ensure the tranamount is correct
            decimal realamt = 0;
            if (listResults.Count > 0)
            {
                listResults.ForEach(rst =>
                {
                    realamt += rst.TranAmount;
                });
                if (realamt != datInput.TotalAmount)
                {
                    listResults[0].TranAmount -= (realamt - datInput.TotalAmount);
                }
            }

            return listResults;
        }
    }
}
