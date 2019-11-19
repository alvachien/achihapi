using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using achihapi.ViewModels;

namespace achihapi.Utilities
{
    public class CommonUtility
    {
        public static List<RepeatFrequencyDateViewModel> GetDates(RepeatFrequencyDateInput datInput)
        {
            List<RepeatFrequencyDateViewModel> listResults = new List<RepeatFrequencyDateViewModel>();

            // Input checks
            if (datInput == null)
                throw new Exception("Input the data!");
            if (datInput.EndDate < datInput.StartDate)
                throw new Exception("Invalid data range");

            switch (datInput.RptType)
            {
                case RepeatFrequency.Day:
                    {
                        var tspans = datInput.EndDate.Date - datInput.StartDate.Date;
                        var tdays = (Int32)tspans.Days;

                        for (int i = 0; i <= tdays; i++)
                        {
                            listResults.Add(new RepeatFrequencyDateViewModel
                            {
                                StartDate = datInput.StartDate.AddDays(i),
                            });
                        }

                        for (int i = 0; i < listResults.Count; i ++)
                        {
                            listResults[i].EndDate = listResults[i].StartDate;
                        }
                    }
                    break;

                case RepeatFrequency.Fortnight:
                    {
                        var tspans = datInput.EndDate.Date - datInput.StartDate.Date;
                        var tdays = (Int32)tspans.Days;

                        var tfortnights = tdays / 14;

                        for (int i = 0; i <= tfortnights; i++)
                        {
                            listResults.Add(new RepeatFrequencyDateViewModel
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
                                listResults[i].EndDate = listResults[i+1].StartDate.AddDays(-1);
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
                            listResults.Add(new RepeatFrequencyDateViewModel
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
                            listResults.Add(new RepeatFrequencyDateViewModel
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
                            listResults.Add(new RepeatFrequencyDateViewModel
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
                        var tspans = datInput.EndDate.Date - datInput.StartDate.Date;
                        var tdays = (Int32)tspans.Days;

                        var tweeks = tdays / 7;

                        for (int i = 0; i <= tweeks; i++)
                        {
                            listResults.Add(new RepeatFrequencyDateViewModel
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
                            listResults.Add(new RepeatFrequencyDateViewModel
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
                        listResults.Add(new RepeatFrequencyDateViewModel
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
    }
}
