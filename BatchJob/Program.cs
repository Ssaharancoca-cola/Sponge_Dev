//using System.Collections.Specialized;
//using System.Data;
//using System.Drawing;
//using System.Globalization;
//using System.Net.Mail;
//using System.Text;
//using DAL;
//using DAL.Models;
//using OfficeOpenXml;
//using OfficeOpenXml.DataValidation.Contracts;
//using Sponge.Common;
//namespace BatchJob
//{  /// <summary>
//   /// Class for Main program.
//   /// </summary>
//    public class Program
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        public static void Main()
//        {
//            try
//            {
//                Console.WriteLine("----Start Job-----");
//                Program batchJob = new Program();
//                DataSet ds = new DataSet();

//                ds = batchJob.GetConfigForExcelGeneration(0, "Y");
//                ds = batchJob.GetPrepopulatedConfigForExcelGeneration(0, "N");
//                Console.WriteLine("----End Job-----");
//            }
//            catch (Exception ex)
//            {
//                ErrorLog srsEx = new ErrorLog();
//                srsEx.LogErrorInTextFile(ex);
//                //throw ex;
//            }
//        }
//        public void PeriodFromPeriodTo(string fortime, string ontime, string frequency, string TimeLevel, out string PeriodFrom, out string PeriodTo)
//        {
//            PeriodFrom = "";
//            PeriodTo = "";
//            int Formonth = 0;
//            int Tomonth = 0;
//            Formonth = Convert.ToInt32(fortime.Substring(4, 2));
//            string ToYear = "";
//            if (fortime.Length > 6)
//            {
//                Tomonth = Convert.ToInt32(ontime.Substring(10, 2));
//                ToYear = ontime.Substring(6, 4);
//            }
//            else
//            {
//                Tomonth = Convert.ToInt32(ontime.Substring(4, 2));
//                ToYear = ontime.Substring(0, 4);
//            }
//            string ForMonthDIsplay = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(Formonth);
//            string ToMonthDIsplay = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(Tomonth);
//            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(Tomonth);

//            int year = Convert.ToInt32(fortime.Substring(0, 4));
//            int LastDay = DateTime.DaysInMonth(year, Tomonth);
//            PeriodFrom = string.Format("01-{0}-{1}", ForMonthDIsplay, fortime.Substring(0, 4));
//            PeriodTo = string.Format("{0}-{1}-{2}", LastDay.ToString(), ToMonthDIsplay, ToYear);

//        }

//        public void DeleteTemplateID(decimal templateId)
//        {
//            if (templateId > 0)
//            {
//                string dynamicQuery = "DELETE FROM EP_TEMPLATE WHERE TEMPLATE_ID =" + templateId;
//                string FormedQueryLookupType = "SP_DELETE_DETAILS";
//                GetDataSet objDeleteSetValue = new GetDataSet();
//                objDeleteSetValue.DeleteUsingDynamicQuery(FormedQueryLookupType, dynamicQuery.ToString());

//            }
//        }
//        public void SendEmailForERROR(decimal SubjectAreaId, int ConfigId)
//        {
//            SPONGE_Context m = new SPONGE_Context();

//            var selectval = (from config in m.POC_CONFIGURATION
//                             join area in m.EP_SUBJECTAREA on config.SUBJECTAREA_ID equals area.SUBJECTAREA_ID
//                             join user in m.EP_USERS on config.USER_ID equals user.ID
//                             where config.CONFIG_ID == ConfigId && area.SUBJECTAREA_ID == SubjectAreaId && user.ACTIVE_FLAG == "Y"
//                             select new { UserName = user.NAME, UserEmail = user.EMAILID, SUBJECTAREANAME = area.SUBJECTAREA_NAME, LockDate = config.LOCK_DATE }).FirstOrDefault();
           

//            NameValueCollection mailBodyplaceHolders = new NameValueCollection();
//            mailBodyplaceHolders.Add("<UserName>", selectval.UserName);
//            mailBodyplaceHolders.Add("<SubjectArea>", selectval.SUBJECTAREANAME);
            
//            //Format the header    
//            string DataCollectionSubject = "[iQlik Portal] - Error in Excel template generation for Subject Area:  [" + selectval.SUBJECTAREANAME + "]";

//            string mailbody = "";
//            string messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["ErrorMailTemplate"].ToString());

//            mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);

//            try
//            {
//                SendMailForErrorMsg(DataCollectionSubject, mailbody, selectval.UserEmail);
//            }
//            catch (Exception ex)
//            {

//            }

//        }
//        public void UpdateSendResendTasktable(decimal SendResendId, SPONGE_Context m)
//        {
//            if (SendResendId > 0)
//            {
//                EP_SENDORRESENDTASK esppp = m.EP_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
//                esppp.SENT = 1;
//                esppp.ATTEMPTS++;
//                esppp.ID = SendResendId;
//                m.SaveChanges();

//            }
//        }
//        public DataSet GetConfigForExcelGeneration(decimal configid, string p_IS_PREPOPULATE)
//        {
//            DataSet ds = new DataSet();
//            ErrorLog lg = new ErrorLog();

//            try
//            {

//                SPONGE_Context m = new SPONGE_Context();

//                List<POC_CONFIG_STRUCTURE> lstConfigEntity = new List<POC_CONFIG_STRUCTURE>();
//                GetDataSet gd = new GetDataSet();              
//                ds = gd.GetBatchJobDataSetValue("SP_GETCONFIG_BATCH", Convert.ToInt32(configid), p_IS_PREPOPULATE);
//                CommonUtility objutility = new CommonUtility();
                
//                foreach (DataRow dr in ds.Tables[0].Rows)
//                {

//                    bool IsGroupColumnNameExist = false;

//                    int configId = Convert.ToInt32(dr["CONFIG_ID"]);

//                    decimal SubjectAreaId = Convert.ToDecimal(dr["subjectarea_id"]);
//                    string UserId = Convert.ToString(dr["Created_By"]);
//                    try
//                    {
//                        DateTime dtlockdate = dr["LOCKDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["LOCKDATE"]);
//                        DateTime dtesc = dr["ESCALATIONDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["ESCALATIONDATE"]);
//                        DateTime dtrem = dr["UPLOADREMINDERDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["UPLOADREMINDERDATE"]);
//                        decimal SendResendId = 0;
//                        ForTimeOnTimeCode fp = new ForTimeOnTimeCode();
//                        string frequency = Convert.ToString(dr["FREQUENCY"]);
//                        string TimeLevel = Convert.ToString(dr["TIME_LEVEL"]);
//                        string ReportingPeriod = Convert.ToString(dr["REPORTING_PERIOD"]);
//                        string Granulaity = Convert.ToString(dr["ONTIMELEVEL"]);
//                        string Version = Convert.ToString(dr["VERSION"]);
//                        string DataCollection = Convert.ToString(dr["DATA_COLLECTION"]);
//                        string PeriodFrom;
//                        string PeriodTo;
//                        string ForTime, OnTime;
//                        int currentyear = fp.GetForTimeOnTimeVersionN(ReportingPeriod, frequency, Granulaity, Version, out ForTime, out OnTime, out PeriodFrom, out PeriodTo);
//                        if (Version == "Y")
//                        {
//                            int nextyear = fp.GetForTimeOnTimeVersion(ReportingPeriod, frequency, Granulaity, Version, ref ForTime, out OnTime, ref PeriodFrom, ref PeriodTo); ;
//                        }
//                        bool IsFlagContinue = false;
//                        string GetTodayDate = Convert.ToDateTime(DateTime.Now.Date).ToString("dd-MMM-yyyy");
                        
//                        var CheckScheduleTime = m.EP_SENDORRESENDTASK.Where(w => w.CONFIG_ID == configId && (w.FORTIMECODE == ForTime) && w.ONTIMECODE == OnTime && w.SENT == 1 && w.PERIOD_FROM == null).ToList();
//                        foreach (var item in CheckScheduleTime)
//                        {
//                            if (Convert.ToDateTime(item.CREATEDDATE).ToString("dd-MMM-yyyy") == GetTodayDate && Convert.ToString(dr["MANUALFLAG"]) == "AUTO")
//                            {
//                                IsFlagContinue = true;
//                                break;
//                            }
//                        }

//                        if (IsFlagContinue == true)
//                            continue;
//                        if (Convert.ToString(dr["MANUALFLAG"]) == "AUTO")
//                        {
//                            EP_SENDORRESENDTASK esp = new EP_SENDORRESENDTASK();
//                            esp.CONFIG_ID = configId;
//                            esp.CREATEDBY = UserId;
//                            esp.CREATEDDATE = DateTime.Now;
//                            esp.LOCKDATE = dtlockdate;
//                            esp.ESCALATIONDATE = dtesc;
//                            esp.IS_AUTO_MANUAL = "AUTO";
//                            esp.UPLOADREMINDERDATE = dtrem;
//                            esp.FORTIMECODE = ForTime;
//                            esp.ONTIMECODE = OnTime;
//                            m.EP_SENDORRESENDTASK.Add(esp);
//                            m.SaveChanges();
//                            SendResendId = esp.ID;

//                        }
//                        else
//                        {
//                            ForTime = Convert.ToString(dr["FORTIMECODE"]);
//                            OnTime = Convert.ToString(dr["ONTIMECODE"]);
//                            PeriodFrom = Convert.ToString(dr["PERIOD_FROM"]);
//                            PeriodTo = Convert.ToString(dr["PERIOD_TO"]);
//                            SendResendId = Convert.ToInt32(dr["ID"]);
//                        }

//                        var ot_details = (from ep in m.POC_CONFIG_STRUCTURE
//                                          join pd in m.EP_SUBJECTAREA on ep.SUBJECTAREA_ID equals pd.SUBJECTAREA_ID
//                                          join pc in m.POC_CONFIGURATION on ep.CONFIG_ID equals pc.CONFIG_ID
//                                          join pf in m.EP_SUBFUNCTION on pd.SUBFUNCTION_ID equals pf.SUBFUNCTION_ID
//                                          join us in m.EP_USERS on ep.USER_ID equals us.ID
//                                          where ep.CONFIG_ID == configId
//                                          select new { SUBJECTAREAID = pd.SUBJECTAREA_ID, COLLECTION_TYPE = pc.DATA_COLLECTION, SubjectArea = pd.SUBJECTAREA_NAME, ReportingPeriod = pd.REPORTING_PERIOD, Function = pf.FUNCTION_NAME, SubFunction = pf.SUBFUNCTION_NAME, UserId = us.ID, UserName = us.NAME, UserEmail = us.EMAILID }).FirstOrDefault();

//                        //Custom
//                        StringBuilder custom = new StringBuilder();
//                        StringBuilder customexcel = new StringBuilder();
//                        StringBuilder customonline = new StringBuilder();

//                        DataSet custom_ds = new DataSet();
//                        custom_ds = gd.GetDataSetValue("SP_GETMASTEREMAIL", configId);
//                        for (int v = 0; v < custom_ds.Tables[0].Rows.Count; v++)
//                        {
//                            custom.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
//                            custom.Append("\r\n");
//                            customexcel.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
//                            customexcel.Append(",");
//                            customonline.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
//                            customonline.Append("<br/>");
//                        }
//                        string FileName = "";
//                        String FileCode = "";
//                        string FormedQuery3 = "";
//                        DataSet ds3 = new DataSet();
//                        DataSet ds33 = new DataSet();

//                        decimal TemplateId = 0;
//                        GetDataSet gds = new GetDataSet();
//                        string documentidtest = "";
//                        string documentid = "";
//                        int TemplateChkId = 0;
//                        documentidtest = gds.GetDataSetValueCheck("SP_CHECKTEMPLATE", configId, ForTime, OnTime, out TemplateChkId);

//                        documentid = m.EP_DOCUMENT.Where(w => w.TEMPLATEID == TemplateChkId && w.APPROVALSTATUSID != 4).OrderByDescending(d => d.UPLOADDATE).Select(s => s.ID).FirstOrDefault();

//                        if (TemplateChkId == 0)
//                        {
//                            EP_TEMPLATE EPTemplate = new EP_TEMPLATE();
//                            EPTemplate.CONFIG_ID = configId;
//                            EPTemplate.CREATEDBY = UserId;
//                            EPTemplate.CREATEDON = DateTime.Now;
//                            EPTemplate.ESCALATION_DATE = dtesc;
//                            EPTemplate.UPLOAD_REMINDER_DATE = dtrem;
//                            EPTemplate.LOCK_DATE = dtlockdate;
//                            EPTemplate.ONTIMECODE = OnTime;
//                            EPTemplate.FORTIMECODE = ForTime;
//                            EPTemplate.PERIOD_FROM = Convert.ToDateTime(DateTime.Parse(PeriodFrom.Trim()).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
//                            EPTemplate.PERIOD_TO = Convert.ToDateTime(DateTime.Parse(PeriodTo.Trim()).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));

