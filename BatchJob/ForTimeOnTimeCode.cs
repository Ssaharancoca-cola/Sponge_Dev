using Sponge.Common;
using System.Globalization;

namespace BatchJob
{
    public class ForTimeOnTimeCode
    {
        public int GetForTimeOnTimeVersionN(string ReportingPeriod, string Frequency, string Granulity, string Version, out string ForTime, out string OnTime, out string PeriodFrom, out string PeriodTo)
        {
            ForTime = "";
            OnTime = "";
            int ReportingYear = DateTime.Now.Year;
            int CurrentYear = 0;
            PeriodFrom = "";
            PeriodTo = "";
            if (Frequency == "YEARLY")
            {
                if (ReportingPeriod == "PREVIOUS")
                {
                    ReportingYear = DateTime.Now.Year - 1;
                }
                else if (ReportingPeriod == "NEXT")
                {
                    ReportingYear = DateTime.Now.Year + 1;

                }
                else
                {
                    ReportingYear = DateTime.Now.Year;
                }
                if (DateTime.Now.Month >= (int)Helper.Month.July)
                {
                    ForTime = string.Format("{0}01{1}12", ReportingYear, ReportingYear + 1);//string.Format("{0}-{1}", DateTime.Today.Year, (DateTime.Today.Year + 1));
                    PeriodFrom = string.Format("01-Jan-{0}", ReportingYear);
                    PeriodTo = string.Format("31-Dec-{0}", ReportingYear + 1);
                }
                else
                {
                    ForTime = string.Format("{0}07{1}06", (ReportingYear - 1), ReportingYear);
                    PeriodFrom = string.Format("01-Jan-{0}", ReportingYear - 1);
                    PeriodTo = string.Format("31-Dec-{0}", ReportingYear);
                }

            }
            else if (Frequency == "HALF_YEARLY")
            {
                DateTime ReportingDate = DateTime.Now;
                if (ReportingPeriod == "NEXT")
                {
                    ReportingDate = DateTime.Now.Date.AddMonths(6);
                }
                else if (ReportingPeriod == "PREVIOUS")
                {
                    ReportingDate = DateTime.Now.Date.AddMonths(-6);
                }
                if (ReportingDate.Month <= (int)Helper.Month.June)
                {
                    ForTime = string.Format("{0}01{1}06", ReportingDate.Year, ReportingDate.Year, "H1");
                    PeriodFrom = string.Format("01-Jan-{0}", ReportingDate.Year);
                    PeriodTo = string.Format("30-Jun-{0}", ReportingDate.Year);
                }

                else
                {
                    ForTime = string.Format("{0}07{1}12", ReportingDate.Year, ReportingDate.Year, "H2");
                    PeriodFrom = string.Format("01-Jul-{0}", ReportingDate.Year);
                    PeriodTo = string.Format("31-Dec-{0}", ReportingDate.Year);
                }
            }

            else if (Frequency == "MONTHLY")
            {
                DateTime ReportingDate = DateTime.Now;
                if (ReportingPeriod == "PREVIOUS")
                {
                    ReportingDate = ReportingDate.AddMonths(-1);
                }
                else if (ReportingPeriod == "NEXT")
                {
                    ReportingDate = ReportingDate.AddMonths(1);
                }
                string month = string.Format("{00:00}", ReportingDate.Month).Trim().TrimEnd().TrimStart();
                ForTime = string.Format("{0}{1}", ReportingDate.Year, month);
                //PeriodFrom = "01-" + ReportingDate.ToString("MMM") + "-" + ReportingDate.Year; //string.Format("{0}-{1}-{2}",1, ReportingDate.ToString("MMM"), ReportingDate.Year);
                PeriodFrom = string.Format("{0}-{1}-{2}", 01, ReportingDate.ToString("MMM"), ReportingDate.Year);
                PeriodTo = string.Format("{0}-{1}-{2}", DateTime.DaysInMonth(ReportingDate.Year, ReportingDate.Month), ReportingDate.ToString("MMM"), ReportingDate.Year);



            }
            else if (Frequency == "WEEKLY")
            {
                DateTime ReportingDate = DateTime.Now;

                if (ReportingPeriod == "NEXT")
                {
                    ReportingDate = DateTime.Now.AddDays(7);
                }
                else if (ReportingPeriod == "PREVIOUS")
                {
                    ReportingDate = DateTime.Now.AddDays(-7);
                };
                if (ReportingDate.Day <= 7)
                {
                    ForTime = string.Format("{0}1", ReportingDate.ToString("yyyyMM"));
                    PeriodFrom = string.Format("01-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                    PeriodTo = string.Format("07-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);

                }
                else if (ReportingDate.Day > 7 && ReportingDate.Day <= 14)
                {
                    ForTime = string.Format("{0}2", ReportingDate.ToString("yyyyMM"));
                    PeriodFrom = string.Format("08-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                    PeriodTo = string.Format("14-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                }
                else if (ReportingDate.Day > 14 && ReportingDate.Day <= 21)
                {
                    ForTime = string.Format("{0}3", ReportingDate.ToString("yyyyMM"));
                    PeriodFrom = string.Format("15-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                    PeriodTo = string.Format("21-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);

                }
                else
                {
                    ForTime = string.Format("{0}4", ReportingDate.ToString("yyyyMM"));
                    PeriodFrom = string.Format("22-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                    PeriodTo = string.Format("{0}-{1}-{2}", DateTime.DaysInMonth(ReportingDate.Year, ReportingDate.Month), ReportingDate.ToString("MMM"), ReportingDate.Year);

                }
            }
            else if (Frequency == "QUARTERLY")
            {

                DateTime ReportingDate = DateTime.Now;
                if (ReportingPeriod == "NEXT")
                {
                    ReportingDate = DateTime.Now.Date.AddMonths(3);
                }
                else if (ReportingPeriod == "PREVIOUS")
                {
                    ReportingDate = DateTime.Now.Date.AddMonths(-3);
                }
                if (ReportingDate.Month >= (int)Helper.Month.July && ReportingDate.Month <= (int)Helper.Month.September)
                {
                    ForTime = string.Format("{0}07{1}09", ReportingDate.Year, ReportingDate.Year);

                    PeriodFrom = string.Format("01-Jul-{0}", ReportingDate.Year);
                    PeriodTo = string.Format("30-Sep-{0}", ReportingDate.Year);
                }
                else if (ReportingDate.Month >= (int)Helper.Month.October && ReportingDate.Month <= (int)Helper.Month.December)
                {
                    ForTime = string.Format("{0}10{1}12", ReportingDate.Year, ReportingDate.Year);
                    PeriodFrom = string.Format("01-Oct-{0}", ReportingDate.Year);
                    PeriodTo = string.Format("31-Dec-{0}", ReportingDate.Year);
                }
                else if (ReportingDate.Month >= (int)Helper.Month.January && ReportingDate.Month <= (int)Helper.Month.March)
                {
                    PeriodFrom = string.Format("01-Jan-{0}", ReportingDate.Year);
                    PeriodTo = string.Format("31-Mar-{0}", ReportingDate.Year);
                    ForTime = string.Format("{0}01{1}03", ReportingDate.Year, ReportingDate.Year);
                }
                else
                {
                    PeriodFrom = string.Format("01-Apr-{0}", ReportingDate.Year);
                    PeriodTo = string.Format("30-Jun-{0}", ReportingDate.Year);
                    ForTime = string.Format("{0}04{1}06", ReportingDate.Year, ReportingDate.Year);
                }
            }
            else if (Frequency == "DAILY")
            {

                DateTime ReportingDate = DateTime.Now;
                if (ReportingPeriod == "NEXT")
                {
                    ReportingDate = DateTime.Now.Date.AddDays(1);
                }
                else if (ReportingPeriod == "PREVIOUS")
                {
                    ReportingDate = DateTime.Now.Date.AddDays(-1);
                }
                ForTime = ReportingDate.ToString("yyyyMMdd");
                PeriodTo = string.Format("{0}-{1}-{2}", ReportingDate.Day, ReportingDate.ToString("MMM"), ReportingDate.Year);
                PeriodFrom = string.Format("{0}-{1}-{2}", ReportingDate.Day, ReportingDate.ToString("MMM"), ReportingDate.Year);

            }
            OnTime = ForTime;

            return CurrentYear;
        }
        public int GetForTimeOnTimeVersion(string ReportingPeriod, string Frequency, string Granulity, string Version, ref string ForTime, out string OnTime, ref string PeriodFrom, ref string PeriodTo)
        {
            OnTime = "";

            int currentYear = 0;

            if (Frequency == "YEARLY")
            {
                if (Granulity == "HALF_YEARLY")
                {
                    DateTime ReportingDate = DateTime.Now;
                    if (ReportingPeriod == "NEXT")
                    {
                        ReportingDate = DateTime.Now.Date.AddMonths(6);
                    }
                    else if (ReportingPeriod == "PREVIOUS")
                    {
                        ReportingDate = DateTime.Now.Date.AddMonths(-6);
                    }
                    if (ReportingDate.Month <= (int)Helper.Month.June)
                    {
                        OnTime = string.Format("{0}01{1}06", ReportingDate.Year, ReportingDate.Year, "H1");
                        //PeriodFrom = string.Format("01-Jul-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("01-Dec-{0}", ReportingDate.Year);
                    }
                    else
                    {
                        OnTime = string.Format("{0}07{1}12", ReportingDate.Year, ReportingDate.Year, "H2");
                        //PeriodFrom = string.Format("01-Jan-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("30-Jun-{0}", ReportingDate.Year);
                    }

                }
                else if (Granulity == "QUARTERLY")
                {
                    DateTime ReportingDate = DateTime.Now;
                    if (ReportingPeriod == "NEXT")
                    {
                        ReportingDate = DateTime.Now.Date.AddMonths(3);
                    }
                    else if (ReportingPeriod == "PREVIOUS")
                    {
                        ReportingDate = DateTime.Now.Date.AddMonths(-3);
                    }
                    if (ReportingDate.Month >= (int)Helper.Month.July && ReportingDate.Month <= (int)Helper.Month.September)
                    {
                        OnTime = string.Format("{0}07{1}09", ReportingDate.Year, ReportingDate.Year);
                        //PeriodFrom = string.Format("01-Jul-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("30-Sep-{0}", ReportingDate.Year);
                    }
                    else if (ReportingDate.Month >= (int)Helper.Month.October && ReportingDate.Month <= (int)Helper.Month.December)
                    {
                        OnTime = string.Format("{0}10{1}12", ReportingDate.Year, ReportingDate.Year);
                        //PeriodFrom = string.Format("01-Oct-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("31-Dec-{0}", ReportingDate.Year);
                    }
                    else if (ReportingDate.Month >= (int)Helper.Month.January && ReportingDate.Month <= (int)Helper.Month.March)
                    {
                        OnTime = string.Format("{0}01{1}03", ReportingDate.Year, ReportingDate.Year);
                        //PeriodFrom = string.Format("01-Jan-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("31-Mar-{0}", ReportingDate.Year);
                    }
                    else
                    {
                        OnTime = string.Format("{0}04{1}06", ReportingDate.Year, ReportingDate.Year);
                        //PeriodFrom = string.Format("01-Apr-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("30-Jun-{0}", ReportingDate.Year);
                    }


                }
                else if (Granulity == "MONTHLY")
                {
                    DateTime ReportingDate = DateTime.Now;
                    if (ReportingPeriod == "PREVIOUS")
                    {
                        ReportingDate = ReportingDate.AddMonths(-1);
                    }
                    else if (ReportingPeriod == "NEXT")
                    {
                        ReportingDate = ReportingDate.AddMonths(1);
                    }

                    string month = string.Format("{00:00}", ReportingDate.Month).Trim().TrimEnd().TrimStart();
                    OnTime = string.Format("{0}{1}", ReportingDate.Year, month);

                    //PeriodFrom = string.Format("01-{0}-{1}", ReportingDate.Month, ReportingDate.Year);
                    //PeriodTo = string.Format("{0}-{1}-{2}", DateTime.DaysInMonth(ReportingDate.Year, ReportingDate.Month), ReportingDate.ToString("MMM"), ReportingDate.Year);


                }
            }
            else if (Frequency == "HALF_YEARLY")
            {
                if (Granulity == "QUARTERLY")
                {
                    DateTime ReportingDate = DateTime.Now;
                    if (ReportingPeriod == "NEXT")
                    {
                        ReportingDate = DateTime.Now.Date.AddMonths(3);
                    }
                    else if (ReportingPeriod == "PREVIOUS")
                    {
                        ReportingDate = DateTime.Now.Date.AddMonths(-3);
                    }
                    if (ReportingDate.Month >= (int)Helper.Month.July && ReportingDate.Month <= (int)Helper.Month.September)
                    {
                        OnTime = string.Format("{0}07{1}09", ReportingDate.Year, ReportingDate.Year);
                        //PeriodFrom = string.Format("01-Jul-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("30-Sep-{0}", ReportingDate.Year);
                    }
                    else
                  if (ReportingDate.Month >= (int)Helper.Month.October && ReportingDate.Month <= (int)Helper.Month.December)
                    {
                        OnTime = string.Format("{0}10{1}12", ReportingDate.Year, ReportingDate.Year);
                        //PeriodFrom = string.Format("01-Oct-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("31-Dec-{0}", ReportingDate.Year);
                    }
                    else
                   if (ReportingDate.Month >= (int)Helper.Month.January && ReportingDate.Month <= (int)Helper.Month.March)
                    {
                        OnTime = string.Format("{0}01{1}03", ReportingDate.Year, ReportingDate.Year);

                        //PeriodFrom = string.Format("01-Jan-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("31-Mar-{0}", ReportingDate.Year);
                    }
                    else
                    {
                        OnTime = string.Format("{0}04{1}06", ReportingDate.Year, ReportingDate.Year);
                        //PeriodFrom = string.Format("01-Apr-{0}", ReportingDate.Year);
                        //PeriodTo = string.Format("30-Jun-{0}", ReportingDate.Year);
                    }
                }
                else if (Granulity == "MONTHLY")
                {
                    DateTime ReportingDate = DateTime.Now;
                    if (ReportingPeriod == "PREVIOUS")
                    {
                        ReportingDate = ReportingDate.AddMonths(-1);
                    }
                    else if (ReportingPeriod == "NEXT")
                    {
                        ReportingDate = ReportingDate.AddMonths(1);
                    }
                    string month = string.Format("{00:00}", ReportingDate.Month).Trim().TrimEnd().TrimStart();

                    OnTime = string.Format("{0}{1}", ReportingDate.Year, month);
                    //PeriodFrom = string.Format("01-{0}-{1}", ReportingDate.Month, ReportingDate.Year);
                    //PeriodTo = string.Format("{0}-{1}-{2}", DateTime.DaysInMonth(ReportingDate.Year, ReportingDate.Month), ReportingDate.ToString("MMM"), ReportingDate.Year);


                }

            }

            else if (Frequency == "MONTHLY")
            {
                if (Granulity == "WEEKLY")
                {
                    DateTime ReportingDate = DateTime.Now;

                    if (ReportingPeriod == "NEXT")
                    {
                        ReportingDate = DateTime.Now.AddDays(7);
                    }
                    else if (ReportingPeriod == "PREVIOUS")
                    {
                        ReportingDate = DateTime.Now.AddDays(-7);
                    };
                    ForTime = string.Format("{0}", ReportingDate.ToString("yyyyMM"));

                    if (ReportingDate.Day <= 7)
                    {
                        OnTime = string.Format("{0}1", ReportingDate.ToString("yyyyMM"));
                        //PeriodFrom = string.Format("01-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                        //PeriodTo = string.Format("07-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                    }
                    else if (ReportingDate.Day > 7 && ReportingDate.Day <= 14)
                    {
                        OnTime = string.Format("{0}2", ReportingDate.ToString("yyyyMM"));

                        //PeriodFrom = string.Format("08-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                        //PeriodTo = string.Format("14-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                    }
                    else if (ReportingDate.Day > 14 && ReportingDate.Day <= 21)
                    {
                        OnTime = string.Format("{0}3", ReportingDate.ToString("yyyyMM"));

                        //PeriodFrom = string.Format("15-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                        //PeriodTo = string.Format("21-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                    }
                    else
                    {
                        //PeriodFrom = string.Format("22-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                        //PeriodTo = string.Format("{0}-{1}-{2}", DateTime.DaysInMonth(ReportingDate.Year, ReportingDate.Month), ReportingDate.ToString("MMM"), ReportingDate.Year);
                        OnTime = string.Format("{0}4", ReportingDate.ToString("yyyyMM"));
                    }
                }
                else if (Granulity == "DAILY")
                {
                    DateTime ReportingDate = DateTime.Now;
                    if (ReportingPeriod == "NEXT")
                    {
                        ReportingDate = DateTime.Now.Date.AddDays(1);
                    }
                    else if (ReportingPeriod == "PREVIOUS")
                    {
                        ReportingDate = DateTime.Now.Date.AddDays(-1);
                    }
                    OnTime = ReportingDate.ToString("yyyyMMdd");
                    ForTime = ReportingDate.ToString("yyyyMM");
                    //PeriodFrom = string.Format("01-{0}-{1}", ReportingDate.Month, ReportingDate.Year);
                    //PeriodTo = string.Format("{0}-{1}-{2}", DateTime.DaysInMonth(ReportingDate.Year, ReportingDate.Month), ReportingDate.ToString("MMM"), ReportingDate.Year);
                }
                else if (Frequency == "QUARTERLY")
                {
                    if (Granulity == "MONTHLY")
                    {
                        DateTime ReportingDate = DateTime.Now;
                        if (ReportingPeriod == "PREVIOUS")
                        {
                            ReportingDate = ReportingDate.AddMonths(-1);
                        }
                        else if (ReportingPeriod == "NEXT")
                        {
                            ReportingDate = ReportingDate.AddMonths(1);
                        }
                        string month = string.Format("{00:00}", ReportingDate.Month).Trim().TrimEnd().TrimStart();
                        OnTime = string.Format("{0}{1}", ReportingDate.Year, month);
                        //PeriodFrom = string.Format("01-{0}-{1}", ReportingDate.Month, ReportingDate.Year);
                        //PeriodTo = string.Format("{0}-{1}-{2}", DateTime.DaysInMonth(ReportingDate.Year, ReportingDate.Month), ReportingDate.ToString("MMM"), ReportingDate.Year);

                    }
                }
                else if (Frequency == "WEEKLY")
                {
                    if (Granulity == "DAILY")
                    {
                        DateTime ReportingDate = DateTime.Now;
                        if (ReportingPeriod == "NEXT")
                        {
                            ReportingDate = DateTime.Now.Date.AddDays(1);
                        }
                        else if (ReportingPeriod == "PREVIOUS")
                        {
                            ReportingDate = DateTime.Now.Date.AddDays(-1);
                        }
                        if (ReportingDate.Day <= 7)
                        {
                            OnTime = string.Format("{0}1", ReportingDate.ToString("yyyyMM"));
                            ForTime = string.Format("{0}1", ReportingDate.ToString("yyyyMM"));


                        }
                        else if (ReportingDate.Day > 7 && ReportingDate.Day <= 14)
                        {
                            OnTime = string.Format("{0}8", ReportingDate.ToString("yyyyMM"));

                            ForTime = string.Format("{0}2", ReportingDate.ToString("yyyyMM"));

                        }
                        else if (ReportingDate.Day > 14 && ReportingDate.Day <= 21)
                        {
                            OnTime = string.Format("{0}15", ReportingDate.ToString("yyyyMM"));
                            ForTime = string.Format("{0}3", ReportingDate.ToString("yyyyMM"));

                        }
                        else
                        {
                            OnTime = string.Format("{0}22", ReportingDate.ToString("yyyyMM"));
                            ForTime = string.Format("{0}4", ReportingDate.ToString("yyyyMM"));
                        }

                        //PeriodFrom = string.Format("01-{0}-{1}", ReportingDate.ToString("MMM"), ReportingDate.Year);
                        //PeriodTo = string.Format("{0}-{1}-{2}", DateTime.DaysInMonth(ReportingDate.Year, ReportingDate.Month), ReportingDate.ToString("MMM"), ReportingDate.Year);

                    }

                }
            }
            return currentYear;
        }
        public void PeriodFromPeriodTo(string fortime, string ontime, string frequency, string TimeLevel, out string PeriodFrom, out string PeriodTo)
        {
            PeriodFrom = "";
            PeriodTo = "";
            int Formonth = 0;
            int Tomonth = 0;
            Formonth = Convert.ToInt32(fortime.Substring(4, 2));
            string ToYear = "";
            if (fortime.Length > 6)
            {
                Tomonth = Convert.ToInt32(ontime.Substring(10, 2));
                ToYear = ontime.Substring(6, 4);
            }
            else
            {
                Tomonth = Convert.ToInt32(ontime.Substring(4, 2));
                ToYear = ontime.Substring(0, 4);
            }
            string ForMonthDIsplay = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(Formonth);
            string ToMonthDIsplay = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(Tomonth);
            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(Tomonth);

            int year = Convert.ToInt32(fortime.Substring(0, 4));
            int LastDay = DateTime.DaysInMonth(year, Tomonth);
            PeriodFrom = string.Format("01-{0}-{1}", ForMonthDIsplay, fortime.Substring(0, 4));
            PeriodTo = string.Format("{0}-{1}-{2}", LastDay.ToString(), ToMonthDIsplay, ToYear);

        }


    }
}