using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using DAL;
using DAL.Models;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;
using OfficeOpenXml.Style;
using Sponge.Common;
using Sponge;

namespace BatchJob
{  /// <summary>
   /// Class for Main program.
   /// </summary>
    public class Program
    {

        private readonly AppSettings _settings;

        public Program()
        {
            var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                 .AddJsonFile("appsettings.json", false)
                 .Build();

            _settings = config.GetSection(nameof(AppSettings)).Get<AppSettings>();
        }

        public static void Main()
        {
            try
            {
                Console.WriteLine("----Start Job-----");
                Program batchJob = new Program();
                DataSet ds = new DataSet();
                Console.WriteLine("----Called Batch Job Procedure-----");
                ds = batchJob.GetConfigForExcelGeneration(0, "Y");
                //ds = batchJob.GetPrepopulatedConfigForExcelGeneration(0, "N");
                Console.WriteLine("----End Job-----");
            }
            catch (Exception ex)
            {
                ErrorLog srsEx = new ErrorLog();
                srsEx.LogErrorInTextFile(ex);
                //throw ex;
            }
        }
        private void HandleException(int TemplateId, Exception ex)
        {
            DeleteTemplateID(TemplateId);
            ErrorLog srsEx = new ErrorLog();
            srsEx.LogErrorInTextFile(ex);
        }
        public void PeriodFromPeriodTo(string fortime, string ontime, out string PeriodFrom, out string PeriodTo)
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

        public void DeleteTemplateID(int templateId)
        {
            if (templateId > 0)
            {
                string dynamicQuery = "DELETE FROM SPG_TEMPLATE WHERE TEMPLATE_ID =" + templateId;
                string FormedQueryLookupType = "SP_DELETE_DETAILS";
                GetDataSet objDeleteSetValue = new GetDataSet();
                objDeleteSetValue.DeleteUsingDynamicQuery(FormedQueryLookupType, dynamicQuery.ToString());

            }
        }
        public void SendEmailForERROR(decimal SubjectAreaId, int ConfigId)
        {
            SPONGE_Context m = new SPONGE_Context();

            var selectval = (from config in m.SPG_CONFIGURATION
                             join area in m.SPG_SUBJECTAREA on config.SUBJECTAREA_ID equals area.SUBJECTAREA_ID
                             join user in m.SPG_USERS on config.USER_ID equals user.USER_ID
                             where config.CONFIG_ID == ConfigId && area.SUBJECTAREA_ID == SubjectAreaId && user.ACTIVE_FLAG == "Y"
                             select new { UserName = user.Name, UserEmail = user.EMAIL_ID, SUBJECTAREANAME = area.SUBJECTAREA_NAME, LockDate = config.LOCK_DATE }).FirstOrDefault();


            NameValueCollection mailBodyplaceHolders = new NameValueCollection();
            mailBodyplaceHolders.Add("<UserName>", selectval.UserName);
            mailBodyplaceHolders.Add("<SubjectArea>", selectval.SUBJECTAREANAME);

            //Format the header    
            string DataCollectionSubject = "[Sponge] - Error in Excel template generation for Subject Area:  [" + selectval.SUBJECTAREANAME + "]";

            string mailbody = "";
            string messageTemplatePath = System.IO.File.ReadAllText(_settings.ErrorMailTemplate.ToString());

            mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);

