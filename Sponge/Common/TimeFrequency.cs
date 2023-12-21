using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Sponge.Common
{
    public class TimeFrequency
    {
        string currentVal = Convert.ToString(DateTime.Now.Year);
        string previousVal = Convert.ToString(DateTime.Now.AddYears(-1).Year);
        string prepreviousVal = Convert.ToString(DateTime.Now.AddYears(-2).Year);
        string nextVal = Convert.ToString(DateTime.Now.AddYears(1).Year);
        List<string> headerList = new List<string>();

        public List<string> AnnualFrequency(string FREQUENCY, string TIME_LEVEL, string REPORTING_PERIOD, string PERIOD)
        {
            if (TIME_LEVEL.Equals("YEARLY"))
            {

                if (REPORTING_PERIOD.Equals("PREVIOUS"))
                {
                    headerList.Add(previousVal);
                }
                else if (REPORTING_PERIOD.Equals("NEXT"))
                {
                    headerList.Add(nextVal);
                }
                else
                {
                    headerList.Add(currentVal);
                }

            }
            else if (TIME_LEVEL.Equals("QUARTERLY"))
            {
                for (int i = 1; i < 5; i++)
                {
                    if (REPORTING_PERIOD.Equals("PREVIOUS"))
                    {
                        headerList.Add("Q" + i);
                    }
                    else if (REPORTING_PERIOD.Equals("NEXT"))
                    {
                        headerList.Add("Q" + i);
                    }
                    else
                    {
                        headerList.Add("Q" + i);
                    }
                }

            }
            else if (TIME_LEVEL.Equals("HALF_YEARLY"))
            {
                for (int i = 1; i < 3; i++)
                {
                    if (REPORTING_PERIOD.Equals("PREVIOUS"))
                    {
                        headerList.Add("H" + i);
                    }
                    else if (REPORTING_PERIOD.Equals("NEXT"))
                    {
                        headerList.Add("H" + i);
                    }
                    else
                    {
                        headerList.Add("H" + i);
                    }
                }
            }
            else if (TIME_LEVEL.Equals("MONTHLY"))
            {
                string monthName = "";//DateTime.Today;
                for (int i = 1; i <= 6; i++)
                {
                    monthName = new DateTime(DateTime.Now.AddYears(-2).Year, i, 1).ToString("MMM", CultureInfo.InvariantCulture);
                    headerList.Add(monthName);
                }
                for (int i = 7; i <=12; i++)
                {
                    monthName = new DateTime(DateTime.Now.AddYears(-1).Year, i, 1).ToString("MMM", CultureInfo.InvariantCulture);
                    headerList.Add(monthName);
                }

            }
            return headerList;
        }

        public List<string> HalfYearlyFrequency(string FREQUENCY, string TIME_LEVEL, string REPORTING_PERIOD, string PERIOD)
        {
            if (TIME_LEVEL.Equals("QUARTERLY"))
            {
                for (int i = 1; i <= 2; i++)
                {
                    headerList.Add("Q" + i);

                }

            }
            else if (TIME_LEVEL.Equals("MONTHLY"))
            {
                string monthName = "";// DateTime.Today;
                for (int i = 1; i <= 6; i++)
                {
                    headerList.Add("M" + i);
                }
            }
            else if (TIME_LEVEL.Equals("HALF_YEARLY"))
            {
                if (REPORTING_PERIOD.Equals("PREVIOUS"))
                {
                    headerList.Add(previousVal + "-H1");
                }
                else if (REPORTING_PERIOD.Equals("NEXT"))
                {
                    headerList.Add(nextVal + "-H1");
                }
                else
                {
                    headerList.Add(currentVal + "-H1");
                }
            }
            return headerList;
        }

        public List<string> QuarterlyFrequency(string FREQUENCY, string TIME_LEVEL, string REPORTING_PERIOD, string PERIOD)
        {
            int currMonth = DateTime.Now.Month;
            int currYear = DateTime.Now.Year;
            #region Monthly
            if (TIME_LEVEL.Equals("MONTHLY"))
            {
                string monthName = "";//DateTime.Today;

                for (int i = 1; i <= 3; i++)
                {
                    headerList.Add("M" + i);
                }
            }
            #endregion
            #region Weekly
            else if (TIME_LEVEL.Equals("WEEKLY"))
            {
                for (int i = 1; i <= 12; i++)
                {
                    headerList.Add("W" + i);
                }


            }
            #endregion
            #region Quarterly
            else if (TIME_LEVEL.Equals("QUARTERLY"))
            {
                if (REPORTING_PERIOD.Equals("PREVIOUS"))
                {
                    headerList.Add(previousVal + "-Q");
                }
                else if (REPORTING_PERIOD.Equals("NEXT"))
                {
                    headerList.Add(nextVal + "-Q");
                }
                else
                {
                    headerList.Add(currentVal + "-Q");
                }
            }
            #endregion
            return headerList;
        }
        public List<string> MonthlyFrequency(string FREQUENCY, string TIME_LEVEL, string REPORTING_PERIOD, string PERIOD)
        {
            DateTime dateTime = DateTime.Now;
            if (TIME_LEVEL.Equals("WEEKLY") || TIME_LEVEL.Equals("MONTHLY"))
            {
                if (REPORTING_PERIOD.Equals("PREVIOUS"))
                    dateTime = dateTime.AddMonths(-1);
                else if (REPORTING_PERIOD.Equals("NEXT"))
                    dateTime = dateTime.AddMonths(1);

                for (int i = 1; i <= Convert.ToInt32(PERIOD); i++)
                {
                    if (TIME_LEVEL.Equals("WEEKLY"))
                    {
                        headerList.Add("W" + i);
                    }
                    if (TIME_LEVEL.Equals("MONTHLY"))
                    {
                        var weekCount = i % 4;

                        headerList.Add("M" + (weekCount + 1));
                    }
                }
            }
            else if (TIME_LEVEL.Equals("DAILY"))
            {
                for (int i = 1; i <= 31; i++)
                {
                    headerList.Add("" + i);
                }
            }
            return headerList;
        }

        public List<string> WeeklyFrequency(string FREQUENCY, string TIME_LEVEL, string REPORTING_PERIOD, string PERIOD)
        {
            DateTime dateTime = DateTime.Now;
            if (TIME_LEVEL.Equals("WEEKLY"))
            {
                for (int i = 0; i < Convert.ToInt32(PERIOD); i++)
                {
                    headerList.Add(dateTime.ToString("yyyy") + "-" + dateTime.ToString("MMM") + "W" + i);
                }
            }
            else if (TIME_LEVEL.Equals("DAILY"))
            {
                int iManipulater = 0;
                for (int i = 1; i <= 10; i++)
                {
                    headerList.Add("D" + i);

                }
            }
            return headerList;
        }
    }
}