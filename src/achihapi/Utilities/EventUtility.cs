using System;
using System.Collections.Generic;
using achihapi.ViewModels;

namespace achihapi.Utilities
{
    public class EventUtility
    {
        public static List<EventGenerationResultViewModel> GenerateEvents(EventGenerationInputViewModel datInput)
        {
            List<EventGenerationResultViewModel> listResults = new List<EventGenerationResultViewModel>();

            // Input checks
            if (datInput == null)
                throw new Exception("Input the data!");
            if (datInput.EndTimePoint < datInput.StartTimePoint)
                throw new Exception("Invalid data range");
            if (String.IsNullOrEmpty(datInput.Name))
                throw new Exception("Invalid name");

            switch (datInput.RptType)
            {
                case RepeatFrequency.Day:
                    {
                        var tspans = datInput.EndTimePoint.Date - datInput.StartTimePoint.Date;
                        var tdays = (Int32)tspans.Days;

                        for (int i = 0; i < tdays; i++)
                        {
                            listResults.Add(new EventGenerationResultViewModel
                            {
                                StartTimePoint = datInput.StartTimePoint.AddDays(i),
                                Name = datInput.Name + " | " + (i + 1).ToString() + " / " + tdays.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Fortnight:
                    {
                        var tspans = datInput.EndTimePoint.Date - datInput.StartTimePoint.Date;
                        var tdays = (Int32)tspans.Days;

                        var tfortnights = tdays / 14;

                        for (int i = 0; i < tfortnights; i++)
                        {
                            listResults.Add(new EventGenerationResultViewModel
                            {
                                StartTimePoint = datInput.StartTimePoint.AddDays(i * 14),
                                Name = datInput.Name + " | " + (i + 1).ToString() + " / " + tfortnights.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.HalfYear:
                    {
                        var nmonths = (datInput.EndTimePoint.Year - datInput.StartTimePoint.Year) * 12 + (datInput.EndTimePoint.Month - datInput.StartTimePoint.Month);
                        var nhalfyear = nmonths / 6;

                        for (int i = 0; i < nhalfyear; i++)
                        {
                            listResults.Add(new EventGenerationResultViewModel
                            {
                                StartTimePoint = datInput.StartTimePoint.AddMonths(i * 6),
                                Name = datInput.Name + " | " + (i + 1).ToString() + " / " + nhalfyear.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Month:
                    {
                        var nmonths = (datInput.EndTimePoint.Year - datInput.StartTimePoint.Year) * 12 + (datInput.EndTimePoint.Month - datInput.StartTimePoint.Month);

                        for (int i = 0; i < nmonths; i++)
                        {
                            listResults.Add(new EventGenerationResultViewModel
                            {
                                StartTimePoint = datInput.StartTimePoint.AddMonths(i),
                                Name = datInput.Name + " | " + (i + 1).ToString() + " / " + nmonths.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Quarter:
                    {
                        var nmonths = (datInput.EndTimePoint.Year - datInput.StartTimePoint.Year) * 12 + (datInput.EndTimePoint.Month - datInput.StartTimePoint.Month);
                        var nquarters = nmonths / 3;

                        for (int i = 0; i < nquarters; i++)
                        {
                            listResults.Add(new EventGenerationResultViewModel
                            {
                                StartTimePoint = datInput.StartTimePoint.AddMonths(i * 3),
                                Name = datInput.Name + " | " + (i + 1).ToString() + " / " + nquarters.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Week:
                    {
                        var tspans = datInput.EndTimePoint.Date - datInput.StartTimePoint.Date;
                        var tdays = (Int32)tspans.Days;

                        var tweeks = tdays / 7;

                        for (int i = 0; i < tweeks; i++)
                        {
                            listResults.Add(new EventGenerationResultViewModel
                            {
                                StartTimePoint = datInput.StartTimePoint.AddDays(i * 7),
                                Name = datInput.Name + " | " + (i + 1).ToString() + " / " + tweeks.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Year:
                    {
                        var nyears = datInput.EndTimePoint.Year - datInput.StartTimePoint.Year;

                        for (int i = 0; i < nyears; i++)
                        {
                            listResults.Add(new EventGenerationResultViewModel
                            {
                                StartTimePoint = datInput.StartTimePoint.AddYears(i),
                                Name = datInput.Name + " | " + (i + 1).ToString() + " / " + nyears.ToString()
                            });
                        }
                    }
                    break;

                case RepeatFrequency.Manual:
                    {
                        // TBD.
                    }
                    break;
            }

            return listResults;
        }
    }
}