            try
            {
                SendMailForErrorMsg(DataCollectionSubject, mailbody, selectval.UserEmail);
            }
            catch (Exception ex)
            {

            }

        }
        public void UpdateSendResendTasktable(int SendResendId, SPONGE_Context m)
        {
            if (SendResendId > 0)
            {
                SPG_SENDORRESENDTASK esppp = m.SPG_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
                esppp.SENT = 1;
                esppp.ATTEMPTS++;
                esppp.ID = SendResendId;
                m.SaveChanges();

            }
        }
        public void UpdateSendResendTasktableDatasetNull(int SendResendId, SPONGE_Context m)
        {
            if (SendResendId > 0)
            {
                SPG_SENDORRESENDTASK esppp = m.SPG_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
               
                esppp.ATTEMPTS++;
                esppp.ID = SendResendId;
                m.SaveChanges();

            }
        }
        public DataSet GetConfigForExcelGeneration(decimal configid, string p_IS_PREPOPULATE)
        {


            DataSet ds = new DataSet();
            ErrorLog lg = new ErrorLog();

            try
            {
                lg.LogTextInTextFile("Calling Batch Job");
                SPONGE_Context m = new SPONGE_Context();

                List<SPG_CONFIG_STRUCTURE> lstConfigEntity = new List<SPG_CONFIG_STRUCTURE>();
                GetDataSet gd = new GetDataSet();
                ErrorLog srsLogFile = new ErrorLog();
                srsLogFile.LogTextInTextFile("Calling Batch Job");
                ds = gd.GetBatchJobDataSetValue("SP_GET_CONFIG_BATCH", Convert.ToInt32(configid), p_IS_PREPOPULATE);
                CommonUtility objutility = new CommonUtility();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    bool IsGroupColumnNameExist = false;

                    int configId = Convert.ToInt32(dr["CONFIG_ID"]);

                    decimal SubjectAreaId = Convert.ToDecimal(dr["subjectarea_id"]);
                    string UserId = Convert.ToString(dr["Created_By"]);
                    try
                    {
                        DateTime dtlockdate = dr["LOCKDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["LOCKDATE"]);
                        DateTime dtesc = dr["ESCALATIONDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["ESCALATIONDATE"]);
                        DateTime dtrem = dr["UPLOADREMINDERDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["UPLOADREMINDERDATE"]);
                        int SendResendId = 0;
                        ForTimeOnTimeCode fp = new ForTimeOnTimeCode();
                        string frequency = Convert.ToString(dr["FREQUENCY"]);
                        string TimeLevel = Convert.ToString(dr["TIME_LEVEL"]);
                        string ReportingPeriod = Convert.ToString(dr["REPORTING_PERIOD"]);
                        string Granulaity = Convert.ToString(dr["ONTIMELEVEL"]);
                        string Version = Convert.ToString(dr["VERSION"]);
                        string DataCollection = Convert.ToString(dr["DATA_COLLECTION"]);
                        string PeriodFrom;
                        string PeriodTo;
                        string ForTime, OnTime;
                        int currentyear = fp.GetForTimeOnTimeVersionN(ReportingPeriod, frequency, Granulaity, Version, out ForTime, out OnTime, out PeriodFrom, out PeriodTo);
                        if (Version == "Y")
                        {
                            int nextyear = fp.GetForTimeOnTimeVersion(ReportingPeriod, frequency, Granulaity, Version, ref ForTime, out OnTime, ref PeriodFrom, ref PeriodTo); ;
                        }
                        bool IsFlagContinue = false;
                        string GetTodayDate = Convert.ToDateTime(DateTime.Now.Date).ToString("dd-MMM-yyyy");

                        var CheckScheduleTime = m.SPG_SENDORRESENDTASK.Where(w => w.CONFIG_ID == configId && (w.FORTIMECODE == ForTime) && w.ONTIMECODE == OnTime && w.SENT == 1 && w.PERIOD_FROM == null).ToList();
                        foreach (var item in CheckScheduleTime)
                        {
                            if (Convert.ToDateTime(item.CREATEDDATE).ToString("dd-MMM-yyyy") == GetTodayDate && Convert.ToString(dr["MANUALFLAG"]) == "AUTO")
                            {
                                IsFlagContinue = true;
                                break;
                            }
                        }
                        srsLogFile.LogTextInTextFile("checking  Auto/Manual Template" + configId);
                        if (IsFlagContinue == true)
                            continue;
                        if (Convert.ToString(dr["MANUALFLAG"]) == "AUTO")
                        {
                            SPG_SENDORRESENDTASK esp = new SPG_SENDORRESENDTASK();
                            esp.CONFIG_ID = configId;
                            esp.CREATEDBY = UserId;
                            esp.CREATEDDATE = DateTime.Now;
                            esp.LOCKDATE = dtlockdate;
                            esp.ESCALATIONDATE = dtesc;
                            esp.IS_AUTO_MANUAL = "AUTO";
                            esp.UPLOADREMINDERDATE = dtrem;
                            esp.FORTIMECODE = ForTime;
                            esp.ONTIMECODE = OnTime;
                            m.SPG_SENDORRESENDTASK.Add(esp);
                            m.SaveChanges();
                            SendResendId = esp.ID;
                            srsLogFile.LogTextInTextFile("Save Data into SPG_SENDRESENDTASK done" + configId);
                        }
                        else
                        {
                            ForTime = Convert.ToString(dr["FORTIMECODE"]);
                            OnTime = Convert.ToString(dr["ONTIMECODE"]);
                            PeriodFrom = Convert.ToString(dr["PERIOD_FROM"]);
                            PeriodTo = Convert.ToString(dr["PERIOD_TO"]);
                            SendResendId = Convert.ToInt32(dr["ID"]);
                        }

                        var ot_details = (from ep in m.SPG_CONFIG_STRUCTURE
                                          join pd in m.SPG_SUBJECTAREA on ep.SUBJECTAREA_ID equals pd.SUBJECTAREA_ID
                                          join pc in m.SPG_CONFIGURATION on ep.CONFIG_ID equals pc.CONFIG_ID
                                          join pf in m.SPG_SUBFUNCTION on pd.SUBFUNCTION_ID equals pf.SUBFUNCTION_ID
                                          join us in m.SPG_USERS on ep.USER_ID equals us.USER_ID
                                          where ep.CONFIG_ID == configId
                                          select new { SUBJECTAREAID = pd.SUBJECTAREA_ID, CONFIG_NAME = pc.Config_Name, COLLECTION_TYPE = pc.DATA_COLLECTION, SubjectArea = pd.SUBJECTAREA_NAME, ReportingPeriod = pd.REPORTING_PERIOD, Function = pf.FUNCTION_NAME, SubFunction = pf.SUBFUNCTION_NAME, UserId = us.ID, UserName = us.Name, UserEmail = us.EMAIL_ID }).FirstOrDefault();

                        //Custom
                        StringBuilder custom = new StringBuilder();
                        StringBuilder customexcel = new StringBuilder();
                        StringBuilder customonline = new StringBuilder();
                        srsLogFile.LogTextInTextFile("Before Calling  SP_GETMASTEREMAIL" + configId);
                        DataSet custom_ds = new DataSet();
                        custom_ds = gd.GetDataSetValue("SP_GETMASTEREMAIL", configId);
                        for (int v = 0; v < custom_ds.Tables[0].Rows.Count; v++)
                        {
                            custom.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
                            custom.Append("\r\n");
                            customexcel.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
                            customexcel.Append(",");
                            customonline.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
                            customonline.Append("<br/>");
                        }
                        string FileName = "";
                        String FileCode = "";
                        string FormedQuery3 = "";
                        DataSet ds3 = new DataSet();
                        DataSet ds33 = new DataSet();

                        int TemplateId = 0;
                        GetDataSet gds = new GetDataSet();
                        string documentidtest = "";
                        string documentid = "";
                        int TemplateChkId = 0;
                        documentidtest = gds.GetDataSetValueCheck("SP_CHECKTEMPLATE", configId, ForTime, OnTime, out TemplateChkId);
                        srsLogFile.LogTextInTextFile("After Calling  SP_CHECKTEMPLATE" + configId);
                        documentid = m.SPG_DOCUMENT.Where(w => w.TEMPLATEID == TemplateChkId && w.APPROVALSTATUSID != 4).OrderByDescending(d => d.UPLOADDATE).Select(s => s.ID).FirstOrDefault();

                        if (TemplateChkId == 0)
                        {
                            SPG_TEMPLATE SPGTemplate = new SPG_TEMPLATE();
                            SPGTemplate.CONFIG_ID = configId;
                            SPGTemplate.CREATEDBY = UserId;
                            SPGTemplate.CREATEDON = DateTime.Now;
                            SPGTemplate.ESCALATION_DATE = dtesc;
                            SPGTemplate.UPLOAD_REMINDER_DATE = dtrem;
                            SPGTemplate.LOCK_DATE = dtlockdate;
                            SPGTemplate.ONTIMECODE = OnTime;
                            SPGTemplate.FORTIMECODE = ForTime;
                            SPGTemplate.PERIOD_FROM = Convert.ToDateTime(DateTime.Parse(PeriodFrom.Trim()).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
                            SPGTemplate.PERIOD_TO = Convert.ToDateTime(DateTime.Parse(PeriodTo.Trim()).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));

                            //  SPGTemplate.FILE_NAME = "";
                            m.SPG_TEMPLATE.Add(SPGTemplate);
                            m.SaveChanges();
                            TemplateId = SPGTemplate.TEMPLATE_ID;
                            srsLogFile.LogTextInTextFile("After Saving Data in SPG_TEMPLATE" + configId);
                            FileCode = Guid.NewGuid().ToString();
                            objutility.GenerateDynamicColumnNames(configId, Convert.ToDateTime(PeriodFrom), Convert.ToDateTime(PeriodTo), TimeLevel, frequency, TemplateId, out IsGroupColumnNameExist, null);

                            FormedQuery3 = "GET_TEMPLATE_DATA";
                            using (GetDataSet objDataSetValue = new GetDataSet())
                            {
                                try
                                {
                                    ds3 = objDataSetValue.GetDataSetValueForBatchJob(FormedQuery3, configId, documentid, Convert.ToDateTime(SPGTemplate.PERIOD_TO).ToString("dd-MMM-yyyy"));
                                    srsLogFile.LogTextInTextFile("After Calling GET_TEMPLATE_DATA" + configId);

                                }
                                catch (Exception ex)
                                {
                                   
                                    HandleException(TemplateId, ex);
                                }


                               


                                if (ds3 == null)
                                {
                                    try
                                    {
                                        UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                        DeleteTemplateID(TemplateId);
                                        SendEmailForERROR(SubjectAreaId, configId);
                                        continue;

                                    }
                                    catch (Exception ex)
                                    {
                                        DeleteTemplateID(TemplateId);
                                        ErrorLog srsEx = new ErrorLog();
                                        srsEx.LogErrorInTextFile(ex);
                                        continue;

                                    }

                                }
                                else if (ds3.Tables.Count == 0)
                                {
                                    try
                                    {
                                        UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                        DeleteTemplateID(TemplateId);
                                        SendEmailForERROR(SubjectAreaId, configId);
                                        continue;
                                    }
                                    catch (Exception ex)
                                    {
                                        DeleteTemplateID(TemplateId);
                                        ErrorLog srsEx = new ErrorLog();
                                        srsEx.LogErrorInTextFile(ex);
                                        continue;

                                    }
                                }
                                else if (ds3.Tables[0].Rows.Count == 0)
                                {
                                    try
                                    {
                                        UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                        DeleteTemplateID(TemplateId);
                                        SendEmailForERROR(SubjectAreaId, configId);
                                        continue;
                                    }
                                    catch (Exception ex)
                                    {
                                        DeleteTemplateID(TemplateId);
                                        ErrorLog srsEx = new ErrorLog();
                                        srsEx.LogErrorInTextFile(ex);
                                        continue;

                                    }
                                }
                            }
                        }
                        else
                        {

                            var Templatedetails = (from ep in m.SPG_TEMPLATE
                                                   where ep.TEMPLATE_ID == TemplateChkId
                                                   select new { TemplateId = ep.TEMPLATE_ID, PeriodFrom = ep.PERIOD_FROM, PeriodTo = ep.PERIOD_TO, FileCode = ep.FILE_CODE }).FirstOrDefault();
                            TemplateId = Templatedetails.TemplateId;
                            PeriodFrom = Templatedetails.PeriodFrom.ToString();
                            PeriodTo = Templatedetails.PeriodTo.ToString();
                            try
                            {
                                var GetColumnNames = m.SPG_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == configId && w.COLLECTION_TYPE == "Measure" && w.GROUPCOLUMNNAME != null).Select(s => s.GROUPCOLUMNNAME).Distinct().FirstOrDefault();

                                if (GetColumnNames.Length > 0)
                                {
                                    IsGroupColumnNameExist = true;
                                }
                            }
                            catch (Exception exi)
                            {

                            }

                            if (DataCollection.ToUpper() == "OFFLINE" && string.IsNullOrEmpty(Templatedetails.FileCode))
                            {
                                FileCode = Guid.NewGuid().ToString();
                            }
                            else
                            {
                                FileCode = Templatedetails.FileCode;
                            }

                            if (string.IsNullOrEmpty(documentid) || documentid == "null")
                            {
                                FormedQuery3 = "GET_TEMPLATE_DATA";
                                using (GetDataSet objDataSetValue = new GetDataSet())
                                {

                                    ds3 = objDataSetValue.GetDataSetValueForBatchJob(FormedQuery3, configId, documentid, Convert.ToDateTime(PeriodTo).ToString("dd-MM-yyyy"));

                                    if (ds3 == null)
                                    {
                                        try
                                        {
                                            UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                            SendEmailForERROR(SubjectAreaId, configId);
                                            continue;
                                        }
                                        catch (Exception exe)
                                        {
                                            //DeleteTemplateID(TemplateId);
                                            ErrorLog srsEx = new ErrorLog();
                                            srsEx.LogErrorInTextFile(exe);
                                            continue;

                                        }

                                    }
                                    else if (ds3.Tables.Count == 0)
                                    {
                                        try
                                        {
                                            UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                            SendEmailForERROR(SubjectAreaId, configId);
                                            continue;
                                        }
                                        catch (Exception ex)
                                        {
                                            DeleteTemplateID(TemplateId);
                                            ErrorLog srsEx = new ErrorLog();
                                            srsEx.LogErrorInTextFile(ex);
                                            continue;

                                        }
                                    }
                                    else if (ds3.Tables[0].Rows.Count == 0)
                                    {
                                        try
                                        {
                                            UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                            SendEmailForERROR(SubjectAreaId, configId);
                                            continue;
                                        }
                                        catch (Exception ex)
                                        {
                                            //   DeleteTemplateID(TemplateId);
                                            ErrorLog srsEx = new ErrorLog();
                                            srsEx.LogErrorInTextFile(ex);
                                            continue;

                                        }
                                    }
                                }
                            
                            }
                            else
                            {

                                FormedQuery3 = "GET_TEMPLATE_DATA_EDIT";
                                using (GetDataSet objDataSetValue = new GetDataSet())
                                {

                                    ds3 = objDataSetValue.GetDataSetValueForEditBatchjob(FormedQuery3, configId, documentid, Convert.ToDateTime(PeriodTo).ToString("dd-MM-yyyy"));
                                    if (ds3 == null)
                                    {
                                        try
                                        {
                                            UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                            SendEmailForERROR(SubjectAreaId, configId);
                                            continue;
                                        }
                                        catch (Exception ex)
                                        {
                                            //DeleteTemplateID(TemplateId);
                                            ErrorLog srsEx = new ErrorLog();
                                            srsEx.LogErrorInTextFile(ex);
                                            continue;

                                        }

                                    }
                                    else if (ds3.Tables.Count == 0)
                                    {
                                        try
                                        {
                                            UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                            SendEmailForERROR(SubjectAreaId, configId);
                                            continue;
                                        }
                                        catch (Exception ex)
                                        {
                                            DeleteTemplateID(TemplateId);
                                            ErrorLog srsEx = new ErrorLog();
                                            srsEx.LogErrorInTextFile(ex);
                                            continue;

                                        }
                                    }
                                    else if (ds3.Tables[0].Rows.Count == 0)
                                    {
                                        try
                                        {
                                            UpdateSendResendTasktableDatasetNull(SendResendId, m);
                                            SendEmailForERROR(SubjectAreaId, configId);
                                            continue;
                                        }
                                        catch (Exception ex)
                                        {
                                            //DeleteTemplateID(TemplateId);
                                            ErrorLog srsEx = new ErrorLog();
                                            srsEx.LogErrorInTextFile(ex);
                                            continue;

                                        }
                                    }
                                }
                                //isViewData = true;

                            }



                        }

                        //if (DataCollection == "Online")
                        //{

                        //    string OntimeCode = ForTime;
                        //    string ForTimeCode = OnTime;
                        //    string Period = PeriodTo;
                        //    string onlineTemplateURL = System.Configuration.ConfigurationManager.AppSettings["HostName"] + "/" + ConfigurationManager.AppSettings["ApplicationName"] + "/OnlineFormView/ViewOnlineForm?Strid=" + configId.ToString() + "&StrsubjectAreaId=" + ot_details.SUBJECTAREAID.ToString() + "&UserID=" + ot_details.UserId + "&ForTimeCode=" + OntimeCode + "&OnTimeCode=" + ForTimeCode + "&PeriodTo=" + PeriodTo;
                        //    NameValueCollection mailBodyplaceHolders = new NameValueCollection();
                        //    string ChangedSubjectArea = ot_details.SubjectArea.Replace("_", " ");
                        //    ChangedSubjectArea = new CultureInfo("en-US").TextInfo.ToTitleCase(ChangedSubjectArea.ToLower());

                        //    mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
                        //    mailBodyplaceHolders.Add("<SubjectArea>", ChangedSubjectArea);
                        //    mailBodyplaceHolders.Add("<ForTimeCode>", ForTime);
                        //    mailBodyplaceHolders.Add("<OnTimeCode>", OnTime);
                        //    mailBodyplaceHolders.Add("<LockDate>", dtlockdate.ToString());
                        //    mailBodyplaceHolders.Add("<Custom>", customonline.ToString());
                        //    mailBodyplaceHolders.Add("<OnlineTemplateURL>", onlineTemplateURL);
                        //    //Format the header    

                        //    string DataCollectionSubject = "[Sponge] - Data collection template link for  [" + ot_details.SubjectArea + "] -[" + ot_details.ReportingPeriod + "]";
                        //    string mailbody = "";
                        //    string messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["OnlineTemplateDataUpload"].ToString());

                        //    mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                        //    try
                        //    {
                        //        SendMailIsHtml("", DataCollectionSubject, mailbody, ot_details.UserEmail);
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        SPG_SENDORRESENDTASK ESP = m.SPG_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
                        //        ESP.ATTEMPTS++;
                        //        m.SaveChanges();
                        //    }
                        //}
                        //else
                        //{
                        int MasterColumn = m.SPG_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Master").Select(s => new { CONFIG_ID = s.CONFIG_ID }).Count();
                        int MeasureColmnsCount = m.SPG_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Measure").Select(s => new { CONFIG_ID = s.CONFIG_ID }).Count();
                        var measurecolumn = m.SPG_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Measure").Select(s => new { Text = s.DATA_TYPE, Value = s.DISPLAY_TYPE, ConfigUserId = s.CONFIGUSER_ID }).OrderBy(s => s.ConfigUserId);
                        var MasterShowColumn = m.SPG_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Master").Select(s => new { DATA_TYPE = s.DATA_TYPE, IS_SHOW = s.IS_SHOW });

                        FileName = ot_details.SubjectArea + "_" + ot_details.CONFIG_NAME + "_[T" + TemplateId + "]_[" + ForTime + "]_[" + OnTime + "]_[" + DateTime.Now.ToString("dd-MM-yyyy") + "-" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + " " + DateTime.Now.ToString("tt ") + "]" + ".xlsx";
                        string FormedQuery4 = "SP_GETLOOKUPDATA";
                        DataSet ds4 = new DataSet();
                        try
                        {
                            srsLogFile.LogTextInTextFile("After SP_GETLOOKUPDATA" + configId);

                            using (GetDataSet objDataSetValue = new GetDataSet())
                            {
                                ds4 = objDataSetValue.GetDataSetValue(FormedQuery4, configId);
                            }
                        }
                        catch (Exception ex)
                        {

                            ErrorLog srsEx = new ErrorLog();
                            srsEx.LogErrorInTextFile(ex);
                        }
                        //ProcessDataSetHeader(ref ds3, ref ds33, configId);
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (ExcelPackage objExcelPackage = new ExcelPackage())
                        {

                            DataTable dsExcel = new DataTable();
                            // var SecondList = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.FIELD_NAME.Contains("_CODE") || s.FIELD_NAME.Contains("_CD"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
                            var SecondList = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.IS_SHOW == "N")).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID, CollectionType = s.COLLECTION_TYPE, DisplayType = s.DISPLAY_TYPE }).OrderBy(y => y.CONFIGUSER_ID).ToList();
                            // var FirstList = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && !(s.FIELD_NAME.Contains("_CD")) && (!s.FIELD_NAME.Contains("_CODE"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
                            var FirstList = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.IS_SHOW == "Y" || s.IS_SHOW == null)).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID, CollectionType = s.COLLECTION_TYPE, DisplayType = s.DISPLAY_TYPE }).OrderBy(y => y.CONFIGUSER_ID).ToList();
                            var totalList = FirstList.OrderBy(o => o.CollectionType).Concat(SecondList).ToList();
                            //var J = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && !(s.FIELD_NAME.Contains("_CD")) && (!s.FIELD_NAME.Contains("_CODE"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
                            //var M = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.FIELD_NAME.Contains("_CODE") || s.FIELD_NAME.Contains("_CD"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
                            //var N = J.Concat(M).ToList();
                            // var DisplayName = (from T in MasterShowColumn1 join R in N on T.DATA_TYPE equals R.DATA_TYPE select new { DISPLAY_NAME = R.DISPLAY_NAME, DATA_TYPE =R.DATA_TYPE }).ToList();
                            if (ds3.Tables.Count > 0)
                            {
                                DataTable table = ds3.Tables[0];

                                foreach (var column in totalList)
                                {
                                    var displayName = column.DISPLAY_NAME;
                                    if (IsGroupColumnNameExist)
                                    {
                                        var tempDisplayName = m.SPG_GETTIMECODE
                                                               .Where(w => w.CONFIG_ID == configId
                                                              && w.DATA_TYPE == column.DATA_TYPE
                                                              && w.TEMPLATE_ID == TemplateId)
                                                              .Select(s => s.DISPLAY_NAME.Trim()).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(tempDisplayName))
                                        {
                                            displayName = tempDisplayName;
                                        }
                                    }
                                    table.Columns[column.DATA_TYPE].ColumnName = displayName;
                                }

                                table.AcceptChanges();
                                dsExcel = table;
                            }
                            //foreach (DataTable dtSrc in dsExcel)
                            if (dsExcel.Rows.Count > 0)
                            {
                                SPONGE_Context spg = new();
                                var uomName = spg.SPG_CONFIG_STRUCTURE
                                .Where(w => w.CONFIG_ID == configId && w.COLLECTION_TYPE == "Measure")                                                              
                                                              .Select(w => w.UOM).FirstOrDefault();
                                ExcelWorksheet hiddenSheet = objExcelPackage.Workbook.Worksheets.Add(_settings.HiddenSheetName);
                                hiddenSheet.Cells[1000000, 500].Value = FileCode;// Need to pass File Code 
                                hiddenSheet.Hidden = eWorkSheetHidden.VeryHidden;
                                //Create the worksheet    
                                dsExcel.TableName = "DataCollectionSheet";
                                ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add(dsExcel.TableName);
                                //ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Data Collection Sheet");

                                objWorksheet.Cells["A1"].Value = ot_details.SubjectArea + "- Data Collection Sheet";
                                objWorksheet.Cells["A2"].Value = "Reporting Start Date-" + Convert.ToDateTime(PeriodFrom).ToString("dd/MMM/yyyy");
                                objWorksheet.Cells["B2"].Value = "Reporting End Date-" + Convert.ToDateTime(PeriodTo).ToString("dd/MMM/yyyy");
                                objWorksheet.Cells["A3"].Value = "Assigned User-" + ot_details.UserName;
                                objWorksheet.Cells["B3"].Value = "Lock Date-" + Convert.ToDateTime(dtlockdate).ToString("dd/MMM/yyyy");
                                objWorksheet.Cells["C3"].Value = "Generation Date-" + DateTime.Now.Date.ToString("dd/MMM/yyyy");
                                objWorksheet.Cells["D3"].Value = "Uom-" + uomName;
                                objWorksheet.Cells["A4"].Value = "";// customexcel;
                                objWorksheet.Row(4).Hidden = true;//Hide 4th row
                                objWorksheet.DefaultColWidth = 30;
                                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1 
                                try
                                {
                                    objWorksheet.Cells["A5"].LoadFromDataTable(dsExcel, true);
                                }
                                catch (Exception ex)
                                {
                                    string ErrorDetails = "Error Details:Total Rows " + dsExcel.Rows.Count + " and ConfigId :" + configId + " and Subject Area Name:" + ot_details.SubjectArea + "  Assigned User: " + ot_details.UserName;
                                    ErrorLog srsEx = new ErrorLog();
                                    srsEx.LogErrorInTextFile(ex);
                                    srsEx.LogTextInTextFile(ErrorDetails);
                                    continue;

                                }

                                objWorksheet.Cells.Style.Font.SetFromFont("Calibri", float.Parse("10"), false, false, false, false);
                                // char next='A';
                                char next = incrementCharacter('A', MasterColumn);
                                int xx = 0;
                                List<string> LstMeasureColumns = new List<string>();
                                List<string> LstGrandTotalColumns = new List<string>();
                                foreach (var T in totalList)
                                {


                                    foreach (var s in measurecolumn)
                                    {

                                        if (T.DATA_TYPE == s.Text && s.Value == "DROPDOWN" && T.CollectionType == "Measure")
                                        {
                                            next = incrementCharacter('A', xx);
                                            string resultchar = (next.ToString() + ':' + next.ToString()).ToString();
                                            IExcelDataValidation dataValidation;

                                            var lookuptype = objWorksheet.DataValidations.AddListValidation(resultchar.ToString());
                                            foreach (DataRow row in ds4.Tables[0].Rows) // Loop over the rows.
                                            {
                                                if (row.ItemArray[0].ToString() == s.Text)
                                                {
                                                    lookuptype.Formula.Values.Add(row.ItemArray[2].ToString());
                                                }
                                                dataValidation = lookuptype;
                                                dataValidation.ShowErrorMessage = true;
                                                dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                                                dataValidation.ErrorTitle = "An invalid value was entered";
                                                dataValidation.Error = "Select a value from the from  list";
                                            }
                                            LstMeasureColumns.Add(next.ToString());
                                            break;

                                        }
                                        //if (s.Value == "TEXTBOX" && T.DATA_TYPE.Contains("VC") && T.CollectionType == "Measure")
                                        if (s.Value == T.DisplayType && s.Value != "DROPDOWN" && T.DATA_TYPE.Contains("VC") && T.CollectionType == "Measure")
                                        {
                                            next = incrementCharacter('A', xx);
                                            //CellsNumeric(objWorksheet, next, 1);

                                            LstMeasureColumns.Add(next.ToString());
                                            LstGrandTotalColumns.Add(next.ToString());
                                            break;

                                        }
                                        //if (s.Value == "TEXTBOX" && T.DATA_TYPE.Contains("N") && T.CollectionType == "Measure")
                                        if (s.Value == T.DisplayType && T.DATA_TYPE.Contains("N") && T.CollectionType == "Measure")
                                        {
                                            next = incrementCharacter('A', xx);
                                            CellsNumeric(objWorksheet, next, 1);

                                            LstMeasureColumns.Add(next.ToString());
                                            LstGrandTotalColumns.Add(next.ToString());
                                            break;

                                        }
                                        // if (s.Value == "DATE" && T.DATA_TYPE.Contains("DT") && T.CollectionType == "Measure")
                                        if (s.Value == T.DisplayType && T.DATA_TYPE.Contains("DT") && T.CollectionType == "Measure")
                                        {
                                            next = incrementCharacter('A', xx);
                                            DateNumeric(objWorksheet, next, 1);
                                            LstMeasureColumns.Add(next.ToString());
                                            break;
                                        }
                                        // if (s.Value == "TEXTBOX" && T.DATA_TYPE.Contains("P") && T.CollectionType == "Measure")
                                        if (s.Value == T.DisplayType && T.DATA_TYPE.Contains("P") && T.CollectionType == "Measure")
                                        {
                                            next = incrementCharacter('A', xx);
                                            CellsNumericPercentage(objWorksheet, next, 1);
                                            LstMeasureColumns.Add(next.ToString());
                                            LstGrandTotalColumns.Add(next.ToString());
                                            break;
                                        }


                                    }
                                    xx++;
                                }
                                // char next = incrementCharacter('A', MasterColumn);
                                char nextmeasure = incrementCharacter(next, MeasureColmnsCount);
                                //string nextstring = (next.ToString() + ':' + nextmeasure.ToString()).ToString();
                                //int countindex = 1;
                                objWorksheet.Cells["A4:C4"].Merge = true;
                                objWorksheet.Protection.IsProtected = true;
                                if (LstMeasureColumns.Count > 0)
                                {
                                    //Unlock Measure Columns
                                    string S = LstMeasureColumns[0];
                                    string L = LstMeasureColumns[LstMeasureColumns.Count - 1];
                                    int LastRownumber = dsExcel.Rows.Count + 5;

                                    using (ExcelRange objRange = objWorksheet.Cells[S + "6" + ":" + L + LastRownumber])
                                    {
                                        objRange.Style.Locked = false;
                                    }
                                }
                                //dsExcel.Rows

                                objWorksheet.Protection.IsProtected = true;
                                if (!string.IsNullOrEmpty(FileCode))
                                    objWorksheet.Protection.SetPassword(FileCode);
                                objWorksheet.Cells.AutoFitColumns();

                                hiddenSheet.Cells.AutoFitColumns();

                                objExcelPackage.Workbook.Protection.LockStructure = true;
                                char total = incrementCharacter('A', (FirstList.Count - 1));//Remove unwanted column in excel 
                                /*Freeze Panes*/
                                objWorksheet.View.FreezePanes(6, 1);

                                using (ExcelRange objRange = objWorksheet.Cells["A5:" + total.ToString() + "5"])
                                {

                                    objRange.Style.Font.Bold = true;
                                    ////objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    ////objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Justify;
                                    ////objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    ////objRange.Style.Font.Color.SetColor(Color.Black);
                                    // objRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

                                    /*Border lines should be added*/
                                    //objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    //objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    //objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    //objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                                    objRange.AutoFilter = true;
                                    objWorksheet.Protection.AllowAutoFilter = true;

                                    //•	Hide isShow(which is true) Columns  from Excel.
                                    if (SecondList.Count > 0)
                                    {

                                        int Firstlistcloulmncount = FirstList.Count;
                                        for (int c = 1; c <= SecondList.Count; c++)
                                        {

                                            objWorksheet.Column(Firstlistcloulmncount + c).Hidden = true;
                                        }

                                    }
                                    //Border lines should be added
                                    int columnno = 6;
                                    int rows = dsExcel.Rows.Count;
                                    for (int c = 1; c <= rows; c++)
                                    {
                                        int rr = columnno;

                                        //objWorksheet.Cells["A6:" + next + "6"].Style.Locked = true;
                                        objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                        objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                        objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                        objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                        columnno++;
                                    }
                                    //SubTotal
                                    //objWorksheet.Cells["A" + (rows + 6).ToString() + ""]
                                    //Grand Total
                                    objWorksheet.Cells["A" + (rows + 6).ToString() + ""].Value = "Grand Total";

                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                                    /*Border lines should be added*/
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Font.Bold = true;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.VerticalAlignment = ExcelVerticalAlignment.Justify;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Font.Color.SetColor(Color.White);


                                    if (LstGrandTotalColumns.Count > 0)
                                    {
                                        for (int g = 0; g < LstGrandTotalColumns.Count; g++)
                                        {
                                            objWorksheet.Cells[LstGrandTotalColumns[g] + "" + (dsExcel.Rows.Count + 6) + ""].Formula = "=SUM(" + LstGrandTotalColumns[g] + "6:" + LstGrandTotalColumns[g] + "" + (dsExcel.Rows.Count + 5) + ")";
                                        }
                                    }
                                    //objWorksheet.Cells["F" + (dsExcel.Rows.Count + 6) + ""].Formula = "=SUM(E6:E" + (dsExcel.Rows.Count + 5) + ")";
                                }

                                using (ExcelRange objRange = objWorksheet.Cells["A1:" + total.ToString() + "1"])
                                {
                                    objRange.Style.Font.Bold = true;
                                    objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    objRange.Style.Font.Color.SetColor(Color.White);

                                    objRange.Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                                    /*Border lines should be added*/
                                    objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                                }
                                using (ExcelRange objRange = objWorksheet.Cells["A2:" + total.ToString() + "2"])
                                {
                                    objRange.Style.Font.Bold = true;
                                    objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    objRange.Style.Font.Color.SetColor(Color.White);
                                    objRange.Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                                    /*Border lines should be added*/
                                    objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                }
                                using (ExcelRange objRange = objWorksheet.Cells["A3:" + total.ToString() + "3"])
                                {
                                    objRange.Style.Font.Bold = true;
                                    objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    objRange.Style.Font.Color.SetColor(Color.White);
                                    objRange.Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                                    /*Border lines should be added*/
                                    objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                }
                                using (ExcelRange objRange = objWorksheet.Cells["A4:" + total.ToString() + "4"])
                                {
                                    objRange.Style.Font.Bold = true;
                                    objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    objRange.Style.Font.Color.SetColor(Color.White);
                                    objRange.Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                                    /*Border lines should be added*/
                                    objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                }


                                string filepath = _settings.BatchJobErrorlocation;
                                string path = Path.Combine(filepath, FileName);
                                //string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Excel"), FileName);
                                //Create excel file on physical disk    
                                FileStream objFileStrm = System.IO.File.Create(path);
                                objFileStrm.Close();
                                //Write content to excel file    
                                System.IO.File.WriteAllBytes(path, objExcelPackage.GetAsByteArray());
                                string ChangedSubjectArea = ot_details.SubjectArea.Replace("_", " ");
                                ChangedSubjectArea = new CultureInfo("en-US").TextInfo.ToTitleCase(ChangedSubjectArea.ToLower());

                               
                               // PeriodFromPeriodTo(ForTime, OnTime, out PeriodFrom, out PeriodTo);
                                NameValueCollection mailBodyplaceHolders = new NameValueCollection();
                                mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
                                mailBodyplaceHolders.Add("<SubjectArea>", ChangedSubjectArea);
                                mailBodyplaceHolders.Add("<ForTimeCode>", Convert.ToDateTime(PeriodFrom).ToString("MMM/dd/yyyy"));
                                mailBodyplaceHolders.Add("<OnTimeCode>", Convert.ToDateTime(PeriodTo).ToString("MMM/dd/yyyy"));
                                mailBodyplaceHolders.Add("<LockDate>", Convert.ToDateTime(dtlockdate.ToString()).ToString("MMM/dd/yyyy"));
                                mailBodyplaceHolders.Add("<Custom>", custom.ToString());
                                //Format the header    
                                string DataCollectionSubject = "[Sponge] - Data collection template for  [" + ot_details.SubjectArea + "] -[" + ot_details.ReportingPeriod + "]";
                                srsLogFile.LogTextInTextFile("Send Email to users" + configId);

                                string mailbody = "";
                                string messageTemplatePath = System.IO.File.ReadAllText(_settings.TemplateGenerationEmail.ToString());

                                mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);

                                srsLogFile.LogTextInTextFile("Send Email to users" + configId);


                                try
                                {

                                    SendMail(path, DataCollectionSubject, mailbody, ot_details.UserEmail);
                                }
                                catch (Exception ex)
                                {
                                    SPG_SENDORRESENDTASK ESP = m.SPG_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
                                    ESP.ATTEMPTS++;
                                    m.SaveChanges();
                                }
                            }
                        }


                        SPG_TEMPLATE EPM = m.SPG_TEMPLATE.Where(x => x.TEMPLATE_ID == TemplateId).FirstOrDefault();
                        if (FileName != "")
                        {
                            EPM.FILE_NAME = FileName;
                        }
                        EPM.UPDATEDBY = UserId;
                        EPM.UPDATEDON = DateTime.Now;
                        //Write File Code  or not 


                        if (documentid == "" || string.IsNullOrEmpty(documentid))
                        {

                            if (!string.IsNullOrEmpty(FileCode))
                            {
                                EPM.FILE_CODE = FileCode;
                            }

                        }
                        else if (!string.IsNullOrEmpty(documentid) && DataCollection.ToUpper() == "OFFLINE")
                        {
                            EPM.FILE_CODE = FileCode;
                        }

                        m.SaveChanges();
                        SPG_SENDORRESENDTASK esppp = m.SPG_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
                        esppp.SENT = 1;
                        esppp.ATTEMPTS++;
                        esppp.ID = SendResendId;
                        m.SaveChanges();
                    }

                    catch (Exception ex)
                    {
                        string ErrorDetails = "Error Details: Config Id :" + configId + " Subject Area Id" + SubjectAreaId + " and Assigned Userid : " + UserId;
                        ErrorLog srsEx = new ErrorLog();
                        srsEx.LogErrorInTextFile(ex, ErrorDetails);
                        continue;
                    }
                }//Main For loop Close
                SendUploadMail();
                SendEscalationMail();

            }
            catch (Exception ex)
            {
                string ErrorDetails = "Error Details: No data return from SP: SP_GETCONFIG_BATCH for IsPopulated 'Y' and  Function Name:GetConfigForExcelGeneration() ";
                ErrorLog srsEx = new ErrorLog();
                srsEx.LogErrorInTextFile(ex, ErrorDetails);
                //throw ex;

            }

            return ds;

        }
        public void SendUploadMail()
        {
            SPONGE_Context m = new SPONGE_Context();
            GetDataSet gd = new GetDataSet();
            DataSet ds = new DataSet();
            //ErrorLog lg = new ErrorLog();
            ErrorLog lg = new ErrorLog();
            ds = gd.GetDataSetValue("SP_GETUPLOADEMAIL", 0);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataSet custom_ds = new DataSet();
                    StringBuilder custom = new StringBuilder();
                    custom_ds = gd.GetDataSetValue("SP_GETMASTEREMAIL", Convert.ToInt32(ds.Tables[0].Rows[i].ItemArray[8]));
                    for (int v = 0; v < custom_ds.Tables[0].Rows.Count; v++)
                    {
                        custom.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
                        custom.Append("\r\n");
                    }
                    decimal configId = Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[8]);
                    decimal TemplateId = Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[0]);
                    var ot_details = (from ep in m.SPG_CONFIG_STRUCTURE
                                      join pd in m.SPG_SUBJECTAREA on ep.SUBJECTAREA_ID equals pd.SUBJECTAREA_ID
                                      join us in m.SPG_USERS on ep.USER_ID equals us.USER_ID
                                      where ep.CONFIG_ID == configId
                                      select new { UserName = us.Name, ReportingPeriod = pd.REPORTING_PERIOD, UserEmail = us.EMAIL_ID }).FirstOrDefault();
                    NameValueCollection mailBodyplaceHolders = new NameValueCollection
                    {
                        { "<FileName>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[9]) },
                        { "<UserName>", ot_details.UserName },
                        { "<SubjectArea>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[3]) },
                        { "<ForTimeCode>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[1]) },
                        { "<OnTimeCode>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[2]) },
                        { "<LockDate>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[6]) },
                        { "<Custom>", custom.ToString() }
                    };

                    string DataCollectionSubject = "[Sponge] - Upload Reminder template for  [" + Convert.ToString(ds.Tables[0].Rows[i].ItemArray[3]) + "] -[" + ot_details.ReportingPeriod + "]";
                    string mailbody = "";
                    string messageTemplatePath = "";

                    messageTemplatePath = System.IO.File.ReadAllText(_settings.UploadReminderMailExcelTemplate.ToString());

                    mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                    SendMail("", DataCollectionSubject, mailbody, ot_details.UserEmail);
                   lg.LogTextInTextFile("Mail Sent Upload Reminder ID - " + configId + DateTime.Now.ToString());


                    SPG_TEMPLATE et = m.SPG_TEMPLATE.Where(x => x.TEMPLATE_ID == TemplateId).FirstOrDefault();
                    et.UPLOAD_REMINDER_SENT = 1;
                    m.SaveChanges();
                     lg.LogTextInTextFile("Updated SPG_TEMPLATE ID-" + configId + DateTime.Now.ToString());
                }

            }
            catch (Exception ex)
            {
                ErrorLog srsEx = new ErrorLog();
                srsEx.LogErrorInTextFile(ex);

            }

        }

        public void SendEscalationMail()
        {
            SPONGE_Context m = new SPONGE_Context();
            GetDataSet gd = new GetDataSet();
            DataSet ds = new DataSet();
            ErrorLog lg = new ErrorLog();
            ds = gd.GetDataSetValue("SP_GETESCALATION", 0);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataSet custom_ds = new DataSet();
                    StringBuilder custom = new StringBuilder();
                    custom_ds = gd.GetDataSetValue("SP_GETMASTEREMAIL", Convert.ToInt32(ds.Tables[0].Rows[i].ItemArray[8]));
                    for (int v = 0; v < custom_ds.Tables[0].Rows.Count; v++)
                    {
                        custom.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
                        custom.Append("\r\n");
                    }
                    decimal configId = Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[8]);
                    decimal TemplateId = Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[0]);
                    var ot_details = (from ep in m.SPG_CONFIG_STRUCTURE
                                      join pd in m.SPG_SUBJECTAREA on ep.SUBJECTAREA_ID equals pd.SUBJECTAREA_ID
                                      join us in m.SPG_USERS on ep.USER_ID equals us.USER_ID
                                      join ap in m.SPG_CONFIGURATION on ep.CONFIG_ID equals ap.CONFIG_ID
                                      where ep.CONFIG_ID == configId
                                      select new { UserName = us.Name, ReportingPeriod = pd.REPORTING_PERIOD, UserEMail = us.EMAIL_ID, ApproverEmail = ap.APPROVER_EMAILD, ManagerEmail = us.EMAIL_ID }).FirstOrDefault();
                    NameValueCollection mailBodyplaceHolders = new NameValueCollection();
                    mailBodyplaceHolders.Add("<FileName>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[9]));
                    mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
                    mailBodyplaceHolders.Add("<SubjectArea>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[3]));
                    mailBodyplaceHolders.Add("<ForTimeCode>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[1]));
                    mailBodyplaceHolders.Add("<OnTimeCode>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[2]));
                    mailBodyplaceHolders.Add("<LockDate>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[6]));
                    mailBodyplaceHolders.Add("<Custom>", custom.ToString());

                    string DataCollectionSubject = "[Sponge] - Escalation template for  [" + Convert.ToString(ds.Tables[0].Rows[i].ItemArray[3]) + "] -[" + ot_details.ReportingPeriod + "]";
                    string mailbody = "";
                    string messageTemplatePath = "";

                    messageTemplatePath = System.IO.File.ReadAllText(_settings.EscalationMailExcelTemplate);

                    mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                    SendMail("", DataCollectionSubject, mailbody, ot_details.UserEMail);
                    SendMail("", DataCollectionSubject, mailbody, ot_details.ApproverEmail);
                    SendMail("", DataCollectionSubject, mailbody, ot_details.ManagerEmail);
                   lg.LogTextInTextFile("Mail Sent Esclataion Reminder" + DateTime.Now.ToString());

                    SPG_TEMPLATE et = m.SPG_TEMPLATE.Where(x => x.TEMPLATE_ID == TemplateId).FirstOrDefault();
                    et.ESCALATION_REMINDER_SENT = 1;
                    m.SaveChanges();
                    lg.LogTextInTextFile("Updated in SPG_Template  ID-" + configId + DateTime.Now.ToString());
                }
            }
            catch (Exception ex)
            {
                ErrorLog srsEx = new ErrorLog();
                srsEx.LogErrorInTextFile(ex);

            }
        }
        public void SendMailIsHtml(string filename, string subject, string mailbody, string MailID)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = _settings.SMTPHost;

            MailMessage mailMessage = new MailMessage();
            mailMessage.Body = mailbody;
            mailMessage.Subject = subject;
            mailMessage.From = new MailAddress(_settings.MailFrom);
            mailMessage.To.Add(new MailAddress(MailID));
            mailMessage.IsBodyHtml = true;
            if (!string.IsNullOrEmpty(filename))
            {
                mailMessage.Attachments.Add(new Attachment(filename));
            }

            smtpClient.Send(mailMessage);

        }

        public string GetMessageBody(string messageTemplate, NameValueCollection nvc)
        {
            messageTemplate = ReplacePlaceHolders(messageTemplate, nvc);
            return messageTemplate;
        }

        private string ReplacePlaceHolders(string text, NameValueCollection valueCollection)
        {
            if (valueCollection == null || valueCollection.Count <= 0)
            {
                throw new ArgumentException("Invalid NameValueCollection");
            }
            //string text=null;
            string result = text;
            string value;
            foreach (string key in valueCollection.AllKeys)
            {
                value = valueCollection[key];
                result = result.Replace(key, value);
            }
            return result;
        }


        public void SendMailForErrorMsg(string subject, string mailbody, string MailID)
        {


            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = _settings.SMTPHost;

            MailMessage mailMessage = new MailMessage();
            mailMessage.Body = mailbody;
            mailMessage.Subject = subject;
            mailMessage.From = new MailAddress(_settings.MailFrom);
            mailMessage.To.Add(new MailAddress(MailID));
            //mailMessage.IsBodyHtml = true;
            //if (filename != "")
            //{
            //    mailMessage.Attachments.Add(new Attachment(filename));
            //}

            smtpClient.Send(mailMessage);
        }
        public void SendMail(string filename, string subject, string mailbody, string MailID)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = _settings.SMTPHost;

            MailMessage mailMessage = new MailMessage();
            mailMessage.Body = mailbody;
            mailMessage.Subject = subject;
            mailMessage.From = new MailAddress(_settings.MailFrom);
            mailMessage.To.Add(new MailAddress(MailID));
            //mailMessage.IsBodyHtml = true;
            if (filename != "")
            {
                var attachment = filename.Split(',');
                foreach (var item in attachment)
                {
                    mailMessage.Attachments.Add(new Attachment(item));
                }
            }

            smtpClient.Send(mailMessage);

        }
        public static DataTable MergeTablesByIndex(DataTable t1, DataTable t2)
        {
            if (t1 == null || t2 == null) throw new ArgumentNullException("t1 or t2", "Both tables must not be null");

            DataTable t3 = t1.Clone();  // first add columns from table1
            foreach (DataColumn col in t2.Columns)
            {
                string newColumnName = col.ColumnName;
                int colNum = 1;
                while (t3.Columns.Contains(newColumnName))
                {
                    newColumnName = string.Format("{0}_{1}", col.ColumnName, ++colNum);
                }
                t3.Columns.Add(newColumnName, col.DataType);
            }
            var mergedRows = t1.AsEnumerable().Zip(t2.AsEnumerable(),
                (r1, r2) => r1.ItemArray.Concat(r2.ItemArray).ToArray());
            foreach (object[] rowFields in mergedRows)
                t3.Rows.Add(rowFields);

            return t3;
        }
        private void ProcessDataSetHeader(ref DataSet ds, ref DataSet ds1, decimal id)
        {
            SPONGE_Context m = new SPONGE_Context();
            //var j = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == id).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID);
            var G = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == id && (s.FIELD_NAME.Contains("_CODE") || s.FIELD_NAME.Contains("_CD"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
            var H = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == id && !(s.FIELD_NAME.Contains("_CD")) && (!s.FIELD_NAME.Contains("_CODE"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
            // var I = m.SPG_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (!s.FIELD_NAME.Contains("_CODE"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
            var j = H.Concat(G);
            DataTable table = ds.Tables[0];
            DataTable table1 = ds1.Tables[0];
            table = MergeTablesByIndex(table, table1);
            //table.Merge(table1);

            foreach (var column in j)
            {
                table.Columns[column.DATA_TYPE].ColumnName = column.DISPLAY_NAME;
            }
            table.AcceptChanges();
        }
        char incrementCharacter(char input, int number)
        {
            return (input == 'z' ? 'a' : (char)(input + number));
        }
        private void CellsNumeric(ExcelWorksheet objWorksheet, char nextcharacter, int number)
        {

            string resultchar = (nextcharacter.ToString() + ':' + nextcharacter.ToString()).ToString();
            var val = objWorksheet.DataValidations.AddDecimalValidation(resultchar.ToString());
            val.ShowErrorMessage = true;
            IExcelDataValidation dataValidation;
            long minValue = Convert.ToInt64(_settings.NumberMin);
            long maxValue = Convert.ToInt64(_settings.NumberMax);

            // If UoM1 == 9Litres then Decimal else Integer

            val.Formula.Value = minValue;
            val.Formula2.Value = maxValue;
            dataValidation = val;

            dataValidation.Error = "Enter The Value between " + minValue + "  to  " + maxValue + "";
            dataValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
            dataValidation.ErrorTitle = "Input Validation";
            dataValidation.ShowErrorMessage = true;
            dataValidation.AllowBlank = true;

            objWorksheet.Cells[resultchar].Style.Numberformat.Format = "#.00";
           // objWorksheet.Cells[resultchar].Style.Numberformat.Format = "####";

        }
        private void DateNumeric(ExcelWorksheet objWorksheet, char nextcharacter, int number)
        {

            string resultchar = (nextcharacter.ToString() + ':' + nextcharacter.ToString()).ToString();
            var val = objWorksheet.DataValidations.AddDateTimeValidation(resultchar.ToString());
            val.ShowErrorMessage = true;
            IExcelDataValidation dataValidation;
            DateTime minValue = DateTime.MinValue;
            DateTime maxValue = DateTime.MaxValue;

            // If UoM1 == 9Litres then Decimal else Integer

            val.Formula.Value = minValue;
            val.Formula2.Value = maxValue;
            dataValidation = val;

            dataValidation.Error = "Error! Invalid date.Date format should be MM/DD/YYYY";//"Invalid Date! ";
            dataValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
            dataValidation.ErrorTitle = "Input Validation";
            dataValidation.ShowErrorMessage = true;
            dataValidation.AllowBlank = true;
            //objWorksheet.Cells[resultchar].Value="MM/DD/YYYY";
            objWorksheet.Cells[resultchar].AutoFitColumns();
            objWorksheet.Cells[resultchar].Style.Numberformat.Format = "MM/DD/YYYY";



        }

        private void CellsNumericPercentage(ExcelWorksheet objWorksheet, char nextcharacter, int number)
        {

            string resultchar = (nextcharacter.ToString() + ':' + nextcharacter.ToString()).ToString();
            //var val = objWorksheet.DataValidations.AddIntegerValidation(resultchar.ToString());
            var val = objWorksheet.DataValidations.AddDecimalValidation(resultchar.ToString());
            val.ShowErrorMessage = true;
            IExcelDataValidation dataValidation;
            double minValue = Convert.ToDouble(_settings.NumberMin);
            double maxValue = Convert.ToDouble(_settings.NumberMax);


            // If UoM1 == 9Litres then Decimal else Integer

            val.Formula.Value = minValue;
            val.Formula2.Value = maxValue;
            dataValidation = val;

            dataValidation.Error = "Enter The Value between " + minValue + " to " + maxValue + "";
            dataValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
            dataValidation.ErrorTitle = "Input Validation";
            dataValidation.ShowErrorMessage = true;
            dataValidation.AllowBlank = true;

            objWorksheet.Cells[resultchar].Style.Numberformat.Format = "##.00";

        }
    }

}