//                            //  EPTemplate.FILE_NAME = "";
//                            m.EP_TEMPLATE.Add(EPTemplate);
//                            m.SaveChanges();
//                            TemplateId = EPTemplate.TEMPLATE_ID;

//                            FileCode = Guid.NewGuid().ToString();
//                           objutility.GenerateDynamicColumnNames(configId, Convert.ToDateTime(PeriodFrom), Convert.ToDateTime(PeriodTo), TimeLevel, frequency, TemplateId, out IsGroupColumnNameExist, null);

//                            FormedQuery3 = "GETTEMPLATEDATANEW";
//                            using (GetDataSet objDataSetValue = new GetDataSet())
//                            {
//                                ds3 = objDataSetValue.GetDataSetValueForBatchJob(FormedQuery3, configId, documentid, Convert.ToDateTime(EPTemplate.PERIOD_TO).ToString("dd-MMM-yyyy"));

//                                if (ds3 == null)
//                                {
//                                    try
//                                    {
//                                        UpdateSendResendTasktable(SendResendId, m);
//                                        DeleteTemplateID(TemplateId);
//                                        SendEmailForERROR(SubjectAreaId, configId);
//                                        continue;

//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        DeleteTemplateID(TemplateId);
//                                        ErrorLog srsEx = new ErrorLog();
//                                        srsEx.LogErrorInTextFile(ex);
//                                        continue;

//                                    }

//                                }
//                                else if (ds3.Tables.Count == 0)
//                                {
//                                    try
//                                    {
//                                        UpdateSendResendTasktable(SendResendId, m);
//                                        DeleteTemplateID(TemplateId);
//                                        SendEmailForERROR(SubjectAreaId, configId);
//                                        continue;
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        DeleteTemplateID(TemplateId);
//                                        ErrorLog srsEx = new ErrorLog();
//                                        srsEx.LogErrorInTextFile(ex);
//                                        continue;

//                                    }
//                                }
//                                else if (ds3.Tables[0].Rows.Count == 0)
//                                {
//                                    try
//                                    {
//                                        UpdateSendResendTasktable(SendResendId, m);
//                                        DeleteTemplateID(TemplateId);
//                                        SendEmailForERROR(SubjectAreaId, configId);
//                                        continue;
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        DeleteTemplateID(TemplateId);
//                                        ErrorLog srsEx = new ErrorLog();
//                                        srsEx.LogErrorInTextFile(ex);
//                                        continue;

//                                    }
//                                }
//                            }
//                        }
//                        else
//                        {

//                            var Templatedetails = (from ep in m.EP_TEMPLATE
//                                                   where ep.TEMPLATE_ID == TemplateChkId
//                                                   select new { TemplateId = ep.TEMPLATE_ID, PeriodFrom = ep.PERIOD_FROM, PeriodTo = ep.PERIOD_TO, FileCode = ep.FILE_CODE }).FirstOrDefault();
//                            TemplateId = Templatedetails.TemplateId;
//                            PeriodFrom = Templatedetails.PeriodFrom.ToString();
//                            PeriodTo = Templatedetails.PeriodTo.ToString();
//                            try
//                            {
//                                var GetColumnNames = m.POC_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == configId && w.COLLECTION_TYPE == "Measure" && w.GROUPCOLUMNNAME != null).Select(s => s.GROUPCOLUMNNAME).Distinct().FirstOrDefault();

//                                if (GetColumnNames.Length > 0)
//                                {
//                                    IsGroupColumnNameExist = true;
//                                }
//                            }
//                            catch (Exception exi)
//                            {

//                            }

//                            if (DataCollection.ToUpper() == "OFFLINE" && string.IsNullOrEmpty(Templatedetails.FileCode))
//                            {
//                                FileCode = Guid.NewGuid().ToString();
//                            }
//                            else
//                            {
//                                FileCode = Templatedetails.FileCode;
//                            }
                            
//                            if (string.IsNullOrEmpty(documentid) || documentid == "null")
//                            {
//                                FormedQuery3 = "GETTEMPLATEDATANEW";
//                                using (GetDataSet objDataSetValue = new GetDataSet())
//                                {

//                                    ds3 = objDataSetValue.GetDataSetValueForBatchJob(FormedQuery3, configId, documentid, Convert.ToDateTime(PeriodTo).ToString("dd-MMM-yyyy"));

//                                    if (ds3 == null)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception exe)
//                                        {
//                                            //DeleteTemplateID(TemplateId);
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(exe);
//                                            continue;

//                                        }

//                                    }
//                                    else if (ds3.Tables.Count == 0)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            DeleteTemplateID(TemplateId);
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }
//                                    }
//                                    else if (ds3.Tables[0].Rows.Count == 0)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            //   DeleteTemplateID(TemplateId);
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }
//                                    }
//                                }
//                            }
//                            else
//                            {

//                                FormedQuery3 = "SP_GETVIEWDATA_STEENA";
//                                using (GetDataSet objDataSetValue = new GetDataSet())
//                                {

//                                    ds3 = objDataSetValue.GetDataSetValueForEditBatchjob(FormedQuery3, configId, documentid, Convert.ToDateTime(PeriodTo).ToString("dd-MMM-yyyy"));

//                                    if (ds3 == null)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            //DeleteTemplateID(TemplateId);
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }

//                                    }
//                                    else if (ds3.Tables.Count == 0)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            DeleteTemplateID(TemplateId);
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }
//                                    }
//                                    else if (ds3.Tables[0].Rows.Count == 0)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            //DeleteTemplateID(TemplateId);
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }
//                                    }
//                                }
//                                //isViewData = true;

//                            }



//                        }

//                        if (DataCollection == "Online")
//                        {
                            
//                            string OntimeCode = ForTime;
//                            string ForTimeCode = OnTime;
//                            string Period = PeriodTo;
//                            string onlineTemplateURL = ConfigurationManager.AppSettings["HostName"] + "/" + ConfigurationManager.AppSettings["ApplicationName"] + "/OnlineFormView/ViewOnlineForm?Strid=" + configId.ToString() + "&StrsubjectAreaId=" + ot_details.SUBJECTAREAID.ToString() + "&UserID=" + ot_details.UserId + "&ForTimeCode=" + OntimeCode + "&OnTimeCode=" + ForTimeCode + "&PeriodTo=" + PeriodTo;
//                            NameValueCollection mailBodyplaceHolders = new NameValueCollection();
//                            string ChangedSubjectArea = ot_details.SubjectArea.Replace("_", " ");
//                            ChangedSubjectArea = new CultureInfo("en-US").TextInfo.ToTitleCase(ChangedSubjectArea.ToLower());

//                            mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
//                            mailBodyplaceHolders.Add("<SubjectArea>", ChangedSubjectArea);
//                            mailBodyplaceHolders.Add("<ForTimeCode>", ForTime);
//                            mailBodyplaceHolders.Add("<OnTimeCode>", OnTime);
//                            mailBodyplaceHolders.Add("<LockDate>", dtlockdate.ToString());
//                            mailBodyplaceHolders.Add("<Custom>", customonline.ToString());
//                            mailBodyplaceHolders.Add("<OnlineTemplateURL>", onlineTemplateURL);
//                            //Format the header    

//                            string DataCollectionSubject = "[iQlik Portal] - Data collection template link for  [" + ot_details.SubjectArea + "] -[" + ot_details.ReportingPeriod + "]";
//                            string mailbody = "";
//                            string messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["OnlineTemplateDataUpload"].ToString());

//                            mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
//                            try
//                            {
//                                SendMailIsHtml("", DataCollectionSubject, mailbody, ot_details.UserEmail);
//                            }
//                            catch (Exception ex)
//                            {
//                                EP_SENDORRESENDTASK ESP = m.EP_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
//                                ESP.ATTEMPTS++;
//                                m.SaveChanges();
//                            }
//                        }
//                        else
//                        {
//                            int MasterColumn = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Master").Select(s => new { CONFIG_ID = s.CONFIG_ID }).Count();
//                            int MeasureColmnsCount = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Measure").Select(s => new { CONFIG_ID = s.CONFIG_ID }).Count();
//                            var measurecolumn = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Measure").Select(s => new { Text = s.DATA_TYPE, Value = s.DISPLAY_TYPE, ConfigUserId = s.CONFIGUSER_ID }).OrderBy(s => s.ConfigUserId);
//                            var MasterShowColumn = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Master").Select(s => new { DATA_TYPE = s.DATA_TYPE, IS_SHOW = s.IS_SHOW });

//                            FileName = ot_details.SubjectArea + "_[T" + TemplateId + "]_[" + ForTime + "]_[" + OnTime + "]_[" + DateTime.Now.ToString("dd-MM-yyyy") + "-" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + " " + DateTime.Now.ToString("tt ") + "]" + ".xlsx";
//                            string FormedQuery4 = "SP_GETDROPDOWNDATA";
//                            DataSet ds4 = new DataSet();
//                            using (GetDataSet objDataSetValue = new GetDataSet())
//                            {
//                                ds4 = objDataSetValue.GetDataSetValue(FormedQuery4, configId);
//                            }
//                            //ProcessDataSetHeader(ref ds3, ref ds33, configId);
//                            using (ExcelPackage objExcelPackage = new ExcelPackage())
//                            {
//                                DataTable dsExcel = new DataTable();
//                                // var SecondList = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.FIELD_NAME.Contains("_CODE") || s.FIELD_NAME.Contains("_CD"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//                                var SecondList = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.IS_SHOW == "N")).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID, CollectionType = s.COLLECTION_TYPE, DisplayType = s.DISPLAY_TYPE }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//                                // var FirstList = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && !(s.FIELD_NAME.Contains("_CD")) && (!s.FIELD_NAME.Contains("_CODE"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//                                var FirstList = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.IS_SHOW == "Y" || s.IS_SHOW == null)).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID, CollectionType = s.COLLECTION_TYPE, DisplayType = s.DISPLAY_TYPE }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//                                var totalList = FirstList.OrderBy(o => o.CollectionType).Concat(SecondList).ToList();
//                                //var J = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && !(s.FIELD_NAME.Contains("_CD")) && (!s.FIELD_NAME.Contains("_CODE"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//                                //var M = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.FIELD_NAME.Contains("_CODE") || s.FIELD_NAME.Contains("_CD"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//                                //var N = J.Concat(M).ToList();
//                                // var DisplayName = (from T in MasterShowColumn1 join R in N on T.DATA_TYPE equals R.DATA_TYPE select new { DISPLAY_NAME = R.DISPLAY_NAME, DATA_TYPE =R.DATA_TYPE }).ToList();
//                                if (ds3.Tables.Count > 0)
//                                {
//                                    DataTable table = ds3.Tables[0];

//                                    foreach (var column in totalList)
//                                    {
//                                        if (IsGroupColumnNameExist == true)
//                                        {
//                                            var DisplayName = m.GET_TIMECODE.Where(w => w.CONFIG_ID == configId && w.DATA_TYPE == column.DATA_TYPE && w.TEMPLATE_ID == TemplateId).Select(s => new { DisplayName = s.DISPLAY_NAME, DataType = s.DATA_TYPE }).FirstOrDefault();
//                                            if (DisplayName != null)
//                                                table.Columns[column.DATA_TYPE].ColumnName = DisplayName.DisplayName.Trim();
//                                            else
//                                                table.Columns[column.DATA_TYPE].ColumnName = column.DISPLAY_NAME;
//                                        }
//                                        else
//                                        {
//                                            table.Columns[column.DATA_TYPE].ColumnName = column.DISPLAY_NAME;
//                                        }
//                                    }
//                                    table.AcceptChanges();
//                                    dsExcel = table;
//                                }
//                                //foreach (DataTable dtSrc in dsExcel)
//                                if (dsExcel.Rows.Count > 0)
//                                {                                   

//                                    ExcelWorksheet hiddenSheet = objExcelPackage.Workbook.Worksheets.Add(ConfigurationManager.AppSettings["HiddenSheetName"]);
//                                    hiddenSheet.Cells[1000000, 500].Value = FileCode;// Need to pass File Code 
//                                    hiddenSheet.Hidden = eWorkSheetHidden.VeryHidden;
//                                    //Create the worksheet    
//                                    dsExcel.TableName = "DataCollectionSheet";
//                                    ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add(dsExcel.TableName);
//                                    //ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Data Collection Sheet");

