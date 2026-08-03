using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Claims;
using hihapi.Exceptions;
using hihapi.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace hihapi.Utilities
{
    internal class HIHAPIConstants
    {
        public const String OnlyOwnerAndDisplay = "OnlyOwnerAndDisplay";
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
        internal static void HandleModelStateError(ModelStateDictionary modelState)
        {
            var errors = new List<string>();
            foreach (var value in modelState.Values)
            {
                foreach (var err in value.Errors)
                {
                    errors.Add(err.Exception != null ? err.Exception.Message : err.ErrorMessage);
                }
            }

            throw new BadRequestException("Model State Failed: " + string.Join("; ", errors));
        }

        /// <summary>
        /// Gets the immutable user identifier from the authenticated user's claims.
        /// <para>
        /// Resolution order:
        ///   1. "sub" — OIDC standard subject claim (Duende IdentityServer access tokens)
        ///   2. ClaimTypes.NameIdentifier — used in test mocks
        /// </para>
        /// <para>
        /// Does NOT fall back to the "name" claim, because the username is mutable
        /// and must not be used as a stable identity key / foreign key in the database.
        /// </para>
        /// <remarks>
        /// Root cause of the change (2026-06-24):
        /// Previously this method returned the "name" claim (e.g. "alvachien"), which is
        /// the mutable ASP.NET Core Identity username. However, the UI sends the immutable
        /// subject ID (the "sub" GUID from IdentityServer) as the <c>User</c> field when
        /// creating HomeMember records. This caused a mismatch:
        /// <list type="bullet">
        ///   <item>POST /HomeDefines stored Createdby = "alvachien" (from "name" claim),
        ///         but Members[].User = "20e31ea5-..." (GUID from the UI).</item>
        ///   <item>GET  /HomeDefines queried <c>WHERE hmem.User == usrName</c>, where
        ///         usrName was resolved from the "name" claim ("alvachien"). Since the
        ///         stored User was the GUID, the query returned no results — the home
        ///         appeared to be created successfully (data was in the DB) but the UI
        ///         showed nothing, with no error reported.</item>
        /// </list>
        /// The fix: always resolve the subject ID ("sub" claim) as the user identity.
        /// This makes POST and GET use the same immutable value, and also protects
        /// against future username changes breaking existing data references.
        /// </remarks>
        /// <returns>
        /// The immutable user identifier (subject ID GUID). Returns
        /// <see cref="string.Empty"/> when no subject claim is found; callers should
        /// throw <see cref="UnauthorizedAccessException"/> in that case.
        /// </returns>
        internal static String GetUserID(Microsoft.AspNetCore.Mvc.ControllerBase ctrl)
        {
            if (ctrl.User != null)
                return ctrl.User.FindFirst("sub")?.Value
                    ?? ctrl.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? String.Empty;
            return String.Empty;
        }

        /// <summary>
        /// Gets the immutable user identifier, throwing <see cref="UnauthorizedAccessException"/>
        /// if the user is not authenticated or no subject claim is present.
        /// See <see cref="GetUserID"/> for claim resolution details.
        /// </summary>
        internal static String GetAuthenticatedUserName(Microsoft.AspNetCore.Mvc.ControllerBase ctrl)
        {
            var userId = GetUserID(ctrl);
            if (String.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();
            return userId;
        }

        internal static string EnsureFolderExistence(String rootPath, String subFolders)
        {
            var fullPath = Path.GetFullPath(Path.Combine(rootPath, subFolders));
            if (!fullPath.StartsWith(Path.GetFullPath(rootPath) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !fullPath.Equals(Path.GetFullPath(rootPath), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Path traversal detected in subfolder path");
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        internal static string UploadFolder { get; set; }
        internal static string BlogFolder { get; set; }
    }

    public class CommonUtility
    {
        public static List<RepeatedDates> WorkoutRepeatedDates(RepeatDatesCalculationInput datInput)
        {
            List<RepeatedDates> listResults = new List<RepeatedDates>();

            // Input checks
            if (datInput == null)
                throw new ArgumentException("Input the data!");
            var dtEnd = new DateTime(datInput.EndDate.Year, datInput.EndDate.Month, datInput.EndDate.Day);
            var dtStart = new DateTime(datInput.StartDate.Year, datInput.StartDate.Month, datInput.StartDate.Day);
            if (dtEnd < dtStart)
                throw new ArgumentException("Invalid data range");

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
                    var curdate = dtStart;
                    while (true)
                    {
                        listResults.Add(new RepeatedDates
                        {
                            StartDate = curdate,
                        });

                        curdate = curdate.AddDays(14);
                        if (curdate > dtEnd)
                            break;
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
                    var curdate = dtStart;
                    while (true)
                    {
                        listResults.Add(new RepeatedDates
                        {
                            StartDate = curdate,
                        });

                        curdate = curdate.AddMonths(6);
                        if (curdate > dtEnd)
                            break;
                    }

                    for (int i = 0; i < listResults.Count; i++)
                    {
                        if (i == listResults.Count - 1)
                        {
                            listResults[i].EndDate = dtEnd;
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
                    var curdate = dtStart;
                    while (true)
                    {
                        listResults.Add(new RepeatedDates
                        {
                            StartDate = curdate,
                        });

                        curdate = curdate.AddMonths(1);
                        if (curdate > dtEnd)
                            break;
                    }

                    for (int i = 0; i < listResults.Count; i++)
                    {
                        if (i == listResults.Count - 1)
                        {
                            listResults[i].EndDate = dtEnd;
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
                    var curdate = dtStart;
                    while (true)
                    {
                        listResults.Add(new RepeatedDates
                        {
                            StartDate = curdate,
                        });

                        curdate = curdate.AddMonths(3);
                        if (curdate > dtEnd)
                            break;
                    }

                    for (int i = 0; i < listResults.Count; i++)
                    {
                        if (i == listResults.Count - 1)
                        {
                            listResults[i].EndDate = dtEnd;
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
                    var curdate = dtStart;
                    while (true)
                    {
                        listResults.Add(new RepeatedDates
                        {
                            StartDate = curdate,
                        });

                        curdate = curdate.AddDays(7);
                        if (curdate > dtEnd)
                            break;
                    }

                    for (int i = 0; i < listResults.Count; i++)
                    {
                        if (i == listResults.Count - 1)
                        {
                            listResults[i].EndDate = dtEnd;
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
                    var curdate = dtStart;
                    while (true)
                    {
                        listResults.Add(new RepeatedDates
                        {
                            StartDate = curdate,
                        });

                        curdate = curdate.AddYears(1);
                        if (curdate > dtEnd)
                            break;
                    }

                    for (int i = 0; i < listResults.Count; i++)
                    {
                        if (i == listResults.Count - 1)
                        {
                            listResults[i].EndDate = dtEnd;
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
                    throw new ArgumentOutOfRangeException(nameof(datInput.RepeatType));
            }

            return listResults;
        }

        public static List<RepeatedDatesWithAmount> WorkoutRepeatedDatesWithAmount(RepeatDatesWithAmountCalculationInput datInput)
        {
            List<RepeatedDatesWithAmount> listResults = new List<RepeatedDatesWithAmount>();

            // Input checks
            if (datInput == null)
                throw new ArgumentException("Input the data!");
            var dtEnd = new DateTime(datInput.EndDate.Year, datInput.EndDate.Month, datInput.EndDate.Day);
            var dtStart = new DateTime(datInput.StartDate.Year, datInput.StartDate.Month, datInput.StartDate.Day);
            if (dtEnd < dtStart)
                throw new ArgumentException("Invalid data range");
            if (datInput.TotalAmount <= 0)
                throw new ArgumentException("Invalid total amount");
            if (String.IsNullOrEmpty(datInput.Desp))
                throw new ArgumentException("Invalid desp");

            switch (datInput.RepeatType)
            {
                case RepeatFrequency.Day:
                {
                    var tspans = dtEnd - dtStart;
                    var tdays = (Int32)tspans.Days;
                    if (tdays <= 0)
                        throw new ArgumentException("Date range must be at least 1 day for daily repeat");

                    var tamt = Math.Round(datInput.TotalAmount / tdays, 2);
                    for (int i = 0; i < tdays; i++)
                    {
                        listResults.Add(new RepeatedDatesWithAmount
                        {
                            TranDate = datInput.StartDate.AddDays(i),
                            TranAmount = tamt,
                            Desp = datInput.Desp + " | " + (i + 1).ToString(CultureInfo.InvariantCulture) + " / " + tdays.ToString(CultureInfo.InvariantCulture)
                        });
                    }
                }
                break;

                case RepeatFrequency.Fortnight:
                {
                    var tspans = dtEnd - dtStart;
                    var tdays = (Int32)tspans.Days;

                    var tfortnights = tdays / 14;
                    if (tfortnights <= 0)
                        throw new ArgumentException("Date range must be at least 14 days for fortnightly repeat");
                    var tamt = Math.Round(datInput.TotalAmount / tfortnights, 2);

                    for (int i = 0; i < tfortnights; i++)
                    {
                        listResults.Add(new RepeatedDatesWithAmount
                        {
                            TranDate = datInput.StartDate.AddDays(i * 14),
                            TranAmount = tamt,
                            Desp = datInput.Desp + " | " + (i + 1).ToString(CultureInfo.InvariantCulture) + " / " + tfortnights.ToString(CultureInfo.InvariantCulture)
                        });
                    }
                }
                break;

                case RepeatFrequency.HalfYear:
                {
                    var tspans = dtEnd - dtStart;
                    var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);
                    var nhalfyear = nmonths / 6;
                    if (nhalfyear <= 0)
                        throw new ArgumentException("Date range must be at least 6 months for half-yearly repeat");
                    var tamt = Math.Round(datInput.TotalAmount / nhalfyear, 2);

                    for (int i = 0; i < nhalfyear; i++)
                    {
                        listResults.Add(new RepeatedDatesWithAmount
                        {
                            TranDate = datInput.StartDate.AddMonths(i * 6),
                            TranAmount = tamt,
                            Desp = datInput.Desp + " | " + (i + 1).ToString(CultureInfo.InvariantCulture) + " / " + nhalfyear.ToString(CultureInfo.InvariantCulture)
                        });
                    }
                }
                break;

                case RepeatFrequency.Month:
                {
                    var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);
                    if (nmonths <= 0)
                        throw new ArgumentException("Date range must be at least 1 month for monthly repeat");

                    var tamt = Math.Round(datInput.TotalAmount / nmonths, 2);

                    for (int i = 0; i < nmonths; i++)
                    {
                        listResults.Add(new RepeatedDatesWithAmount
                        {
                            TranDate = datInput.StartDate.AddMonths(i),
                            TranAmount = tamt,
                            Desp = datInput.Desp + " | " + (i + 1).ToString(CultureInfo.InvariantCulture) + " / " + nmonths.ToString(CultureInfo.InvariantCulture)
                        });
                    }
                }
                break;

                case RepeatFrequency.Quarter:
                {
                    var nmonths = (datInput.EndDate.Year - datInput.StartDate.Year) * 12 + (datInput.EndDate.Month - datInput.StartDate.Month);
                    var nquarters = nmonths / 3;
                    if (nquarters <= 0)
                        throw new ArgumentException("Date range must be at least 3 months for quarterly repeat");
                    var tamt = Math.Round(datInput.TotalAmount / nquarters, 2);

                    for (int i = 0; i < nquarters; i++)
                    {
                        listResults.Add(new RepeatedDatesWithAmount
                        {
                            TranDate = datInput.StartDate.AddMonths(i * 3),
                            TranAmount = tamt,
                            Desp = datInput.Desp + " | " + (i + 1).ToString(CultureInfo.InvariantCulture) + " / " + nquarters.ToString(CultureInfo.InvariantCulture)
                        });
                    }
                }
                break;

                case RepeatFrequency.Week:
                {
                    var tspans = dtEnd - dtStart;
                    var tdays = (Int32)tspans.Days;

                    var tweeks = tdays / 7;
                    if (tweeks <= 0)
                        throw new ArgumentException("Date range must be at least 7 days for weekly repeat");
                    var tamt = Math.Round(datInput.TotalAmount / tweeks, 2);

                    for (int i = 0; i < tweeks; i++)
                    {
                        listResults.Add(new RepeatedDatesWithAmount
                        {
                            TranDate = datInput.StartDate.AddDays(i * 7),
                            TranAmount = tamt,
                            Desp = datInput.Desp + " | " + (i + 1).ToString(CultureInfo.InvariantCulture) + " / " + tweeks.ToString(CultureInfo.InvariantCulture)
                        });
                    }
                }
                break;

                case RepeatFrequency.Year:
                {
                    var nyears = datInput.EndDate.Year - datInput.StartDate.Year;
                    if (nyears <= 0)
                        throw new ArgumentException("Date range must be at least 1 year for yearly repeat");

                    var tamt = Math.Round(datInput.TotalAmount / nyears, 2);

                    for (int i = 0; i < nyears; i++)
                    {
                        listResults.Add(new RepeatedDatesWithAmount
                        {
                            TranDate = datInput.StartDate.AddYears(i),
                            TranAmount = tamt,
                            Desp = datInput.Desp + " | " + (i + 1).ToString(CultureInfo.InvariantCulture) + " / " + nyears.ToString(CultureInfo.InvariantCulture)
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
                throw new ArgumentException("Input the data!");
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
                    case LoanRepaymentMethod.EqualPrincipalAndInterest:
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
                    case LoanRepaymentMethod.EqualPrincipalAndInterest:
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
