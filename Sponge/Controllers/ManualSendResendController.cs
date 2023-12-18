using DAL.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sponge.Common;
using Sponge.ViewModel;

namespace Sponge.Controllers
{
    public class ManualSendResendController : Controller
    {
        [Route("ManualSendResend/ManualSendResendExcel/{configId:int}/{subjectAreaId:int}")]
        public IActionResult ManualSendResendExcel(int configId, int subjectAreaId)
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            ManualSendResendExcelModel model = new();
            SPG_SUBJECTAREA subArea = sPONGE_Context.SPG_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID.Equals(subjectAreaId)).FirstOrDefault();
            string currentOnTimeForTime = string.Empty;
            var forTimeOnTimeList = GetForTimeOnTime(subArea, subArea.REPORTING_PERIOD, ref currentOnTimeForTime);
            model.ConfigId = configId;
            model.SubjectAreaId = subjectAreaId;
            model.UserName = (from config in sPONGE_Context.SPG_CONFIGURATION
                              join user in sPONGE_Context.SPG_USERS on config.USER_ID equals user.USER_ID
                              where config.CONFIG_ID == configId
                              select user.Name).FirstOrDefault();
            model.SubjectAreaName = sPONGE_Context.SPG_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID.Equals(subjectAreaId)).Select(s => s.SUBJECTAREA_NAME).FirstOrDefault();
            model.Frequency = sPONGE_Context.SPG_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID == subjectAreaId).Select(s => s.FREQUENCY).FirstOrDefault();
            model.Version = sPONGE_Context.SPG_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID == subjectAreaId).Select(s => s.VERSION).FirstOrDefault();
            SPG_SENDORRESENDTASK srt = sPONGE_Context.SPG_SENDORRESENDTASK.Where(x => x.CONFIG_ID == configId).FirstOrDefault();

            if(srt == null)
            {
                SPG_CONFIGURATION scg = sPONGE_Context.SPG_CONFIGURATION.Where(x => x.CONFIG_ID == configId).FirstOrDefault();
                if(scg.SCHEDULED =="Y" && scg.IS_POPULATED != null)
                {
                    model.LockDate = DateTime.Now.AddDays(Convert.ToDouble(scg.LOCK_DATE));
                    //model.UploadReminderDate = model.LockDate.AddDays(Convert.ToDouble(-scg.REMMINDER_DATE));
                    //model.EscalationAlertDate = model.LockDate.AddDays(Convert.ToDouble(-scg.ESCALATION_DATE));
                    model.IsPopulated = scg.IS_POPULATED;
                }
                if(scg.SCHEDULED == "N" && scg.IS_POPULATED != null)
                {
                    model.IsPopulated = scg.IS_POPULATED;
                }
            }
            else
            {
                model.LockDate = sPONGE_Context.SPG_SENDORRESENDTASK.Where(s =>s.CONFIG_ID == configId && s.ONTIMECODE == currentOnTimeForTime && s.FORTIMECODE == currentOnTimeForTime).OrderByDescending(s => s.ID).Select(s => s.LOCKDATE).FirstOrDefault();
                model.EscalationAlertDate = sPONGE_Context.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configId && s.ONTIMECODE == currentOnTimeForTime && s.FORTIMECODE == currentOnTimeForTime).OrderByDescending(s => s.ID).Select(s => s.ESCALATIONDATE).FirstOrDefault();
                model.UploadReminderDate = sPONGE_Context.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configId && s.ONTIMECODE == currentOnTimeForTime && s.FORTIMECODE == currentOnTimeForTime).OrderByDescending(s => s.ID).Select(s => s.UPLOADREMINDERDATE).FirstOrDefault();
                model.IsPopulated = sPONGE_Context.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configId && s.ONTIMECODE == currentOnTimeForTime && s.FORTIMECODE == currentOnTimeForTime).OrderByDescending(s => s.ID).Select(s => s.IS_POPULATED).FirstOrDefault();

            }
            model.DataCollection = sPONGE_Context.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configId && s.ONTIMECODE == currentOnTimeForTime && s.FORTIMECODE == currentOnTimeForTime).OrderByDescending(s => s.ID).Select(s => s.DATA_COLLECTION).FirstOrDefault();
            ViewBag.ForTimeOnTime = forTimeOnTimeList.Items;
            ViewBag.OnTimeLevel = sPONGE_Context.SPG_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID == subjectAreaId).Select(s => s.ONTIMELEVEL).FirstOrDefault();

            return View("Views\\ManualSendResend\\ManualSendResend.cshtml", model);
        }

        private SelectList GetForTimeOnTime(SPG_SUBJECTAREA subArea, string reportingPeriod, ref string onTimeForTime)
        {
            List<KeyValuePair<String, String>> lst = new();
            int range = subArea.FREQUENCY.Equals(Helper.Constant.YEARLY) ? 20 :
                        subArea.FREQUENCY.Equals(Helper.Constant.MONTHLY) ? 72 :
                        subArea.FREQUENCY.Equals(Helper.Constant.HALFYEARLY) ? 10 :
                        subArea.FREQUENCY.Equals(Helper.Constant.QUARTERLY) ? 24 :
                        //subArea.FREQUENCY.Equals(Helper.Constant.WEEKLY) ? 12 :
                        subArea.FREQUENCY.Equals(Helper.Constant.DAILY) ? 92 : 0;
            int rangeEnd = range / 2;
            string currentForTimeOnTime = onTimeForTime;// string.Empty;
            DateTime dt = DateTime.Now;


            if (subArea.FREQUENCY.Equals(Helper.Constant.YEARLY))
            {

                if (reportingPeriod == Helper.ReportingPeriod.NEXT) { dt = dt.AddYears(1); }
                else if (reportingPeriod == Helper.ReportingPeriod.PREVIOUS) { dt = dt.AddYears(-1); }

                if (currentForTimeOnTime == string.Empty) { currentForTimeOnTime = GetCurrentFinancialYear(dt); }
                dt = dt.AddYears(-rangeEnd);
                for (int i = 0; i < range; i++)
                {
                    dt = dt.AddYears(1);
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}07{1}06", dt.Year, dt.Year + 1), string.Format("{0}-{1}", dt.Year, dt.Year + 1)));
                }
            }
            if (subArea.FREQUENCY.Equals(Helper.Constant.MONTHLY))
            {
                if (reportingPeriod == Helper.ReportingPeriod.NEXT) { dt = dt.AddMonths(1); }
                else if (reportingPeriod == Helper.ReportingPeriod.PREVIOUS) { dt = dt.AddMonths(-1); }
                if (currentForTimeOnTime == string.Empty) { currentForTimeOnTime = dt.ToString("yyyyMM"); }
                dt = dt.AddMonths(-rangeEnd);

                for (int i = 0; i < range; i++)
                {
                    dt = dt.AddMonths(1);
                    lst.Add(new KeyValuePair<string, string>(dt.ToString("yyyyMM"), dt.ToString("MMMM-yyyy")));
                }

            }
            if (subArea.FREQUENCY.Equals(Helper.Constant.HALFYEARLY))
            {
                if (reportingPeriod == Helper.ReportingPeriod.NEXT) { dt = dt.AddMonths(6); }
                else if (reportingPeriod == Helper.ReportingPeriod.PREVIOUS) { dt = dt.AddMonths(-6); }

                if (currentForTimeOnTime == string.Empty) { currentForTimeOnTime = GetCurrentFinancialHalfYear(dt); }

                dt = dt.AddYears(-rangeEnd);
                for (int i = 0; i < range; i++)
                {
                    dt = dt.AddYears(1);

                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}07{1}12", dt.Year, dt.Year), string.Format("{0}-{1},{2}", dt.Year, dt.Year + 1, "H1")));
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}01{1}06", dt.Year + 1, dt.Year + 1), string.Format("{0}-{1},{2}", dt.Year, dt.Year + 1, "H2")));
                }
            }
            if (subArea.FREQUENCY.Equals(Helper.Constant.QUARTERLY))
            {
                if (reportingPeriod == Helper.ReportingPeriod.NEXT) { dt = dt.AddMonths(3); }
                else if (reportingPeriod == Helper.ReportingPeriod.PREVIOUS) { dt = dt.AddMonths(-3); }

                if (currentForTimeOnTime == string.Empty) { currentForTimeOnTime = GetCurrentFinancialQuarter(dt); }
                dt = dt.AddYears(-rangeEnd);
                for (int i = 0; i < range; i++)
                {
                    dt = dt.AddYears(1);
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}07{1}09", dt.Year, dt.Year), string.Format("{0}-{1},{2}", dt.Year, dt.Year + 1, "Q1")));
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}10{1}12", dt.Year, dt.Year), string.Format("{0}-{1},{2}", dt.Year, dt.Year + 1, "Q2")));
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}01{1}03", dt.Year + 1, dt.Year + 1), string.Format("{0}-{1},{2}", dt.Year, dt.Year + 1, "Q3")));
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}04{1}06", dt.Year + 1, dt.Year + 1), string.Format("{0}-{1},{2}", dt.Year, dt.Year + 1, "Q4")));
                }

            }
            if (subArea.FREQUENCY.Equals(Helper.Constant.WEEKLY))
            {
                range = 5;//Previous-->Current--->Next
                rangeEnd = 4;//Months
                if (reportingPeriod == Helper.ReportingPeriod.NEXT) { dt = dt.AddDays(7); }
                else if (reportingPeriod == Helper.ReportingPeriod.PREVIOUS) { dt = dt.AddDays(-7); }
                if (currentForTimeOnTime == string.Empty) { currentForTimeOnTime = GetCurrentFinancialWeekValue(dt); }
                dt = dt.AddMonths(-rangeEnd);

                for (int i = 0; i < range; i++)
                {
                    dt = dt.AddMonths(1);
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}1", dt.ToString("yyyyMM")), string.Format("{0},{1}", dt.ToString("MMMM-yyyy"), "W1")));
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}2", dt.ToString("yyyyMM")), string.Format("{0},{1}", dt.ToString("MMMM-yyyy"), "W2")));
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}3", dt.ToString("yyyyMM")), string.Format("{0},{1}", dt.ToString("MMMM-yyyy"), "W3")));
                    lst.Add(new KeyValuePair<string, string>(string.Format("{0}4", dt.ToString("yyyyMM")), string.Format("{0},{1}", dt.ToString("MMMM-yyyy"), "W4")));
                }
            }
            if (subArea.FREQUENCY.Equals(Helper.Constant.DAILY))
            {
                DateTime Previousmonth = DateTime.Now.AddMonths(-1);

                dt = new DateTime(Previousmonth.Year, Previousmonth.Month, 1);
                if (reportingPeriod == Helper.ReportingPeriod.NEXT) { dt = dt.AddDays(1); }
                else if (reportingPeriod == Helper.ReportingPeriod.PREVIOUS) { dt = dt.AddDays(-1); }
                if (currentForTimeOnTime == string.Empty) { currentForTimeOnTime = dt.ToString("yyyyMMdd"); }

                for (int i = 0; i < range; i++)
                {
                    lst.Add(new KeyValuePair<string, string>(dt.ToString("yyyyMMdd"), dt.ToString("dd-MM-yyyy")));
                    dt = dt.AddDays(1);
                }
            }
            onTimeForTime = currentForTimeOnTime;

            return new SelectList(lst.Select(s => new SelectListItem { Text = s.Value, Value = s.Key, Selected = s.Key == currentForTimeOnTime ? true : false }));

        }
        private string GetCurrentFinancialYear(DateTime dt)
        {
            string currentYear = string.Empty;

            if (dt.Month >= (int)Helper.Month.July)
                currentYear = string.Format("{0}07{1}06", dt.Year, dt.Year + 1);//string.Format("{0}-{1}", DateTime.Today.Year, (DateTime.Today.Year + 1));
            else
                currentYear = string.Format("{0}07{1}06", (dt.Year - 1), dt.Year);
            return currentYear;
        }
        private string GetCurrentFinancialHalfYear(DateTime dt)
        {
            string currentFinancialHalfYear = string.Empty;
            if (dt.Month >= (int)Helper.Month.July)
                currentFinancialHalfYear = string.Format("{0}07{1}12", dt.Year, dt.Year, "H1");
            else
                currentFinancialHalfYear = string.Format("{0}01{1}06", dt.Year, dt.Year, "H2");
            return currentFinancialHalfYear;
        }
        private string GetCurrentFinancialQuarter(DateTime dt)
        {
            string currentFinancialQuarter = string.Empty;
            if (dt.Month >= (int)Helper.Month.July && dt.Month <= (int)Helper.Month.September)
                currentFinancialQuarter = string.Format("{0}07{1}09", dt.Year, dt.Year);
            else
            if (dt.Month >= (int)Helper.Month.October && dt.Month <= (int)Helper.Month.December)
                currentFinancialQuarter = string.Format("{0}10{1}12", dt.Year, dt.Year);
            else
            if (dt.Month >= (int)Helper.Month.January && dt.Month <= (int)Helper.Month.March)
                currentFinancialQuarter = string.Format("{0}01{1}03", dt.Year, dt.Year);
            else
                currentFinancialQuarter = string.Format("{0}04{1}06", dt.Year, dt.Year);
            return currentFinancialQuarter;
        }
        private string GetCurrentFinancialWeekValue(DateTime dt)
        {

            string currentFinancialWeek = string.Empty;
            if (dt.Day <= 7) { currentFinancialWeek = string.Format("{0}1", dt.ToString("yyyyMM")); }
            else if (dt.Day > 7 && dt.Day <= 14) { currentFinancialWeek = string.Format("{0}2", dt.ToString("yyyyMM")); }
            else if (dt.Day > 14 && dt.Day <= 21) { currentFinancialWeek = string.Format("{0}3", dt.ToString("yyyyMM")); }
            else { currentFinancialWeek = string.Format("{0}4", dt.ToString("yyyyMM")); }
            return currentFinancialWeek;
        }
        private SelectList GranularTimeList(string granuleTimeValue)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (granuleTimeValue == Helper.Constant.HALFYEARLY)
            {
                list.Add(new SelectListItem { Text = "H1", Value = "H1" });
                list.Add(new SelectListItem { Text = "H2", Value = "H2" });
            }
            if (granuleTimeValue == Helper.Constant.QUARTERLY)
            {
                list.Add(new SelectListItem { Text = "Q1", Value = "Q1" });
                list.Add(new SelectListItem { Text = "Q2", Value = "Q2" });
                list.Add(new SelectListItem { Text = "Q3", Value = "Q3" });
                list.Add(new SelectListItem { Text = "Q4", Value = "Q4" });
            }
            if (granuleTimeValue == Helper.Constant.MONTHLY)
            {
                for (int i = (int)Helper.Month.January; i < (int)Helper.Month.December; i++)
                {
                    Helper.Month e = (Helper.Month)i;
                    list.Add(new SelectListItem { Text = e.ToString(), Value = e.ToString() });
                }
            }
            if (granuleTimeValue == Helper.Constant.WEEKLY)
            {
                list.Add(new SelectListItem { Text = "W1", Value = "W1" });
                list.Add(new SelectListItem { Text = "W2", Value = "W2" });
                list.Add(new SelectListItem { Text = "W3", Value = "W3" });
                list.Add(new SelectListItem { Text = "W4", Value = "W4" });
            }

            return null;

        }
        public JsonResult LoadLockDetails(int configid, string fortime, string ontimecode)
        {
            SPONGE_Context objFunction = new ();
            SPG_SENDORRESENDTASK ESRT = objFunction.SPG_SENDORRESENDTASK.Where(x => x.CONFIG_ID == configid).FirstOrDefault();
            var v1 = (from esrt in objFunction.SPG_SENDORRESENDTASK
                      where (esrt.FORTIMECODE == fortime && esrt.ONTIMECODE == ontimecode && esrt.CONFIG_ID == configid)
                      select new SendResendExcel { ID = esrt.ID, LockDate = esrt.LOCKDATE, UploadReminderDate = esrt.UPLOADREMINDERDATE, EsclationDate = esrt.ESCALATIONDATE }).OrderByDescending(s => s.ID).FirstOrDefault();

            if (ESRT == null)
            {
                SPG_CONFIGURATION EPM = objFunction.SPG_CONFIGURATION.Where(x => x.CONFIG_ID == configid).FirstOrDefault();
                if (EPM.SCHEDULED == "Y" && EPM.IS_POPULATED != null)
                {
                    v1.LockDate = DateTime.Now.AddDays(Convert.ToDouble(EPM.LOCK_DATE));
                    v1.UploadReminderDate = DateTime.Now.AddDays(Convert.ToDouble(EPM.REMMINDER_DATE));
                    v1.EsclationDate = DateTime.Now.AddDays(Convert.ToDouble(EPM.ESCALATION_DATE));
                    v1.IsPopluated = EPM.IS_POPULATED;
                }
                if (EPM.SCHEDULED == "N" && EPM.IS_POPULATED != null)
                {
                    v1 = new SendResendExcel();
                    v1.IsPopluated = EPM.IS_POPULATED;
                    v1.Error = 1;
                }

            }
            else
            {
                v1 = new SendResendExcel();
                v1.LockDate = objFunction.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configid && s.ONTIMECODE == ontimecode && s.FORTIMECODE == fortime).OrderByDescending(s => s.ID).Select(s => s.LOCKDATE).FirstOrDefault();
                v1.LockDate = v1.LockDate == Convert.ToDateTime("1/1/0001 12:00:00 AM") ? null : v1.LockDate;
                v1.UploadReminderDate = objFunction.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configid && s.ONTIMECODE == ontimecode && s.FORTIMECODE == fortime).OrderByDescending(s => s.ID).Select(s => s.UPLOADREMINDERDATE).FirstOrDefault() == Convert.ToDateTime("1/1/0001 12:00:00 AM") ? Convert.ToDateTime(null) : objFunction.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configid && s.ONTIMECODE == ontimecode && s.FORTIMECODE == fortime).OrderByDescending(s => s.ID).Select(s => s.UPLOADREMINDERDATE).FirstOrDefault();
                v1.UploadReminderDate = v1.UploadReminderDate == Convert.ToDateTime("1/1/0001 12:00:00 AM") ? null : v1.UploadReminderDate;
                v1.EsclationDate = objFunction.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configid && s.ONTIMECODE == ontimecode && s.FORTIMECODE == fortime).OrderByDescending(s => s.ID).Select(s => s.ESCALATIONDATE).FirstOrDefault() == Convert.ToDateTime("1/1/0001 12:00:00 AM") ? Convert.ToDateTime(null) : objFunction.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configid && s.ONTIMECODE == ontimecode && s.FORTIMECODE == fortime).OrderByDescending(s => s.ID).Select(s => s.ESCALATIONDATE).FirstOrDefault();
                v1.EsclationDate = v1.EsclationDate == Convert.ToDateTime("1/1/0001 12:00:00 AM") ? null : v1.EsclationDate;
                v1.IsPopluated = objFunction.SPG_SENDORRESENDTASK.Where(s => s.CONFIG_ID == configid && s.ONTIMECODE == ontimecode && s.FORTIMECODE == fortime).OrderByDescending(s => s.ID).Select(s => s.IS_POPULATED).FirstOrDefault();
            }

            return Json(v1);

        }

        public ActionResult SaveManualSendResendExcel(ManualSendResendExcelModel model)
        {
            SPONGE_Context objModel = new ();
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            if (ModelState.IsValid)
            {
                SPG_SENDORRESENDTASK sendresendtask = new SPG_SENDORRESENDTASK();
                sendresendtask.ATTEMPTS = 0;
                sendresendtask.CONFIG_ID = model.ConfigId;
                sendresendtask.SENT = 0;
                sendresendtask.LOCKDATE = model.LockDate;
                sendresendtask.UPLOADREMINDERDATE = model.UploadReminderDate;
                sendresendtask.ESCALATIONDATE = model.EscalationAlertDate;
                sendresendtask.CREATEDDATE = DateTime.Now;
                sendresendtask.CREATEDBY = userName[1];
                sendresendtask.FORTIMECODE = model.OnTime;
                sendresendtask.PERIOD_FROM = model.PeriodFrom;
                sendresendtask.PERIOD_TO = model.PeriodTo;
                sendresendtask.DATA_COLLECTION = "Offline";
                sendresendtask.IS_POPULATED = "Y";
                sendresendtask.ONTIMECODE = model.GranularTime == null ? model.OnTime : model.GranularTime;
                sendresendtask.IS_AUTO_MANUAL = "Manual";
                objModel.SPG_SENDORRESENDTASK.Add(sendresendtask);
                objModel.SaveChanges();
                return RedirectToAction("SearchTemplate", new { Success = 1 });
            }
            else
            {
                SPG_SUBJECTAREA subArea = objModel.SPG_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID.Equals(model.SubjectAreaId)).FirstOrDefault();
                string currentOnTimeForTime = string.Empty;

                var forTimeOnTimeList = GetForTimeOnTime(subArea, subArea.REPORTING_PERIOD, ref currentOnTimeForTime);
                ViewBag.ForTimeOnTime = forTimeOnTimeList.Items;
                ViewBag.OnTimeLevel = objModel.SPG_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID == model.SubjectAreaId).Select(s => s.ONTIMELEVEL).FirstOrDefault().ToString();

                
                //else if (ModelState.IsValidField("lock_date") == false)
                //{
                //    ViewBag.ErrorMsg = "Enter a valid Lock Date";
                //}
                //else if (ModelState.IsValidField("IsPopluated") == false)
                //{
                //    ViewBag.ErrorMsg = "Select Is Pre Populated?";
                //}

                return View("~/Views/Configuration/ManualSendResendExcel.cshtml", model);

            }

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