//                                    objWorksheet.Cells["A1"].Value = ot_details.SubjectArea + "- Data Collection Sheet";
//                                    objWorksheet.Cells["A2"].Value = "Reporting Start Date-" + Convert.ToDateTime(PeriodFrom).ToString("dd/MMM/yyyy");
//                                    objWorksheet.Cells["B2"].Value = "Reporting End Date-" + Convert.ToDateTime(PeriodTo).ToString("dd/MMM/yyyy");
//                                    objWorksheet.Cells["A3"].Value = "Assigned User-" + ot_details.UserName;
//                                    objWorksheet.Cells["B3"].Value = "Lock Date-" + DateTime.Now.Date.ToString("dd/MMM/yyyy");
//                                    objWorksheet.Cells["C3"].Value = "Generation Date-" + DateTime.Now.Date.ToString("dd/MMM/yyyy");
//                                    objWorksheet.Cells["A4"].Value = "";// customexcel;
//                                    objWorksheet.Row(4).Hidden = true;//Hide 4th row
//                                    objWorksheet.DefaultColWidth = 30;
//                                    //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1 
//                                    try
//                                    {
//                                        objWorksheet.Cells["A5"].LoadFromDataTable(dsExcel, true);
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        string ErrorDetails = "Error Details:Total Rows " + dsExcel.Rows.Count + " and ConfigId :" + configId + " and Subject Area Name:" + ot_details.SubjectArea + "  Assigned User: " + ot_details.UserName;
//                                        ErrorLog srsEx = new ErrorLog();
//                                        srsEx.LogErrorInTextFile(ex, ErrorDetails);
//                                        continue;

//                                    }
//                                    objWorksheet.Cells.Style.Font.SetFromFont(new Font("Calibri", 10));
//                                    // char next='A';
//                                    char next = incrementCharacter('A', MasterColumn);
//                                    int xx = 0;
//                                    List<string> LstMeasureColumns = new List<string>();
//                                    List<string> LstGrandTotalColumns = new List<string>();
//                                    foreach (var T in totalList)
//                                    {


//                                        foreach (var s in measurecolumn)
//                                        {

//                                            if (T.DATA_TYPE == s.Text && s.Value == "DROPDOWN" && T.CollectionType == "Measure")
//                                            {
//                                                next = incrementCharacter('A', xx);
//                                                string resultchar = (next.ToString() + ':' + next.ToString()).ToString();
//                                                IExcelDataValidation dataValidation;

//                                                var lookuptype = objWorksheet.DataValidations.AddListValidation(resultchar.ToString());
//                                                foreach (DataRow row in ds4.Tables[0].Rows) // Loop over the rows.
//                                                {
//                                                    if (row.ItemArray[0].ToString() == s.Text)
//                                                    {
//                                                        lookuptype.Formula.Values.Add(row.ItemArray[2].ToString());
//                                                    }
//                                                    dataValidation = lookuptype;
//                                                    dataValidation.ShowErrorMessage = true;
//                                                    dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
//                                                    dataValidation.ErrorTitle = "An invalid value was entered";
//                                                    dataValidation.Error = "Select a value from the from  list";
//                                                }
//                                                LstMeasureColumns.Add(next.ToString());
//                                                break;

//                                            }
//                                            //if (s.Value == "TEXTBOX" && T.DATA_TYPE.Contains("VC") && T.CollectionType == "Measure")
//                                            if (s.Value == T.DisplayType && s.Value != "DROPDOWN" && T.DATA_TYPE.Contains("VC") && T.CollectionType == "Measure")
//                                            {
//                                                next = incrementCharacter('A', xx);
//                                                //CellsNumeric(objWorksheet, next, 1);

//                                                LstMeasureColumns.Add(next.ToString());
//                                                LstGrandTotalColumns.Add(next.ToString());
//                                                break;

//                                            }
//                                            //if (s.Value == "TEXTBOX" && T.DATA_TYPE.Contains("N") && T.CollectionType == "Measure")
//                                            if (s.Value == T.DisplayType && T.DATA_TYPE.Contains("N") && T.CollectionType == "Measure")
//                                            {
//                                                next = incrementCharacter('A', xx);
//                                                CellsNumeric(objWorksheet, next, 1);

//                                                LstMeasureColumns.Add(next.ToString());
//                                                LstGrandTotalColumns.Add(next.ToString());
//                                                break;

//                                            }
//                                            // if (s.Value == "DATE" && T.DATA_TYPE.Contains("DT") && T.CollectionType == "Measure")
//                                            if (s.Value == T.DisplayType && T.DATA_TYPE.Contains("DT") && T.CollectionType == "Measure")
//                                            {
//                                                next = incrementCharacter('A', xx);
//                                                DateNumeric(objWorksheet, next, 1);
//                                                LstMeasureColumns.Add(next.ToString());
//                                                break;
//                                            }
//                                            // if (s.Value == "TEXTBOX" && T.DATA_TYPE.Contains("P") && T.CollectionType == "Measure")
//                                            if (s.Value == T.DisplayType && T.DATA_TYPE.Contains("P") && T.CollectionType == "Measure")
//                                            {
//                                                next = incrementCharacter('A', xx);
//                                                CellsNumericPercentage(objWorksheet, next, 1);
//                                                LstMeasureColumns.Add(next.ToString());
//                                                LstGrandTotalColumns.Add(next.ToString());
//                                                break;
//                                            }


//                                        }
//                                        xx++;
//                                    }
//                                    // char next = incrementCharacter('A', MasterColumn);
//                                    char nextmeasure = incrementCharacter(next, MeasureColmnsCount);
//                                    //string nextstring = (next.ToString() + ':' + nextmeasure.ToString()).ToString();
//                                    //int countindex = 1;
//                                    objWorksheet.Cells["A4:C4"].Merge = true;
//                                    objWorksheet.Protection.IsProtected = true;
//                                    if (LstMeasureColumns.Count > 0)
//                                    {
//                                        //Unlock Measure Columns
//                                        string S = LstMeasureColumns[0];
//                                        string L = LstMeasureColumns[LstMeasureColumns.Count - 1];
//                                        int LastRownumber = dsExcel.Rows.Count + 5;
                                        
//                                        using (ExcelRange objRange = objWorksheet.Cells[S + "6" + ":" + L + LastRownumber])
//                                        {
//                                            objRange.Style.Locked = false;
//                                        }
//                                    }
//                                    //dsExcel.Rows

//                                    objWorksheet.Protection.IsProtected = true;
//                                    if (!string.IsNullOrEmpty(FileCode))
//                                        objWorksheet.Protection.SetPassword(FileCode);
//                                    objWorksheet.Cells.AutoFitColumns();

//                                    hiddenSheet.Cells.AutoFitColumns();

//                                    objExcelPackage.Workbook.Protection.LockStructure = true;
//                                    char total = incrementCharacter('A', (FirstList.Count - 1));//Remove unwanted column in excel 
//                                    /*Freeze Panes*/
//                                    objWorksheet.View.FreezePanes(6, 1);

//                                    using (ExcelRange objRange = objWorksheet.Cells["A5:" + total.ToString() + "5"])
//                                    {

//                                        objRange.Style.Font.Bold = true;
//                                        objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                        objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Justify;
//                                        objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                        objRange.Style.Font.Color.SetColor(Color.Black);
//                                        objRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

//                                        /*Border lines should be added*/
//                                        objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                        
//                                        objRange.AutoFilter = true;
//                                        objWorksheet.Protection.AllowAutoFilter = true;

//                                        //•	Hide isShow(which is true) Columns  from Excel.
//                                        if (SecondList.Count > 0)
//                                        {

//                                            int Firstlistcloulmncount = FirstList.Count;
//                                            for (int c = 1; c <= SecondList.Count; c++)
//                                            {

//                                                objWorksheet.Column(Firstlistcloulmncount + c).Hidden = true;
//                                            }

//                                        }
//                                        //Border lines should be added
//                                        int columnno = 6;
//                                        int rows = dsExcel.Rows.Count;
//                                        for (int c = 1; c <= rows; c++)
//                                        {
//                                            int rr = columnno;

//                                            //objWorksheet.Cells["A6:" + next + "6"].Style.Locked = true;
//                                            objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                            objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                            objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                            objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                            columnno++;
//                                        }
//                                        //SubTotal
//                                        //objWorksheet.Cells["A" + (rows + 6).ToString() + ""]
//                                        //Grand Total
//                                        objWorksheet.Cells["A" + (rows + 6).ToString() + ""].Value = "Grand Total";
                                        
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);
//                                        /*Border lines should be added*/
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Font.Bold = true;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.VerticalAlignment = ExcelVerticalAlignment.Justify;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                        objWorksheet.Cells["A" + (dsExcel.Rows.Count + 6) + ":" + total.ToString() + "" + (dsExcel.Rows.Count + 6) + ""].Style.Font.Color.SetColor(Color.White);


//                                        if (LstGrandTotalColumns.Count > 0)
//                                        {
//                                            for (int g = 0; g < LstGrandTotalColumns.Count; g++)
//                                            {
//                                                objWorksheet.Cells[LstGrandTotalColumns[g] + "" + (dsExcel.Rows.Count + 6) + ""].Formula = "=SUM(" + LstGrandTotalColumns[g] + "6:" + LstGrandTotalColumns[g] + "" + (dsExcel.Rows.Count + 5) + ")";
//                                            }
//                                        }
//                                        //objWorksheet.Cells["F" + (dsExcel.Rows.Count + 6) + ""].Formula = "=SUM(E6:E" + (dsExcel.Rows.Count + 5) + ")";
//                                    }

//                                    using (ExcelRange objRange = objWorksheet.Cells["A1:" + total.ToString() + "1"])
//                                    {
//                                        objRange.Style.Font.Bold = true;
//                                        objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                        objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//                                        objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                        objRange.Style.Font.Color.SetColor(Color.White);

//                                        objRange.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
//                                        /*Border lines should be added*/
//                                        objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

//                                    }
//                                    using (ExcelRange objRange = objWorksheet.Cells["A2:" + total.ToString() + "2"])
//                                    {
//                                        objRange.Style.Font.Bold = true;
//                                        objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                        objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//                                        objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                        objRange.Style.Font.Color.SetColor(Color.White);
//                                        objRange.Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);
//                                        /*Border lines should be added*/
//                                        objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                    }
//                                    using (ExcelRange objRange = objWorksheet.Cells["A3:" + total.ToString() + "3"])
//                                    {
//                                        objRange.Style.Font.Bold = true;
//                                        objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                        objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//                                        objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                        objRange.Style.Font.Color.SetColor(Color.White);
//                                        objRange.Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);
//                                        /*Border lines should be added*/
//                                        objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                    }
//                                    using (ExcelRange objRange = objWorksheet.Cells["A4:" + total.ToString() + "4"])
//                                    {
//                                        objRange.Style.Font.Bold = true;
//                                        objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                        objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//                                        objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                        objRange.Style.Font.Color.SetColor(Color.White);
//                                        objRange.Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);
//                                        /*Border lines should be added*/
//                                        objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                        objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                    }
                                    

//                                    string filepath = System.Configuration.ConfigurationManager.AppSettings["BatchJobErrorlocation"];
//                                    string path = Path.Combine(filepath, FileName);
//                                    //string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Excel"), FileName);
//                                    //Create excel file on physical disk    
//                                    FileStream objFileStrm = System.IO.File.Create(path);
//                                    objFileStrm.Close();
//                                    //Write content to excel file    
//                                    System.IO.File.WriteAllBytes(path, objExcelPackage.GetAsByteArray());
//                                    string ChangedSubjectArea = ot_details.SubjectArea.Replace("_", " ");
//                                    ChangedSubjectArea = new CultureInfo("en-US").TextInfo.ToTitleCase(ChangedSubjectArea.ToLower());

//                                    NameValueCollection mailBodyplaceHolders = new NameValueCollection();
//                                    mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
//                                    mailBodyplaceHolders.Add("<SubjectArea>", ChangedSubjectArea);
//                                    mailBodyplaceHolders.Add("<ForTimeCode>", ForTime);
//                                    mailBodyplaceHolders.Add("<OnTimeCode>", OnTime);
//                                    mailBodyplaceHolders.Add("<LockDate>", dtlockdate.ToString());
//                                    mailBodyplaceHolders.Add("<Custom>", custom.ToString());
//                                    //Format the header    
//                                    string DataCollectionSubject = "[iQlik Portal] - Data collection template for  [" + ot_details.SubjectArea + "] -[" + ot_details.ReportingPeriod + "]";

//                                    string mailbody = "";
//                                    string messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["TemplateGenerationEmail"].ToString());

//                                    mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);

//                                    try
//                                    {

//                                        SendMail(path, DataCollectionSubject, mailbody, ot_details.UserEmail);
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        EP_SENDORRESENDTASK ESP = m.EP_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
//                                        ESP.ATTEMPTS++;
//                                        m.SaveChanges();
//                                    }
//                                }
//                            }

//                        }
//                        EP_TEMPLATE EPM = m.EP_TEMPLATE.Where(x => x.TEMPLATE_ID == TemplateId).FirstOrDefault();
//                        if (FileName != "")
//                        {
//                            EPM.FILE_NAME = FileName;
//                        }
//                        EPM.UPDATEDBY = UserId;
//                        EPM.UPDATEDON = DateTime.Now;
//                        //Write File Code  or not 


//                        if (documentid == "" || string.IsNullOrEmpty(documentid))
//                        {

//                            if (!string.IsNullOrEmpty(FileCode))
//                            {
//                                EPM.FILE_CODE = FileCode;
//                            }

//                        }
//                        else if (!string.IsNullOrEmpty(documentid) && DataCollection.ToUpper() == "OFFLINE")
//                        {
//                            EPM.FILE_CODE = FileCode;
//                        }

//                        m.SaveChanges();
//                        EP_SENDORRESENDTASK esppp = m.EP_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
//                        esppp.SENT = 1;
//                        esppp.ATTEMPTS++;
//                        esppp.ID = SendResendId;
//                        m.SaveChanges();
//                    }

//                    catch (Exception ex)
//                    {
//                        string ErrorDetails = "Error Details: Config Id :" + configId + " Subject Area Id" + SubjectAreaId + " and Assigned Userid : " + UserId;
//                        ErrorLog srsEx = new ErrorLog();
//                        srsEx.LogErrorInTextFile(ex, ErrorDetails);
//                        continue;
//                    }
//                }//Main For loop Close
//                SendUploadMail();
//                SendEscalationMail();

//            }
//            catch (Exception ex)
//            {
//                string ErrorDetails = "Error Details: No data return from SP: SP_GETCONFIG_BATCH for IsPopulated 'Y' and  Function Name:GetConfigForExcelGeneration() ";
//                ErrorLog srsEx = new ErrorLog();
//                srsEx.LogErrorInTextFile(ex, ErrorDetails);
//                //throw ex;

//            }

//            return ds;

//        }
//        public string GetExcelFilePathForIspopulatedYes(decimal? TemplateId)
//        {
//            string FileNamepath = string.Empty;
//            try
//            {
//                SPONGE_Context m = new SPONGE_Context();
//                //var objFile = m.EP_DOCUMENT.Where(w => w.TEMPLATEID == TemplateId).Select(s => new { ID = s.ID,FileName =s.FILE_NAME }).FirstOrDefault();
//                var objFile = (from D in m.EP_DOCUMENT
//                               join T in m.EP_TEMPLATE on D.TEMPLATEID equals T.TEMPLATE_ID
//                               where D.TEMPLATEID == TemplateId
//                               select new { ID = D.ID, FileName = T.FILE_NAME }).FirstOrDefault();
//                // var query = objEntities.Employee.Join(objEntities.Department, r => r.EmpId, p => p.EmpId, (r, p) => new { r.FirstName, r.LastName, p.DepartmentName });
//                string filepath = System.Configuration.ConfigurationManager.AppSettings["SenExcelFileLocationIfIspopulatedYes"];
//                if (objFile != null)
//                {
//                    if (!File.Exists(filepath + "\\" + objFile.FileName))
//                    {
//                        File.Copy(filepath + "\\" + objFile.ID + ".xlsx", filepath + "\\" + objFile.FileName);
//                        FileNamepath = Path.Combine(filepath, filepath + "\\" + objFile.FileName);
//                    }
//                    else
//                    {
//                        FileNamepath = Path.Combine(filepath, filepath + "\\" + objFile.FileName);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {

//            }
//            return FileNamepath;
//        }
//        public DataSet GetPrepopulatedConfigForExcelGeneration(decimal configid, string p_IS_PREPOPULATE)
//        {
//            DataSet ds = new DataSet();
//            CommonUtility objutility = new CommonUtility();
//            ErrorLog lg = new ErrorLog();
//            bool IfDataExistForIsPopulatedYes = false;
//            try
//            {
//                SPONGE_Context m = new SPONGE_Context();
//                List<POC_CONFIG_STRUCTURE> lstConfigEntity = new List<POC_CONFIG_STRUCTURE>();
//                GetDataSet gd = new GetDataSet();
//                ds = gd.GetBatchJobDataSetValue("SP_GETCONFIG_BATCH", Convert.ToInt32(configid), p_IS_PREPOPULATE);

//                foreach (DataRow dr in ds.Tables[0].Rows)
//                {
//                    try
//                    {
//                        bool IsGroupColumnNameExist = false;
//                        int configId = Convert.ToInt32(dr["CONFIG_ID"]);
//                        decimal SubjectAreaId = Convert.ToDecimal(dr["SUBJECTAREA_ID"]);
//                        string UserId = Convert.ToString(dr["Created_By"]);
//                        DateTime dtlockdate = dr["LOCKDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["LOCKDATE"]);
//                        DateTime dtesc = dr["ESCALATIONDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["ESCALATIONDATE"]);
//                        DateTime dtrem = dr["UPLOADREMINDERDATE"] == DBNull.Value ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dr["UPLOADREMINDERDATE"]);

//                        decimal SendResendId = 0;
//                        ForTimeOnTimeCode fp = new ForTimeOnTimeCode();

//                        string TimeLevel = Convert.ToString(dr["TIME_LEVEL"]);
//                        string frequency = Convert.ToString(dr["FREQUENCY"]);
//                        string ReportingPeriod = Convert.ToString(dr["REPORTING_PERIOD"]);
//                        string Granulaity = Convert.ToString(dr["ONTIMELEVEL"]);
//                        string Version = Convert.ToString(dr["VERSION"]);
//                        string IsPopulated = Convert.ToString(dr["IS_POPULATED"]);
//                        string DataCollection = Convert.ToString(dr["DATA_COLLECTION"]);
//                        string PeriodFrom;
//                        string PeriodTo;
//                        string ForTime, OnTime;
//                        int currentyear = fp.GetForTimeOnTimeVersionN(ReportingPeriod, frequency, Granulaity, Version, out ForTime, out OnTime, out PeriodFrom, out PeriodTo);
//                        if (Version == "Y")
//                        {
//                            int nextyear = fp.GetForTimeOnTimeVersion(ReportingPeriod, frequency, Granulaity, Version, ref ForTime, out OnTime, ref PeriodFrom, ref PeriodTo); ;
//                        }

//                        bool IsFlagContinue = false;
//                        string GetTodayDate = Convert.ToDateTime(DateTime.Now.Date).ToString("dd-MMM-yyyy");
                        
//                        var CheckScheduleTime = m.EP_SENDORRESENDTASK.Where(w => w.CONFIG_ID == configId && (w.FORTIMECODE == ForTime) && w.ONTIMECODE == OnTime && w.SENT == 1 && w.PERIOD_FROM == null).ToList();
//                        foreach (var item in CheckScheduleTime)
//                        {
//                            if (Convert.ToDateTime(item.CREATEDDATE).ToString("dd-MMM-yyyy") == GetTodayDate && Convert.ToString(dr["MANUALFLAG"]) == "AUTO")
//                            {
//                                IsFlagContinue = true;
//                                break;
//                            }
//                        }
//                        if (IsFlagContinue == true)
//                            continue;

//                        if (Convert.ToString(dr["MANUALFLAG"]) == "AUTO")
//                        {
//                            EP_SENDORRESENDTASK esp = new EP_SENDORRESENDTASK();
//                            esp.CONFIG_ID = configId;
//                            esp.CREATEDBY = UserId;
//                            esp.CREATEDDATE = DateTime.Now;
//                            esp.LOCKDATE = dtlockdate;
//                            esp.ESCALATIONDATE = dtesc;
//                            esp.UPLOADREMINDERDATE = dtrem;
//                            esp.IS_AUTO_MANUAL = "AUTO";
//                            esp.FORTIMECODE = ForTime;
//                            esp.ONTIMECODE = OnTime;
//                            m.EP_SENDORRESENDTASK.Add(esp);
//                            m.SaveChanges();
//                            SendResendId = esp.ID;

//                        }
//                        else
//                        {
//                            ForTime = Convert.ToString(dr["FORTIMECODE"]);
//                            OnTime = Convert.ToString(dr["ONTIMECODE"]);
//                            SendResendId = Convert.ToInt32(dr["ID"]);
//                            PeriodFrom = Convert.ToString(dr["PERIOD_FROM"]);
//                            PeriodTo = Convert.ToString(dr["PERIOD_TO"]);
                            
//                        }

//                        var ot_details = (from ep in m.POC_CONFIG_STRUCTURE
//                                          join pd in m.EP_SUBJECTAREA on ep.SUBJECTAREA_ID equals pd.SUBJECTAREA_ID
//                                          join pc in m.POC_CONFIGURATION on ep.CONFIG_ID equals pc.CONFIG_ID
//                                          join pf in m.EP_SUBFUNCTION on pd.SUBFUNCTION_ID equals pf.SUBFUNCTION_ID
//                                          join us in m.EP_USERS on ep.USER_ID equals us.ID
//                                          where ep.CONFIG_ID == configId
//                                          select new { SUBJECTAREAID = pd.SUBJECTAREA_ID, COLLECTION_TYPE = pc.DATA_COLLECTION, SubjectArea = pd.SUBJECTAREA_NAME, ReportingPeriod = pd.REPORTING_PERIOD, Function = pf.FUNCTION_NAME, SubFunction = pf.SUBFUNCTION_NAME, UserId = us.ID, UserName = us.NAME, UserEmail = us.EMAILID }).FirstOrDefault();

//                        //Custom
//                        StringBuilder custom = new StringBuilder();
//                        StringBuilder customexcel = new StringBuilder();
//                        StringBuilder customonline = new StringBuilder();

//                        DataSet custom_ds = new DataSet();
//                        custom_ds = gd.GetDataSetValue("SP_GETMASTEREMAIL", configId);
//                        for (int v = 0; v < custom_ds.Tables[0].Rows.Count; v++)
//                        {
//                            custom.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
//                            custom.Append("\r\n");
//                            customexcel.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
//                            customexcel.Append(",");
//                            customonline.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
//                            customonline.Append("<br/>");
//                        }
//                        string FileName = "";
//                        String FileCode = "";
//                        string FormedQuery3 = "";
//                        DataSet ds3 = new DataSet();
//                        decimal TemplateId = 0;
//                        GetDataSet gds = new GetDataSet();
                      
//                        string documentidtest = "";
//                        string documentid = "";
//                        int TemplateChkId = 0;
//                        documentidtest = gds.GetDataSetValueCheck("SP_CHECKTEMPLATE", configId, ForTime, OnTime, out TemplateChkId);

//                        documentid = documentid = m.EP_DOCUMENT.Where(w => w.TEMPLATEID == TemplateChkId && w.APPROVALSTATUSID != 4).OrderByDescending(d => d.UPLOADDATE).Select(s => s.ID).FirstOrDefault();
                        
//                        if (TemplateChkId == 0)
//                        {
//                            IfDataExistForIsPopulatedYes = false;
//                            EP_TEMPLATE EPTemplate = new EP_TEMPLATE();
//                            EPTemplate.CONFIG_ID = configId;
//                            EPTemplate.CREATEDBY = UserId;
//                            EPTemplate.CREATEDON = DateTime.Now;
//                            EPTemplate.ESCALATION_DATE = dtesc;
//                            EPTemplate.UPLOAD_REMINDER_DATE = dtrem;
//                            EPTemplate.LOCK_DATE = dtlockdate;
//                            EPTemplate.ONTIMECODE = OnTime;
//                            EPTemplate.FORTIMECODE = ForTime;
//                            EPTemplate.PERIOD_FROM = Convert.ToDateTime(DateTime.Parse(PeriodFrom.Trim()).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
//                            EPTemplate.PERIOD_TO = Convert.ToDateTime(DateTime.Parse(PeriodTo.Trim()).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
//                            //  EPTemplate.FILE_NAME = "";
//                            m.EP_TEMPLATE.Add(EPTemplate);
//                            m.SaveChanges();
//                            TemplateId = EPTemplate.TEMPLATE_ID;
//                            FileCode = Guid.NewGuid().ToString();
//                            objutility.GenerateDynamicColumnNames(configId, Convert.ToDateTime(PeriodFrom), Convert.ToDateTime(PeriodTo), TimeLevel, frequency, TemplateId, out IsGroupColumnNameExist, null);
                            


//                            FormedQuery3 = "EXCELPROCEDURE1_STEENA_TEST";// "Templatedata_dropdown";// "TEST_EXCELPROCEDURE1_STEENA";//"SP_GETQUERYEXCELTEMPLATE";//
//                            using (GetDataSet objDataSetValue = new GetDataSet())
//                            {
//                                ds3 = objDataSetValue.GetDataSetValueForBatchJob(FormedQuery3, configId, documentid, Convert.ToDateTime(PeriodTo).ToString("dd-MMM-yyyy"));

//                                if (ds3 == null)
//                                {
//                                    try
//                                    {
//                                        UpdateSendResendTasktable(SendResendId, m);
//                                        DeleteTemplateID(TemplateId);
//                                        SendEmailForERROR(SubjectAreaId, configId);
//                                        continue;
//                                    }
//                                    catch (Exception ex)
//                                    {

//                                        ErrorLog srsEx = new ErrorLog();
//                                        srsEx.LogErrorInTextFile(ex);
//                                        continue;

//                                    }

//                                }
//                                else if (ds3.Tables.Count == 0)
//                                {
//                                    try
//                                    {
//                                        UpdateSendResendTasktable(SendResendId, m);
//                                        DeleteTemplateID(TemplateId);
//                                        SendEmailForERROR(SubjectAreaId, configId);
//                                        continue;
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        DeleteTemplateID(TemplateId);
//                                        ErrorLog srsEx = new ErrorLog();
//                                        srsEx.LogErrorInTextFile(ex);
//                                        continue;

//                                    }
//                                }
//                                else if (ds3.Tables[0].Rows.Count == 0)
//                                {
//                                    try
//                                    {
//                                        UpdateSendResendTasktable(SendResendId, m);
//                                        DeleteTemplateID(TemplateId);
//                                        SendEmailForERROR(SubjectAreaId, configId);
//                                        continue;
//                                    }
//                                    catch (Exception ex)
//                                    {

//                                        ErrorLog srsEx = new ErrorLog();
//                                        srsEx.LogErrorInTextFile(ex);
//                                        continue;

//                                    }
//                                }

//                            }
//                        }
//                        else
//                        {

//                            var Templatedetails = (from ep in m.EP_TEMPLATE
//                                                   where ep.TEMPLATE_ID == TemplateChkId
//                                                   select new { TemplateId = ep.TEMPLATE_ID, PeriodFrom = ep.PERIOD_FROM, PeriodTo = ep.PERIOD_TO, FileCode = ep.FILE_CODE }).FirstOrDefault();
//                            TemplateId = Templatedetails.TemplateId;
//                            PeriodFrom = Templatedetails.PeriodFrom.ToString();
//                            PeriodTo = Templatedetails.PeriodTo.ToString();

//                            if (DataCollection.ToUpper() == "OFFLINE" && string.IsNullOrEmpty(Templatedetails.FileCode))
//                            {
//                                FileCode = Guid.NewGuid().ToString();
//                            }
//                            else
//                            {
//                                FileCode = Templatedetails.FileCode;
//                            }
//                            if (string.IsNullOrEmpty(documentid) || documentid == "null")
//                            {
//                                IfDataExistForIsPopulatedYes = false;
//                                FormedQuery3 = "EXCELPROCEDURE1_STEENA_TEST";// "Templatedata_dropdown";// "Test_dropdown";// "TEST_EXCELPROCEDURE1_STEENA";// "SP_GETQUERYEXCELTEMPLATE";
//                                using (GetDataSet objDataSetValue = new GetDataSet())
//                                {

//                                    ds3 = objDataSetValue.GetDataSetValueForBatchJob(FormedQuery3, configId, documentid, Convert.ToDateTime(PeriodTo).ToString("dd-MMM-yyyy"));

//                                    if (ds3 == null)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {

//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }

//                                    }
//                                    else if (ds3.Tables.Count == 0)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            DeleteTemplateID(TemplateId);
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }
//                                    }
//                                    else if (ds3.Tables[0].Rows.Count == 0)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {

//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }
//                                    }
//                                }
//                            }
//                            else
//                            {


//                                FormedQuery3 = "EXCELPROCEDURE1_STEENA_TEST";// "Templatedata_dropdown";// "Test_dropdown";//"SP_GETVIEWDATA";
//                                using (GetDataSet objDataSetValue = new GetDataSet())
//                                {
//                                    IfDataExistForIsPopulatedYes = true;
//                                    ds3 = objDataSetValue.GetDataSetValueForBatchJob(FormedQuery3, configId, documentid, Convert.ToDateTime(PeriodTo).ToString("dd-MMM-yyyy"));

//                                    if (ds3 == null)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {

//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }

//                                    }
//                                    else if (ds3.Tables.Count == 0)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            DeleteTemplateID(TemplateId);
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }
//                                    }
//                                    else if (ds3.Tables[0].Rows.Count == 0)
//                                    {
//                                        try
//                                        {
//                                            UpdateSendResendTasktable(SendResendId, m);
//                                            SendEmailForERROR(SubjectAreaId, configId);
//                                            continue;
//                                        }
//                                        catch (Exception ex)
//                                        {

//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex);
//                                            continue;

//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        if (DataCollection == "Online")
//                        {

//                            string OntimeCode = ForTime;
//                            string ForTimeCode = OnTime;
//                            string Period = PeriodTo;
//                            string onlineTemplateURL = ConfigurationManager.AppSettings["HostName"] + "/" + ConfigurationManager.AppSettings["ApplicationName"] + "/OnlineFormView/ViewOnlineForm?Strid=" + configId.ToString() + "&StrsubjectAreaId=" + ot_details.SUBJECTAREAID.ToString() + "&UserID=" + ot_details.UserId + "&ForTimeCode=" + OntimeCode + "&OnTimeCode=" + ForTimeCode + "&PeriodTo=" + PeriodTo;                           
//                            NameValueCollection mailBodyplaceHolders = new NameValueCollection();
//                            mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
//                            mailBodyplaceHolders.Add("<SubjectArea>", ot_details.SubjectArea);
//                            mailBodyplaceHolders.Add("<ForTimeCode>", ForTime);
//                            mailBodyplaceHolders.Add("<OnTimeCode>", OnTime);
//                            mailBodyplaceHolders.Add("<LockDate>", dtlockdate.ToString());
//                            mailBodyplaceHolders.Add("<Custom>", customonline.ToString());
//                            mailBodyplaceHolders.Add("<OnlineTemplateURL>", onlineTemplateURL);
//                            //Format the header    

//                            string DataCollectionSubject = "[iQlik Portal] - Data collection template link for  [" + ot_details.SubjectArea + "] -[" + ot_details.ReportingPeriod + "]";
//                            string mailbody = "";
//                            string messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["OnlineTemplateDataUpload"].ToString());

//                            mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
//                            try
//                            {
//                                SendMailIsHtml("", DataCollectionSubject, mailbody, ot_details.UserEmail);
//                            }
//                            catch (Exception ex)
//                            {
//                                EP_SENDORRESENDTASK ESP = m.EP_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
//                                ESP.ATTEMPTS++;
//                                m.SaveChanges();
//                            }
//                        }
//                        else
//                        {

//                            if (!string.IsNullOrEmpty(IsPopulated))// if (IsPopulated == "Y")
//                            {
//                                int MasterColumn = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Master").Select(s => new { CONFIG_ID = s.CONFIG_ID }).Count();
//                                int MeasureColmnsCount = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Measure").Select(s => new { CONFIG_ID = s.CONFIG_ID }).Count();
//                                var measurecolumn = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Measure").Select(s => new { Text = s.DATA_TYPE, Value = s.DISPLAY_TYPE, ConfigUserId = s.CONFIGUSER_ID }).OrderBy(s => s.ConfigUserId);
//                                var MasterShowColumn = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Master").Select(s => new { DATA_TYPE = s.DATA_TYPE, IS_SHOW = s.IS_SHOW });
//                                var MasterColumns = m.POC_CONFIG_STRUCTURE.Where(y => y.CONFIG_ID == configId).Where(o => o.COLLECTION_TYPE == "Master").Select(s => new { EntityGroupName = s.ENTITY_GROUP, EntiryField = s.FIELD_NAME });
//                                FileName = ot_details.SubjectArea + "_[T" + TemplateId + "]_[" + ForTime + "]_[" + OnTime + "]_[" + DateTime.Now.ToString("dd-MM-yyyy") + "-" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + " " + DateTime.Now.ToString("tt ") + "]" + ".xlsx";
//                                string FormedQuery4 = "SP_GETDROPDOWNDATA";
//                                DataSet ds4 = new DataSet();
//                                using (GetDataSet objDataSetValue = new GetDataSet())
//                                {
//                                    ds4 = objDataSetValue.GetDataSetValue(FormedQuery4, configId);
//                                }
//                                using (ExcelPackage objExcelPackage = new ExcelPackage())
//                                {
//                                    DataTable dsExcel = new DataTable();
//                                    var SecondList = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.IS_SHOW == "N")).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID, CollectionType = s.COLLECTION_TYPE, DataType = s.DISPLAY_TYPE }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//                                    var FirstList = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (s.IS_SHOW == "Y" || s.IS_SHOW == null)).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID, CollectionType = s.COLLECTION_TYPE, DataType = s.DISPLAY_TYPE }).OrderBy(y => y.CollectionType).ThenBy(t => t.CONFIGUSER_ID).ToList();
//                                    var totalList = FirstList.OrderBy(o => o.CollectionType).Concat(SecondList).ToList();
//                                    if (ds3.Tables.Count > 0)
//                                    {
//                                        DataTable table = ds3.Tables[0];

//                                        foreach (var column in totalList)
//                                        {
//                                            var DisplayName = m.GET_TIMECODE.Where(w => w.CONFIG_ID == configId && w.DATA_TYPE == column.DATA_TYPE && w.TEMPLATE_ID == TemplateId).Select(s => new { DisplayName = s.DISPLAY_NAME, DataType = s.DATA_TYPE }).FirstOrDefault();
//                                            if (DisplayName != null)
//                                                table.Columns[column.DATA_TYPE].ColumnName = DisplayName.DisplayName.Trim();
//                                            else
//                                                table.Columns[column.DATA_TYPE].ColumnName = column.DISPLAY_NAME;
//                                        }
//                                        table.AcceptChanges();
//                                        dsExcel = table;
//                                    }
//                                    dsExcel = ds3.Tables[0];
                                    
//                                    int rows = dsExcel.Rows.Count + 5;

//                                    int SetNoOfRecords = 0;
//                                    var objEP_SUBJECTAREA = (from ep in m.EP_SUBJECTAREA
//                                                             where ep.SUBJECTAREA_ID == SubjectAreaId
//                                                             select new { EXCEL_ROW_SIZE = ep.EXCEL_ROW_SIZE }).FirstOrDefault();

//                                    if (objEP_SUBJECTAREA.EXCEL_ROW_SIZE != null)
//                                        SetNoOfRecords = Convert.ToInt32(objEP_SUBJECTAREA.EXCEL_ROW_SIZE);
//                                    if (SetNoOfRecords > 0)
//                                    {
//                                        rows = SetNoOfRecords;

//                                    }
//                                    if (dsExcel.Rows.Count > 0)
//                                    {

//                                        ExcelWorksheet hiddenSheet = objExcelPackage.Workbook.Worksheets.Add(ConfigurationManager.AppSettings["HiddenSheetName"]);
//                                        hiddenSheet.Cells[1000000, 500].Value = FileCode;// Need to pass File Code 
//                                        hiddenSheet.Hidden = eWorkSheetHidden.VeryHidden;
//                                        //Create the worksheet    
//                                        dsExcel.TableName = "DataCollectionSheet";
//                                        ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add(dsExcel.TableName);
                                       
//                                        objWorksheet.Cells["A1"].Value = ot_details.SubjectArea + "- Data Collection Sheet";
//                                        objWorksheet.Cells["A2"].Value = "Reporting Start Date-" + Convert.ToDateTime(PeriodFrom).ToString("dd/MMM/yyyy");
//                                        objWorksheet.Cells["B2"].Value = "Reporting End Date-" + Convert.ToDateTime(PeriodTo).ToString("dd/MMM/yyyy");
//                                        objWorksheet.Cells["A3"].Value = "Assigned User-" + ot_details.UserName;
//                                        objWorksheet.Cells["B3"].Value = "Lock Date-" + Convert.ToDateTime(dtlockdate).ToString("dd/MMM/yyyy");
//                                        objWorksheet.Cells["C3"].Value = "Generation Date-" + DateTime.Now.Date.ToString("dd/MMM/yyyy");
//                                        objWorksheet.Cells["A4"].Value = "";// customexcel;
//                                        objWorksheet.Cells["A1:B1"].Merge = true;//.HideRow(4);
//                                        objWorksheet.DefaultColWidth = 30;
//                                        objWorksheet.Column(1).Style.WrapText = true;
//                                        objWorksheet.Row(4).Hidden = true;//Hide 4th row

//                                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1 
//                                        string DataSheetName = "DataEntrySheet";
//                                        ExcelWorksheet objWorksheet1 = objExcelPackage.Workbook.Worksheets.Add(DataSheetName);
//                                        objWorksheet1.Workbook.Worksheets[DataSheetName].Hidden = eWorkSheetHidden.VeryHidden;
//                                        try
//                                        {
//                                            objWorksheet1.Cells["A1"].LoadFromDataTable(dsExcel, true);
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            string ErrorDetails = "Error Details:Total Rows " + dsExcel.Rows.Count + " and ConfigId :" + configId + " and Subject Area Name:" + ot_details.SubjectArea + "  Assigned User: " + ot_details.UserName;
//                                            ErrorLog srsEx = new ErrorLog();
//                                            srsEx.LogErrorInTextFile(ex, ErrorDetails);
//                                            continue;

//                                        }

//                                        //Distinct Values
//                                        string DistinctDataSheetName = "DistinctDataEntrySheet";
//                                        ExcelWorksheet objDistinctWorksheet = objExcelPackage.Workbook.Worksheets.Add(DistinctDataSheetName);
//                                        objDistinctWorksheet.Workbook.Worksheets[DistinctDataSheetName].Hidden = eWorkSheetHidden.VeryHidden;

//                                        objWorksheet1.Cells.Style.Font.SetFromFont(new Font("Calibri", 10));

//                                        objWorksheet.Cells.Style.Font.SetFromFont(new Font("Calibri", 10));
//                                        char next = 'A';
//                                        int xx = 0;
//                                        //char next = incrementCharacter('A', MasterColumn);
//                                        List<string> LstMeasureColumns = new List<string>();
//                                        List<string> LstGrandTotalColumns = new List<string>();
//                                        foreach (var T in FirstList)
//                                        {

//                                            foreach (var s in measurecolumn)
//                                            {

//                                                if (T.DATA_TYPE == s.Text && s.Value == "DROPDOWN")
//                                                {
//                                                    next = incrementCharacter('A', xx);
//                                                    string resultchar = (next.ToString() + ':' + next.ToString()).ToString();
//                                                    IExcelDataValidation dataValidation;
//                                                    var lookuptype = objWorksheet.DataValidations.AddListValidation(resultchar.ToString());
//                                                    foreach (DataRow row in ds4.Tables[0].Rows) // Loop over the rows.
//                                                    {
//                                                        if (row.ItemArray[0].ToString() == s.Text)
//                                                        {
//                                                            lookuptype.Formula.Values.Add(row.ItemArray[2].ToString());

//                                                        }
//                                                        dataValidation = lookuptype;
//                                                        dataValidation.ShowErrorMessage = true;
//                                                        dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
//                                                        dataValidation.ErrorTitle = "An invalid value was entered";
//                                                        dataValidation.Error = "Select a value from the list";
//                                                    }
//                                                    LstMeasureColumns.Add(next.ToString());
//                                                    break;

//                                                }
//                                                if (s.Value == "TEXTBOX" && T.DATA_TYPE.Contains("N") && s.Value == T.DataType)
//                                                {
//                                                    next = incrementCharacter('A', xx);
//                                                    CellsNumeric(objWorksheet, next, 1);
//                                                    LstMeasureColumns.Add(next.ToString());
//                                                    LstGrandTotalColumns.Add(next.ToString());
//                                                    break;

//                                                }
//                                                if (s.Value == "TEXTBOX" && s.Value != "DROPDOWN" && T.DATA_TYPE.Contains("VC") && s.Value == T.DataType)
//                                                {
//                                                    next = incrementCharacter('A', xx);
//                                                    // CellsNumeric(objWorksheet, next, 1);
//                                                    LstMeasureColumns.Add(next.ToString());
//                                                    LstGrandTotalColumns.Add(next.ToString());
//                                                    break;

//                                                }
//                                                if (s.Value == "DATE" && T.DATA_TYPE.Contains("DT") && s.Value == T.DataType)
//                                                {
//                                                    next = incrementCharacter('A', xx);
//                                                    DateNumeric(objWorksheet, next, 1);
//                                                    LstMeasureColumns.Add(next.ToString());
//                                                    break;
//                                                }
//                                                if (s.Value == "TEXTBOX" && T.DATA_TYPE.Contains("P") && s.Value == T.DataType)
//                                                {
//                                                    next = incrementCharacter('A', xx);
//                                                    CellsNumericPercentage(objWorksheet, next, 1);
//                                                    LstMeasureColumns.Add(next.ToString());
//                                                    break;
//                                                }
//                                            }
//                                            xx++;
//                                        }


//                                        //                        /*Freeze Panes*/

//                                        char total = incrementCharacter('A', (dsExcel.Columns.Count - 1));


//                                        //Lock Sheet

//                                        objWorksheet.Protection.IsProtected = true;
//                                        if (LstMeasureColumns.Count > 0)
//                                        {
//                                            //Unlock Measure Columns
//                                            string S = "0";
//                                            string L = (dsExcel.Columns.Count - 1).ToString();

//                                            int LastRownumber = rows + 5;
//                                            //Change-11 Oct 2019
//                                            // int LastRownumber = dsExcel.Rows.Count + 5;
//                                            using (ExcelRange objRange = objWorksheet.Cells["A6" + ":" + total + LastRownumber])
//                                            {
//                                                objRange.Style.Locked = false;
//                                            }
//                                        }
//                                        objWorksheet.Protection.IsProtected = true;


//                                        if (!string.IsNullOrEmpty(FileCode))
//                                            objWorksheet.Protection.SetPassword(FileCode);
//                                        objWorksheet.Cells.AutoFitColumns();

//                                        hiddenSheet.Cells.AutoFitColumns();

//                                        objExcelPackage.Workbook.Protection.LockStructure = true;
//                                        //End
//                                        objWorksheet.View.FreezePanes(6, 1);
//                                        //Distinct Value
//                                        using (ExcelRange objRange = objDistinctWorksheet.Cells["A1:" + total.ToString() + "1"])
//                                        {
//                                            char nextCol = '\0';
//                                            char FirstCol = 'A';
//                                            char Preval = '\0';
                                            
//                                            for (int x = 0; x < dsExcel.Columns.Count; x++)
//                                            {
//                                                nextCol = incrementCharacter(FirstCol, x);
//                                                objDistinctWorksheet.Cells[nextCol + "1:" + nextCol + "1"].Value = dsExcel.Columns[x].Caption;
//                                                DataTable dtexcelrowcount = new DataTable();
//                                                dtexcelrowcount = dsExcel.DefaultView.ToTable(true, dsExcel.Columns[x].ToString());
//                                                int Startrow = 2;
//                                                int cellrow = 1;
//                                                int Countrows = dtexcelrowcount.Rows.Count;
//                                                //string PreviousVal = "";
//                                                //string CurrentVal = "";
//                                                for (int y = 0; y < dtexcelrowcount.Rows.Count; y++)
//                                                {
//                                                    objDistinctWorksheet.Cells[nextCol.ToString() + "" + Startrow + ":" + nextCol.ToString() + "" + Startrow + ""].Value = dtexcelrowcount.Rows[y][0];
//                                                    Startrow++;
//                                                }

//                                            }

//                                            // cellrow++;
//                                        }




//                                        //End

//                                        using (ExcelRange objRange = objWorksheet.Cells["A5:" + total.ToString() + "5"])
//                                        {

//                                            //Set Column name in excel
//                                            for (int c = 0; c < dsExcel.Columns.Count; c++)
//                                            {
//                                                char Columnnextval = incrementCharacter('A', c);
//                                                objWorksheet.Cells[Columnnextval + "5:" + Columnnextval + "5"].Value = dsExcel.Columns[c].Caption;
//                                            }
//                                            //End
//                                            objWorksheet.Cells.AutoFitColumns();

//                                            objRange.Style.Font.Bold = true;
//                                            objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                            objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Justify;
//                                            objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                            objRange.Style.Font.Color.SetColor(Color.Black);
//                                            objRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

//                                            /*Border lines should be added*/
//                                            objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                            //objWorksheet.Cells["C5:C5"].GroupBy() 
//                                            /// •	Allow filter for header
//                                            objRange.AutoFilter = true;
//                                            objWorksheet.Protection.AllowAutoFilter = true;

//                                            //Border lines should be added
//                                            int columnno = 6;
//                                            //  int rows = dsExcel.Rows.Count;
//                                            char Preval = '\0';
//                                            char nextval = '\0';
//                                            char FirstChar = 'A';
//                                            //int SetNoOfRecords = Convert.ToInt32(ConfigurationManager.AppSettings["IncreaseDecreaseRowsForIspopulatedNo"]) == 0 ? 1000 : Convert.ToInt32(ConfigurationManager.AppSettings["IncreaseDecreaseRowsForIspopulatedNo"]);
//                                            //if (rows > SetNoOfRecords)
//                                            //  rows = SetNoOfRecords;
//                                            for (int c = 1; c <= rows; c++)
//                                            {
//                                                int rr = columnno;
//                                                //Set Formula on rows

//                                                //End
//                                                objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                                objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                                objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                                objWorksheet.Cells["A" + columnno + ":" + total.ToString() + "" + columnno + ""].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

//                                                IExcelDataValidation dataValidation;
//                                                int CellStartVal = 2;
//                                                int CellEndRange = rows + 1;
//                                                string TotalExcelColumns = "A" + columnno + ":" + total.ToString() + "" + columnno + "";

//                                                List<string> Selectedlist = new List<string>();

//                                                for (int r = 0; r < dsExcel.Columns.Count - (SecondList.Count + MeasureColmnsCount); r++)
//                                                {
//                                                    nextval = incrementCharacter(FirstChar, r);
//                                                    string resultchar = (nextval.ToString() + ':' + nextval.ToString()).ToString();
//                                                    if (nextval == 'A')
//                                                    {
//                                                        //Get Discrint Value
//                                                        DataTable dtexcelrowcount = new DataTable();
//                                                        dtexcelrowcount = dsExcel.DefaultView.ToTable(true, dsExcel.Columns[r].ToString());

//                                                        int Countrows = dtexcelrowcount.Rows.Count + CellStartVal;

//                                                        var lookuptype = objWorksheet.DataValidations.AddListValidation("A" + columnno + ":A" + columnno + "");
//                                                        //lookuptype.Formula.ExcelFormula = "=" + DataSheetName + "!$A$" + CellStartVal + ":$A$" + CellEndRange + "";//=DataEntrySheet!$A$2:$A$24
//                                                        lookuptype.Formula.ExcelFormula = "=" + DistinctDataSheetName + "!$A$" + CellStartVal + ":$A$" + Countrows + "";
//                                                        //int i = 0;
//                                                        //foreach (DataRow dritem in dtexcelrowcount.Rows)
//                                                        //{

//                                                        //    lookuptype.Formula.Values.Add(dtexcelrowcount.Rows[i].ItemArray[0].ToString());
//                                                        //    i++;
//                                                        //}
//                                                        //lookuptype.AllowBlank = true;
//                                                        dataValidation = lookuptype;
//                                                        dataValidation.ShowErrorMessage = true;
//                                                        dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
//                                                        dataValidation.ErrorTitle = "An invalid value was entered";
//                                                        dataValidation.Error = "Select a value from the list";
//                                                        Preval = nextval;
//                                                        // Countrows = 0;
//                                                    }
//                                                    else
//                                                    {
//                                                        if (nextval != Preval)
//                                                        {
//                                                            //Get Discrint Value
//                                                            DataTable dtexcelrowcount = new DataTable();
//                                                            dtexcelrowcount = dsExcel.DefaultView.ToTable(true, dsExcel.Columns[r].ToString());
//                                                            int Countrows = dtexcelrowcount.Rows.Count + CellStartVal;
//                                                            var lookuptype = objWorksheet.DataValidations.AddListValidation(nextval + "" + columnno + ":" + nextval.ToString() + "" + columnno + "");
//                                                            lookuptype.Formula.ExcelFormula = "OFFSET(" + DistinctDataSheetName + "!$" + nextval + "$" + CellStartVal + ",0,0,COUNTA(" + DistinctDataSheetName + "!$" + nextval + "$" + CellStartVal + ":$" + nextval + "$" + Countrows + "),1)";//Remove Balnk Value from dropdown list
//                                                                                                                                                                                                                                                                                                 // = OFFSET(DistinctDataEntrySheet!$B$2, 0, 0, COUNTA(DistinctDataEntrySheet!$B$2:$B$8), 1)
//                                                                                                                                                                                                                                                                                                 //lookuptype.AllowBlank = true;

//                                                            dataValidation = lookuptype;
//                                                            dataValidation.ShowErrorMessage = true;
//                                                            dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
//                                                            dataValidation.ErrorTitle = "An invalid value was entered";
//                                                            dataValidation.Error = "Select a value from the list";


//                                                            Preval = nextval;
//                                                            // Countrows = 0;
//                                                        }
//                                                    }
//                                                }

//                                                columnno++;
//                                            }

//                                            // Set formula at master columns which is mention after measure columns in excel sheet
//                                            int CellVal = 6;
//                                            int Cellrows = rows + 1;
//                                            //change 11 oct 
//                                            // int Cellrows = dsExcel.Rows.Count + 1;

//                                            //  int TotalCellrows = dsExcel.Rows.Count + 1;
//                                            int TotalCellrows = dsExcel.Rows.Count + 1;
//                                            // int TotalCellrows = rows + 1;
//                                            //if (Cellrows > SetNoOfRecords)
//                                            //    Cellrows = SetNoOfRecords;

//                                            for (int c = 1; c <= Cellrows; c++)
//                                            {
//                                                int columncount = FirstList.Count;
//                                                int StartMastercolumns = FirstList.Count;// SecondList.Count + MeasureColmnsCount;
//                                                char nextvalForMasterColumns = '\0';
//                                                char nextvalColumns = '\0';

//                                                int a = 0;

//                                                for (int S = StartMastercolumns; S < dsExcel.Columns.Count; S++)
//                                                {


//                                                    nextvalColumns = incrementCharacter('A', a);
//                                                    a++;
//                                                    nextvalForMasterColumns = incrementCharacter(nextval, MeasureColmnsCount + a);
//                                                    objWorksheet.Cells[nextvalForMasterColumns + "" + CellVal + ":" + nextvalForMasterColumns + "" + CellVal + ""].Formula = "=INDEX(" + DataSheetName + "!$" + nextvalForMasterColumns + "2:$" + nextvalForMasterColumns + "" + TotalCellrows + ", MATCH(" + dsExcel.TableName + "!$" + nextvalColumns + "" + CellVal + ", " + DataSheetName + "!$" + nextvalColumns + "$2:$" + nextvalColumns + "$" + TotalCellrows + ", 0),0)";
//                                                    objWorksheet.Column(S + 1).Hidden = true;
//                                                }

//                                                CellVal++;
//                                            }


//                                        }

//                                        using (ExcelRange objRange = objWorksheet.Cells["A1:" + total.ToString() + "1"])
//                                        {
//                                            objRange.Style.Font.Bold = true;
//                                            objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                            objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//                                            objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                            objRange.Style.Font.Color.SetColor(Color.White);

//                                            objRange.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
//                                            /*Border lines should be added*/
//                                            objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

//                                        }
//                                        using (ExcelRange objRange = objWorksheet.Cells["A2:" + total.ToString() + "2"])
//                                        {
//                                            objRange.Style.Font.Bold = true;
//                                            objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                            objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//                                            objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                            objRange.Style.Font.Color.SetColor(Color.White);
//                                            objRange.Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);
//                                            /*Border lines should be added*/
//                                            objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                        }
//                                        using (ExcelRange objRange = objWorksheet.Cells["A3:" + total.ToString() + "3"])
//                                        {
//                                            objRange.Style.Font.Bold = true;
//                                            objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                            objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//                                            objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                            objRange.Style.Font.Color.SetColor(Color.White);
//                                            objRange.Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);
//                                            /*Border lines should be added*/
//                                            objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                        }
//                                        using (ExcelRange objRange = objWorksheet.Cells["A4:" + total.ToString() + "4"])
//                                        {
//                                            objRange.Style.Font.Bold = true;
//                                            objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
//                                            objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//                                            objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                                            objRange.Style.Font.Color.SetColor(Color.White);
//                                            objRange.Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);
//                                            /*Border lines should be added*/
//                                            objRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
//                                            objRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
//                                        }

//                                        string filepath = System.Configuration.ConfigurationManager.AppSettings["BatchJobErrorlocation"];
//                                        string path = Path.Combine(filepath, FileName);
//                                        //string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Excel"), FileName);
//                                        //Create excel file on physical disk    
//                                        FileStream objFileStrm = System.IO.File.Create(path);
//                                        objFileStrm.Close();
//                                        //Write content to excel file    
//                                        System.IO.File.WriteAllBytes(path, objExcelPackage.GetAsByteArray());

//                                        NameValueCollection mailBodyplaceHolders = new NameValueCollection();
//                                        string ChangedSubjectArea = ot_details.SubjectArea.Replace("_", " ");
//                                        ChangedSubjectArea = new CultureInfo("en-US").TextInfo.ToTitleCase(ChangedSubjectArea.ToLower());

//                                        mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
//                                        mailBodyplaceHolders.Add("<SubjectArea>", ChangedSubjectArea);
//                                        mailBodyplaceHolders.Add("<ForTimeCode>", ForTime);
//                                        mailBodyplaceHolders.Add("<OnTimeCode>", OnTime);
//                                        mailBodyplaceHolders.Add("<LockDate>", dtlockdate.ToString());
//                                        mailBodyplaceHolders.Add("<Custom>", custom.ToString());
//                                        //Format the header    
//                                        string DataCollectionSubject = "[iQlik Portal] - Data collection template for  [" + ot_details.SubjectArea + "] -[" + ot_details.ReportingPeriod + "]";
//                                        string mailbody = "";
//                                        string messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["TemplateGenerationEmail"].ToString());
//                                        mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);

//                                        try
//                                        {
//                                            if (TemplateChkId > 0 && IfDataExistForIsPopulatedYes)
//                                            {
//                                                SendMail(path + "," + GetExcelFilePathForIspopulatedYes(TemplateChkId), DataCollectionSubject, mailbody, ot_details.UserEmail);
//                                            }
//                                            else
//                                            {
//                                                SendMail(path, DataCollectionSubject, mailbody, ot_details.UserEmail);
//                                            }
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            EP_SENDORRESENDTASK ESP = m.EP_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
//                                            ESP.ATTEMPTS++;
//                                            m.SaveChanges();
//                                        }

//                                    }
//                                }
//                            }

//                        }
//                        EP_TEMPLATE EPM = m.EP_TEMPLATE.Where(x => x.TEMPLATE_ID == TemplateId).FirstOrDefault();
//                        if (FileName != "")
//                        {
//                            EPM.FILE_NAME = FileName;
//                        }
//                        EPM.UPDATEDBY = UserId;
//                        EPM.UPDATEDON = DateTime.Now;
//                        if (documentid == "" || string.IsNullOrEmpty(documentid))
//                        {
//                            if (!string.IsNullOrEmpty(FileCode))
//                            {
//                                EPM.FILE_CODE = FileCode;
//                            }

//                        }
//                        else if (!string.IsNullOrEmpty(documentid) && DataCollection.ToUpper() == "OFFLINE")
//                        {
//                            EPM.FILE_CODE = FileCode;
//                        }

//                        m.SaveChanges();
//                        EP_SENDORRESENDTASK esppp = m.EP_SENDORRESENDTASK.Where(x => x.ID == SendResendId).FirstOrDefault();
//                        esppp.SENT = 1;
//                        esppp.ATTEMPTS++;
//                        esppp.ID = SendResendId;
//                        m.SaveChanges();
//                    }

//                    catch (Exception ex)
//                    {
//                        ErrorLog srsEx = new ErrorLog();
//                        srsEx.LogErrorInTextFile(ex);
//                        continue;
//                    }
//                }//Main For loop Close
//                SendUploadMail();
//                SendEscalationMail();

//            }
//            catch (Exception ex)
//            {
//                ErrorLog srsEx = new ErrorLog();
//                srsEx.LogErrorInTextFile(ex);
//            }

//            return ds;

//        }
//        public void SendUploadMail()
//        {
//            SPONGE_Context m = new SPONGE_Context();
//            GetDataSet gd = new GetDataSet();
//            DataSet ds = new DataSet();
//            //ErrorLog lg = new ErrorLog();
//            ErrorLog lg = new ErrorLog();
//            ds = gd.GetDataSetValue("SP_GETUPLOADEMAIL", 0);
//            try
//            {
//                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
//                {
//                    DataSet custom_ds = new DataSet();
//                    StringBuilder custom = new StringBuilder();
//                    custom_ds = gd.GetDataSetValue("SP_GETMASTEREMAIL", Convert.ToInt32(ds.Tables[0].Rows[i].ItemArray[8]));
//                    for (int v = 0; v < custom_ds.Tables[0].Rows.Count; v++)
//                    {
//                        custom.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
//                        custom.Append("\r\n");
//                    }
//                    decimal configId = Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[8]);
//                    decimal TemplateId = Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[0]);
//                    var ot_details = (from ep in m.POC_CONFIG_STRUCTURE
//                                      join pd in m.EP_SUBJECTAREA on ep.SUBJECTAREA_ID equals pd.SUBJECTAREA_ID
//                                      join us in m.EP_USERS on ep.USER_ID equals us.ID
//                                      where ep.CONFIG_ID == configId
//                                      select new { UserName = us.NAME, ReportingPeriod = pd.REPORTING_PERIOD, UserEmail = us.EMAILID }).FirstOrDefault();
//                    NameValueCollection mailBodyplaceHolders = new NameValueCollection();
//                    mailBodyplaceHolders.Add("<FileName>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[9]));
//                    mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
//                    mailBodyplaceHolders.Add("<SubjectArea>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[3]));
//                    mailBodyplaceHolders.Add("<ForTimeCode>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[1]));
//                    mailBodyplaceHolders.Add("<OnTimeCode>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[2]));
//                    mailBodyplaceHolders.Add("<LockDate>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[6]));
//                    mailBodyplaceHolders.Add("<Custom>", custom.ToString());

//                    string DataCollectionSubject = "[iQlik Portal] - Upload Reminder template for  [" + Convert.ToString(ds.Tables[0].Rows[i].ItemArray[3]) + "] -[" + ot_details.ReportingPeriod + "]";
//                    string mailbody = "";
//                    string messageTemplatePath = "";
//                    if (Convert.ToString(ds.Tables[0].Rows[i].ItemArray[7]) == "Online")
//                    {
//                        messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["UploadReminderMailOnlineTemplate"].ToString());
//                    }
//                    else
//                    {
//                        messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["UploadReminderMailExcelTemplate"].ToString());
//                    }
//                    mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
//                    SendMail("", DataCollectionSubject, mailbody, ot_details.UserEmail);
//                    // lg.LogMessageInTextFile("Mail Sent Upload Reminder ID - " + configId + DateTime.Now.ToString());


//                    EP_TEMPLATE et = m.EP_TEMPLATE.Where(x => x.TEMPLATE_ID == TemplateId).FirstOrDefault();
//                    et.UPLOAD_REMINDER_SENT = 1;
//                    m.SaveChanges();
//                    //  lg.LogMessageInTextFile("Updated EP_TEMPLATE ID-" + configId + DateTime.Now.ToString());
//                }

//            }
//            catch (Exception ex)
//            {
//                ErrorLog srsEx = new ErrorLog();
//                srsEx.LogErrorInTextFile(ex);

//            }

//        }

//        public void SendEscalationMail()
//        {
//            SPONGE_Context m = new SPONGE_Context();
//            GetDataSet gd = new GetDataSet();
//            DataSet ds = new DataSet();
//            ErrorLog lg = new ErrorLog();
//            ds = gd.GetDataSetValue("SP_GETESCALATION", 0);
//            try
//            {
//                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
//                {
//                    DataSet custom_ds = new DataSet();
//                    StringBuilder custom = new StringBuilder();
//                    custom_ds = gd.GetDataSetValue("SP_GETMASTEREMAIL", Convert.ToInt32(ds.Tables[0].Rows[i].ItemArray[8]));
//                    for (int v = 0; v < custom_ds.Tables[0].Rows.Count; v++)
//                    {
//                        custom.Append(custom_ds.Tables[0].Rows[v].ItemArray[0]);
//                        custom.Append("\r\n");
//                    }
//                    decimal configId = Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[8]);
//                    decimal TemplateId = Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[0]);
//                    var ot_details = (from ep in m.POC_CONFIG_STRUCTURE
//                                      join pd in m.EP_SUBJECTAREA on ep.SUBJECTAREA_ID equals pd.SUBJECTAREA_ID
//                                      join us in m.EP_USERS on ep.USER_ID equals us.ID
//                                      join ap in m.POC_CONFIGURATION on ep.CONFIG_ID equals ap.CONFIG_ID
//                                      where ep.CONFIG_ID == configId
//                                      select new { UserName = us.NAME, ReportingPeriod = pd.REPORTING_PERIOD, UserEMail = us.EMAILID, ApproverEmail = ap.APPROVER_EMAILD, ManagerEmail = us.MANAGERMAILID }).FirstOrDefault();
//                    NameValueCollection mailBodyplaceHolders = new NameValueCollection();
//                    mailBodyplaceHolders.Add("<FileName>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[9]));
//                    mailBodyplaceHolders.Add("<UserName>", ot_details.UserName);
//                    mailBodyplaceHolders.Add("<SubjectArea>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[3]));
//                    mailBodyplaceHolders.Add("<ForTimeCode>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[1]));
//                    mailBodyplaceHolders.Add("<OnTimeCode>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[2]));
//                    mailBodyplaceHolders.Add("<LockDate>", Convert.ToString(ds.Tables[0].Rows[i].ItemArray[6]));
//                    mailBodyplaceHolders.Add("<Custom>", custom.ToString());

//                    string DataCollectionSubject = "[iQlik Portal] - Escalation template for  [" + Convert.ToString(ds.Tables[0].Rows[i].ItemArray[3]) + "] -[" + ot_details.ReportingPeriod + "]";
//                    string mailbody = "";
//                    string messageTemplatePath = "";
//                    if (Convert.ToString(ds.Tables[0].Rows[i].ItemArray[7]) == "Online")
//                    {
//                        messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["EscalationMailOnlineTemplate"].ToString());
//                    }
//                    else
//                    {
//                        messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["EscalationMailExcelTemplate"].ToString());
//                    }
//                    mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
//                    SendMail("", DataCollectionSubject, mailbody, ot_details.UserEMail);
//                    SendMail("", DataCollectionSubject, mailbody, ot_details.ApproverEmail);
//                    SendMail("", DataCollectionSubject, mailbody, ot_details.ManagerEmail);
//                    //lg.LogMessageInTextFile("Mail Sent Esclataion Reminder" + DateTime.Now.ToString());

//                    EP_TEMPLATE et = m.EP_TEMPLATE.Where(x => x.TEMPLATE_ID == TemplateId).FirstOrDefault();
//                    et.ESCALATION_REMINDER_SENT = 1;
//                    m.SaveChanges();
//                    // lg.LogMessageInTextFile("Updated in EP_Template  ID-" + configId + DateTime.Now.ToString());
//                }
//            }
//            catch (Exception ex)
//            {
//                ErrorLog srsEx = new ErrorLog();
//                srsEx.LogErrorInTextFile(ex);

//            }
//        }
//        public void SendMailIsHtml(string filename, string subject, string mailbody, string MailID)
//        {
//            SmtpClient smtpClient = new SmtpClient();
//            smtpClient.Host = ConfigurationManager.AppSettings["SMTPHost"].ToString();

//            MailMessage mailMessage = new MailMessage();
//            mailMessage.Body = mailbody;
//            mailMessage.Subject = subject;
//            mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"].ToString());
//            mailMessage.To.Add(new MailAddress(MailID));
//            mailMessage.IsBodyHtml = true;
//            if (!string.IsNullOrEmpty(filename))
//            {
//                mailMessage.Attachments.Add(new Attachment(filename));
//            }

//            smtpClient.Send(mailMessage);

//        }

//        public string GetMessageBody(string messageTemplate, NameValueCollection nvc)
//        {
//            messageTemplate = ReplacePlaceHolders(messageTemplate, nvc);
//            return messageTemplate;
//        }

//        private string ReplacePlaceHolders(string text, NameValueCollection valueCollection)
//        {
//            if (valueCollection == null || valueCollection.Count <= 0)
//            {
//                throw new ArgumentException("Invalid NameValueCollection");
//            }
//            //string text=null;
//            string result = text;
//            string value;
//            foreach (string key in valueCollection.AllKeys)
//            {
//                value = valueCollection[key];
//                result = result.Replace(key, value);
//            }
//            return result;
//        }


//        public void SendMailForErrorMsg(string subject, string mailbody, string MailID)
//        {


//            SmtpClient smtpClient = new SmtpClient();
//            smtpClient.Host = ConfigurationManager.AppSettings["SMTPHost"].ToString();

//            MailMessage mailMessage = new MailMessage();
//            mailMessage.Body = mailbody;
//            mailMessage.Subject = subject;
//            mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"].ToString());
//            mailMessage.To.Add(new MailAddress(MailID));
//            //mailMessage.IsBodyHtml = true;
//            //if (filename != "")
//            //{
//            //    mailMessage.Attachments.Add(new Attachment(filename));
//            //}

//            smtpClient.Send(mailMessage);
//        }
//        public void SendMail(string filename, string subject, string mailbody, string MailID)
//        {
//            SmtpClient smtpClient = new SmtpClient();
//            smtpClient.Host = ConfigurationManager.AppSettings["SMTPHost"].ToString();

//            MailMessage mailMessage = new MailMessage();
//            mailMessage.Body = mailbody;
//            mailMessage.Subject = subject;
//            mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"].ToString());
//            mailMessage.To.Add(new MailAddress(MailID));
//            //mailMessage.IsBodyHtml = true;
//            if (filename != "")
//            {
//                var attachment = filename.Split(',');
//                foreach (var item in attachment)
//                {
//                    mailMessage.Attachments.Add(new Attachment(item));
//                }
//            }

//            smtpClient.Send(mailMessage);

//        }
//        public static DataTable MergeTablesByIndex(DataTable t1, DataTable t2)
//        {
//            if (t1 == null || t2 == null) throw new ArgumentNullException("t1 or t2", "Both tables must not be null");

//            DataTable t3 = t1.Clone();  // first add columns from table1
//            foreach (DataColumn col in t2.Columns)
//            {
//                string newColumnName = col.ColumnName;
//                int colNum = 1;
//                while (t3.Columns.Contains(newColumnName))
//                {
//                    newColumnName = string.Format("{0}_{1}", col.ColumnName, ++colNum);
//                }
//                t3.Columns.Add(newColumnName, col.DataType);
//            }
//            var mergedRows = t1.AsEnumerable().Zip(t2.AsEnumerable(),
//                (r1, r2) => r1.ItemArray.Concat(r2.ItemArray).ToArray());
//            foreach (object[] rowFields in mergedRows)
//                t3.Rows.Add(rowFields);

//            return t3;
//        }
//        private void ProcessDataSetHeader(ref DataSet ds, ref DataSet ds1, decimal id)
//        {
//            SPONGE_Context m = new SPONGE_Context();
//            //var j = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == id).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID);
//            var G = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == id && (s.FIELD_NAME.Contains("_CODE") || s.FIELD_NAME.Contains("_CD"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//            var H = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == id && !(s.FIELD_NAME.Contains("_CD")) && (!s.FIELD_NAME.Contains("_CODE"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//            // var I = m.POC_CONFIG_STRUCTURE.Where(s => s.CONFIG_ID == configId && (!s.FIELD_NAME.Contains("_CODE"))).Select(s => new { DISPLAY_NAME = s.DISPLAY_NAME, DATA_TYPE = s.DATA_TYPE, CONFIGUSER_ID = s.CONFIGUSER_ID }).OrderBy(y => y.CONFIGUSER_ID).ToList();
//            var j = H.Concat(G);
//            DataTable table = ds.Tables[0];
//            DataTable table1 = ds1.Tables[0];
//            table = MergeTablesByIndex(table, table1);
//            //table.Merge(table1);

//            foreach (var column in j)
//            {
//                table.Columns[column.DATA_TYPE].ColumnName = column.DISPLAY_NAME;
//            }
//            table.AcceptChanges();
//        }
//        char incrementCharacter(char input, int number)
//        {
//            return (input == 'z' ? 'a' : (char)(input + number));
//        }
//        private void CellsNumeric(ExcelWorksheet objWorksheet, char nextcharacter, int number)
//        {

//            string resultchar = (nextcharacter.ToString() + ':' + nextcharacter.ToString()).ToString();
//            var val = objWorksheet.DataValidations.AddDecimalValidation(resultchar.ToString());
//            val.ShowErrorMessage = true;
//            IExcelDataValidation dataValidation;
//            long minValue = Convert.ToInt64(ConfigurationManager.AppSettings["NumberMin"]);
//            long maxValue = Convert.ToInt64(ConfigurationManager.AppSettings["NumberMax"]);

//            // If UoM1 == 9Litres then Decimal else Integer

//            val.Formula.Value = minValue;
//            val.Formula2.Value = maxValue;
//            dataValidation = val;

//            dataValidation.Error = "Enter The Value between " + minValue + "  to  " + maxValue + "";
//            dataValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
//            dataValidation.ErrorTitle = "Input Validation";
//            dataValidation.ShowErrorMessage = true;
//            dataValidation.AllowBlank = true;

//            //  objWorksheet.Cells[resultchar].Style.Numberformat.Format = "#.00";
//            objWorksheet.Cells[resultchar].Style.Numberformat.Format = "####.0000";

//        }
//        private void DateNumeric(ExcelWorksheet objWorksheet, char nextcharacter, int number)
//        {

//            string resultchar = (nextcharacter.ToString() + ':' + nextcharacter.ToString()).ToString();
//            var val = objWorksheet.DataValidations.AddDateTimeValidation(resultchar.ToString());
//            val.ShowErrorMessage = true;
//            IExcelDataValidation dataValidation;
//            DateTime minValue = DateTime.MinValue;
//            DateTime maxValue = DateTime.MaxValue;

//            // If UoM1 == 9Litres then Decimal else Integer

//            val.Formula.Value = minValue;
//            val.Formula2.Value = maxValue;
//            dataValidation = val;

//            dataValidation.Error = "Error! Invalid date.Date format should be MM/DD/YYYY";//"Invalid Date! ";
//            dataValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
//            dataValidation.ErrorTitle = "Input Validation";
//            dataValidation.ShowErrorMessage = true;
//            dataValidation.AllowBlank = true;
//            //objWorksheet.Cells[resultchar].Value="MM/DD/YYYY";
//            objWorksheet.Cells[resultchar].AutoFitColumns();
//            objWorksheet.Cells[resultchar].Style.Numberformat.Format = "MM/DD/YYYY";



//        }

//        private void CellsNumericPercentage(ExcelWorksheet objWorksheet, char nextcharacter, int number)
//        {

//            string resultchar = (nextcharacter.ToString() + ':' + nextcharacter.ToString()).ToString();
//            //var val = objWorksheet.DataValidations.AddIntegerValidation(resultchar.ToString());
//            var val = objWorksheet.DataValidations.AddDecimalValidation(resultchar.ToString());
//            val.ShowErrorMessage = true;
//            IExcelDataValidation dataValidation;
//            double minValue = Convert.ToDouble(ConfigurationManager.AppSettings["PercentageMin"]);
//            double maxValue = Convert.ToDouble(ConfigurationManager.AppSettings["PercentageMax"]);


//            // If UoM1 == 9Litres then Decimal else Integer

//            val.Formula.Value = minValue;
//            val.Formula2.Value = maxValue;
//            dataValidation = val;

//            dataValidation.Error = "Enter The Value between " + minValue + " to " + maxValue + "";
//            dataValidation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
//            dataValidation.ErrorTitle = "Input Validation";
//            dataValidation.ShowErrorMessage = true;
//            dataValidation.AllowBlank = true;

//            objWorksheet.Cells[resultchar].Style.Numberformat.Format = "##.00";

//        }
//    }

//}




