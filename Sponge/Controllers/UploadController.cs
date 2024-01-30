using DAL.Common;
using DAL;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sponge.Common;
using Sponge.Models;
using Sponge.ViewModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Text;
using OfficeOpenXml;
using LinqToExcel;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class UploadController : Controller
    {
        public const string HiddenSheetName = "SPG_HIDDENSHEETCODE";
        public const int HiddenFileCodeRowIndex = 1000000;
        public const int HiddenFileCodeColIndex = 500;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOptions<AppSettings> _settings;
        private readonly Email _email;

        public UploadController(ILogger<HomeController> logger, IOptions<AppSettings> settings, IConfiguration configuration, Email email)
        {
            _logger = logger;
            _configuration = configuration;
            _email = email;
            _settings = settings;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public JsonResult UploadExcelFiles()
        {
            string DeletedFileNames = Request.Form["filesToDelete"].ToString();
            var splitarray1 = (string[])null;

            if (!string.IsNullOrEmpty(DeletedFileNames))
            {
                string[] splitarray = DeletedFileNames.Split(',');
                splitarray1 = DeletedFileNames.Split(',');
            }
            SPONGE_Context dbcontext = new();
            string TempFilePath = _configuration["AppSettings:TempDocumentFilePath"];
            string UploadedDocumentsFilePath = _configuration["AppSettings:UploadedDocumentsFilePath"];
            string WarningDocumentFilePath = _configuration["AppSettings:WarningDocumentFilePath"];
            string DataApproverRoleId = _configuration["AppSettings:DataApproverRoleId"] == null ? _configuration["AppSettings:AdminRoleId"] : _configuration["AppSettings:DataApproverRoleId"];
            string approverUserId = string.Empty;
            if (!Directory.Exists(TempFilePath))
                Directory.CreateDirectory(TempFilePath);
            string UserRole = HttpContext.Session.GetString("ROLE").ToString();
            string UserName = HttpContext.Session.GetString("NAME").ToString();
            string[] userId = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            FileModel objfiledetails = new FileModel();
            TemplateFile objTemplateFilelist = new TemplateFile();
            List<TemplateFile> listErros = new List<TemplateFile>();
            for (int i = 0; i < Request.Form.Files.Count; i++)
            {
                bool IsFileExist = false;
                var file = Request.Form.Files[i];//Uploaded file
                long fileSize = file.Length;
                string mimeType = file.ContentType;
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');

                using (var fileContent = new FileStream(Path.Combine(TempFilePath, fileName), FileMode.Create))
                {
                     file.CopyTo(fileContent);
                }
                KillExcelProcess();
                string Filename = Path.GetFileName(fileName);
                if (splitarray1 != null)
                {
                    foreach (var s in splitarray1)
                    {
                        if (s == Filename)
                        {
                            IsFileExist = true;
                            break;
                        }
                    }
                }
                if (IsFileExist == true)
                    continue;

                FileModel objFileModel = new FileModel();

                if (UserRole.ToUpper() == "ADMIN" || UserRole.ToUpper()== "DATA CONFIGURE")
                {
                    objFileModel = Checkauthorizeduser(fileName, userId, objFileModel, true);//Check authorized user           
                    if (objFileModel != null && objFileModel.SubFunctionID > 0)
                    {
                        approverUserId = userId[1];
                        listErros = ValidateFileupload(listErros, objFileModel, fileName, TempFilePath, userId[1], approverUserId);
                        TemplateFile result = listErros.Find(x => x.FileName == fileName);
                        if (listErros.Count != 0 && (result != null))
                        {
                            continue;
                        }
                        else
                        {
                            listErros = SaveUploadedExcelFile(listErros, TempFilePath + "\\" + Filename, TempFilePath, userId[1], objFileModel, approverUserId, UploadedDocumentsFilePath, WarningDocumentFilePath);//Save Excel to DB

                            TemplateFile result2 = listErros.Find(x => x.FileName == fileName);
                            if (result2.ErrorType == "S")
                                //SentMailToUploader(objFileModel);
                            continue;
                        }


                    }
                    else
                    {
                        listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file! Please upload the valid excel template which is sent to you;" });
                        continue;

                    }
                }
                else if (UserRole.ToUpper() == "DATA APPROVER")
                {
                    objFileModel = Checkauthorizeduser(fileName, userId, objFileModel, true);// //Check authorized user
                                                                                             // decimal? SubfunctionId = ValidateAdminDetails(fileName);
                    if (objFileModel != null && objFileModel.SubFunctionID > 0)
                    {
                        approverUserId = objFileModel.ApproverID;
                        listErros = ValidateFileupload(listErros, objFileModel, fileName, TempFilePath, userId[1], approverUserId);
                        TemplateFile result = listErros.Find(x => x.FileName == fileName);
                        if (listErros.Count != 0 && (result != null))
                        {
                            continue;
                        }
                        else
                        {
                            listErros = SaveUploadedExcelFile(listErros, TempFilePath + "\\" + Filename, TempFilePath, userId[1], objFileModel, approverUserId, UploadedDocumentsFilePath, WarningDocumentFilePath);//Save Excel to DB

                            TemplateFile result2 = listErros.Find(x => x.FileName == fileName);

                            if (result2.ErrorType == "S")
                                //SentMailToUploader(objFileModel);

                            continue;
                        }
                    }
                    else
                    {
                        listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file! Please upload the valid excel template which is sent to you;" });
                        continue;

                    }
                }
                else
                {
                    //End
                    objFileModel = Checkauthorizeduser(fileName, userId, objFileModel, false);// //Check authorized user                  
                    if (objFileModel != null)
                    {
                        approverUserId = objFileModel.ApproverID;
                        listErros = ValidateFileupload(listErros, objFileModel, fileName, TempFilePath, userId[1], approverUserId);
                        TemplateFile result = listErros.Find(x => x.FileName == fileName);
                        if (listErros.Count != 0 && (result != null))
                        {
                            continue;
                        }
                        else
                        {
                            listErros = SaveUploadedExcelFile(listErros, TempFilePath + "\\" + Filename, TempFilePath, userId[1], objFileModel, approverUserId, UploadedDocumentsFilePath, WarningDocumentFilePath);//Save Excel to DB
                            TemplateFile result2 = listErros.Find(x => x.FileName == fileName);
                            if (result2.ErrorType == "S")
                                //SentMailToUploader(objFileModel);

                            continue;
                        }
                    }
                    else
                    {
                        listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file! Please upload the valid excel template which is sent to you;" });
                        continue;
                    }
                }
            }

            return Json(new { UploadedFileCount = Request.Form.Files.Count, ErrorList = listErros });
        }
        public FileModel Checkauthorizeduser(string FileName, string[] userId, FileModel objFileModel, bool IsAdminOrDataApprover)
        {

            SPONGE_Context objContext = new();
            SPG_USERS objUser = new();
            
            if (IsAdminOrDataApprover)
            {
                objFileModel = (from TD in objContext.SPG_TEMPLATE
                                join PC in objContext.SPG_CONFIGURATION on TD.CONFIG_ID.Value equals PC.CONFIG_ID
                                join SA in objContext.SPG_SUBJECTAREA on PC.SUBJECTAREA_ID equals SA.SUBJECTAREA_ID
                                //join EU in objContext.EP_USERS on PC.USER_ID equals EU.ID
                                join ED in objContext.SPG_DOCUMENT on TD.TEMPLATE_ID equals ED.TEMPLATEID into EDD
                                from ED in EDD.DefaultIfEmpty()
                                where TD.FILE_NAME.Equals(FileName.ToString())
                                select new FileModel
                                {

                                    ApproverID = PC.APPROVER_ID,
                                    ApproverEmailID = PC.APPROVER_EMAILD,
                                    ApproverName = PC.APPROVER_NAME,
                                    SubFunctionID = SA.SUBFUNCTION_ID,
                                    TemplateID = TD.TEMPLATE_ID,
                                    FileName = TD.FILE_NAME,
                                    ConFigId = PC.CONFIG_ID,
                                    FileCode = TD.FILE_CODE,
                                    PERIOD_TO = TD.PERIOD_TO,
                                    PERIOD_FROM = TD.PERIOD_FROM,
                                    SUBJECTAREA_NAME = SA.SUBJECTAREA_NAME,
                                    //RoleID = Convert.ToDecimal(EU.ROLE_ID.HasValue ? EU.ROLE_ID : int.MinValue),

                                    DocumentID = ED.ID,
                                    DocumentFileNAME = ED.FILE_NAME,
                                    DOCTEMPLATEID = ED.TEMPLATEID,
                                    LockDate = TD.LOCK_DATE,
                                    UploderUserId = PC.USER_ID,
                                    //UserName = EU.NAME,
                                    //UploderEmailId = EU.EMAILID,
                                    OnTime = TD.ONTIMECODE,
                                    ForTime = TD.FORTIMECODE
                                }).FirstOrDefault();
                if (objFileModel != null)
                {
                    objUser = objContext.SPG_USERS.Where(a => a.USER_ID == objFileModel.UploderUserId).FirstOrDefault();
                    //objFileModel.RoleID = objUser.ROLE_ID;
                    objFileModel.UserName = objUser.Name;
                    objFileModel.UploderEmailId = objUser.EMAIL_ID;
                }
            }
            else
            {

                objFileModel = (from TD in objContext.SPG_TEMPLATE
                                join PC in objContext.SPG_CONFIGURATION on TD.CONFIG_ID.Value equals PC.CONFIG_ID
                                join SA in objContext.SPG_SUBJECTAREA on PC.SUBJECTAREA_ID equals SA.SUBJECTAREA_ID
                                //join EU in objContext.EP_USERS on PC.USER_ID equals EU.ID
                                join ED in objContext.SPG_DOCUMENT on TD.TEMPLATE_ID equals ED.TEMPLATEID into EDD
                                from ED in EDD.DefaultIfEmpty()
                                where TD.FILE_NAME.Equals(FileName.ToString())
                                && PC.USER_ID == userId[1]
                                select new FileModel
                                {
                                    TemplateID = TD.TEMPLATE_ID,
                                    FileName = TD.FILE_NAME,
                                    ConFigId = PC.CONFIG_ID,
                                    FileCode = TD.FILE_CODE,
                                    PERIOD_TO = TD.PERIOD_TO,
                                    PERIOD_FROM = TD.PERIOD_FROM,
                                    SUBJECTAREA_NAME = SA.SUBJECTAREA_NAME,
                                    //RoleID = Convert.ToDecimal(EU.ROLE_ID),

                                    DocumentID = ED.ID,
                                    DocumentFileNAME = ED.FILE_NAME,
                                    DOCTEMPLATEID = ED.TEMPLATEID,
                                    LockDate = TD.LOCK_DATE,
                                    UploderUserId = PC.USER_ID,
                                    //UserName = EU.NAME,
                                    //UploderEmailId = EU.EMAILID,
                                    ApproverID = PC.APPROVER_ID,
                                    ApproverEmailID = PC.APPROVER_EMAILD,
                                    ApproverName = PC.APPROVER_NAME,
                                    OnTime = TD.ONTIMECODE,
                                    ForTime = TD.FORTIMECODE
                                }).FirstOrDefault();
                if (objFileModel != null)
                {
                    objUser = objContext.SPG_USERS.Where(a => a.USER_ID == userId[1]).FirstOrDefault();
                    //objFileModel.RoleID = objUser.ROLE_ID;
                    objFileModel.UserName = objUser.Name;
                    objFileModel.UploderEmailId = objUser.EMAIL_ID;
                }
            }

            return objFileModel;
        }
        public List<TemplateFile> SaveUploadedExcelFile(List<TemplateFile> listErros, string FilepathAndName, string FilePath, string userId, FileModel objFileModel, string approverUserId, string UploadedDocumentsFilePath, string WarningdocumentFilePath)
        {
            bool flag = false;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {

                //dt = ReadUploadedExcelFile(FilepathAndName);
                SPONGE_Context dbcontext = new SPONGE_Context();
                dt = GetUploadedExcelData(FilepathAndName);
                if (dt.Rows.Count == 0)
                {
                    listErros.Add(new TemplateFile { FileName = objFileModel.FileName, ErrorType = "F", ErrorMessage = "Error!No records found in excel template!" });
                    return listErros;
                }
                else if (dt.Rows.Count <= 5)
                {
                    listErros.Add(new TemplateFile { FileName = objFileModel.FileName, ErrorType = "F", ErrorMessage = "Error!No records found in excel template!" });
                    return listErros;
                }
                //delete 1st rows from DT
                int countOfRowsToBeDeleted = Convert.ToInt32(_configuration["AppSettings:RemoveRowsfromExcel"]);
                dt = dt.AsEnumerable().Skip(countOfRowsToBeDeleted).CopyToDataTable<DataRow>();

                foreach (DataColumn column in dt.Columns)
                {
                    string cName = dt.Rows[0][column.ColumnName].ToString();

                    // if (!dt.Columns.Contains(cName) && cName != "")
                    if (cName != "")
                    {
                        column.ColumnName = cName;
                    }

                }

                dt.Rows[0].Delete();
                dt.AcceptChanges();
                //Get Measure column Drop Down Value
                string StrMeasureDropDownCol = string.Empty;
                try
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        string colName = column.ColumnName.ToString();
                        var ColumnDisplayName = dbcontext.SPG_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == objFileModel.ConFigId && w.DISPLAY_TYPE == "DROPDOWN" && w.DISPLAY_NAME == colName).Select(s => new { DisplayName = s.DISPLAY_NAME }).ToList();
                        if (ColumnDisplayName.Count > 0)
                        {
                            foreach (var it in ColumnDisplayName)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    StrMeasureDropDownCol = row[it.DisplayName].ToString().Trim();
                                    if (!string.IsNullOrEmpty(StrMeasureDropDownCol))
                                        row[it.DisplayName] = GeLookUpValue(objFileModel.ConFigId, StrMeasureDropDownCol);
                                }

                            }
                        }
                    }

                    //Remove blank rows from data table
                    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                    {

                        if (dt.Rows[i][0].ToString() == "" && dt.Rows[i][1].ToString() == "")
                            dt.Rows[i].Delete();
                    }
                    dt.AcceptChanges();


                }
                catch (Exception ex)
                {
                    ErrorLog srsEx = new();
                    srsEx.LogErrorInTextFile(ex);
                    SentErrorMail.SentEmailtoError("Error on upload Excel File on \n \n InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
                }
                //dt.AcceptChanges();

                if (dt.Rows.Count == 0)
                {
                    listErros.Add(new TemplateFile { FileName = objFileModel.FileName, ErrorType = "F", ErrorMessage = "Error!No records Found in excel template!" });
                    return listErros;
                }
                List<String> lsColumnsExcel = new List<string>();
                List<string> lsColumnsDB = new List<string>();
                List<SelectListItem> listOfDBColumns = new List<SelectListItem>();
                List<string> listOfColumnnames = new List<string>();
                List<SelectListItem> listOfInsertColumnnames = new List<SelectListItem>();
                foreach (DataColumn column in dt.Columns)
                {
                    lsColumnsExcel.Add(column.ColumnName);
                }

                //Check   column GROUPCOLUMNNAME value 
                bool IsGROUPCOLUMNNAMEExist = false;
                var GetColumnNames = dbcontext.SPG_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == objFileModel.ConFigId && w.COLLECTION_TYPE == "Measure" && w.GROUPCOLUMNNAME != null).Select(s => new { DisplayName = s.DISPLAY_NAME, DataType = s.DATA_TYPE, ConfigUserId = s.CONFIGUSER_ID }).ToList();
                if (GetColumnNames.Count > 0)
                    IsGROUPCOLUMNNAMEExist = true;
                if (IsGROUPCOLUMNNAMEExist)
                {
                    List<SelectListItem> listDisplayNames = new List<SelectListItem>();
                    List<SelectListItem> listFinalDisplayNames = new List<SelectListItem>();
                    List<SelectListItem> List1 = new List<SelectListItem>();
                    List<SelectListItem> List2 = new List<SelectListItem>();
                    List<SelectListItem> List22 = new List<SelectListItem>();
                    List<SelectListItem> List3 = new List<SelectListItem>();
                    List1 = dbcontext.SPG_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == objFileModel.ConFigId && w.COLLECTION_TYPE == "Master" && string.IsNullOrEmpty(w.GROUPCOLUMNNAME)).Select(s => new SelectListItem { Text = s.DISPLAY_NAME, Value = s.DATA_TYPE }).ToList();
                    List2 = dbcontext.SPG_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == objFileModel.ConFigId && w.COLLECTION_TYPE == "Measure" && w.GROUPCOLUMNNAME != null && w.IS_SHOW == "Y").Select(s => new SelectListItem { Text = s.DISPLAY_NAME, Value = s.DATA_TYPE }).ToList();
                    List22 = dbcontext.SPG_GETTIMECODE.Where(w => w.CONFIG_ID == objFileModel.ConFigId && w.TEMPLATE_ID == objFileModel.TemplateID).Select(s => new SelectListItem { Text = s.DISPLAY_NAME, Value = s.DATA_TYPE }).ToList();
                    List3 = List1.Concat(List2).ToList();
                    foreach (var L in List2)
                    {
                        foreach (var K in List22)
                        {
                            if (L.Value == K.Value)
                            {
                                listDisplayNames.Add(new SelectListItem { Text = K.Text, Value = K.Value });
                                break;
                            }
                        }
                    }
                    listFinalDisplayNames = List1.Concat(listDisplayNames).ToList();
                    //Compare columns with data table columns
                    foreach (var i in lsColumnsExcel)
                    {
                        foreach (var j in listFinalDisplayNames)
                        {
                            if (i == j.Text)
                            {
                                listOfInsertColumnnames.Add(new SelectListItem { Text = j.Text, Value = j.Value });
                                break;
                            }
                        }
                    }
                }
                else
                {
                    string Dynamic_SP_name = "SP_GET_DYNAMIC_QUERY_RESULTS";
                    string dynamicsqlQuery = "SELECT DATA_TYPE,CASE WHEN ISNULL(GROUPCOLUMNNAME, '' )!='' THEN GROUPCOLUMNNAME + '-' + Display_Name    ELSE Display_Name END as Display_Name FROM  SPG_CONFIG_STRUCTURE where config_id = " + objFileModel.ConFigId + "";
                    DataSet dsResults = new DataSet();
                    using (GetDataSet objGetDataSetValue = new GetDataSet())
                    {
                        dsResults = objGetDataSetValue.GetDataSetValueStringParam(Dynamic_SP_name, dynamicsqlQuery);
                    }
                    foreach (DataRow row in dsResults.Tables[0].Rows)
                    {
                        listOfDBColumns.Add(new SelectListItem() { Text = row["Display_Name"].ToString(), Value = row["DATA_TYPE"].ToString() });
                    }
                    foreach (var i in lsColumnsExcel)
                    {
                        foreach (var j in listOfDBColumns)
                        {
                            if (i.ToUpper() == j.Text.ToUpper())
                            {
                                listOfInsertColumnnames.Add(new SelectListItem { Text = j.Text, Value = j.Value });
                                break;
                            }
                        }
                    }
                }
                if (listOfInsertColumnnames.Count > 0)
                {

                    //Update Status to Abandon as  4 for previous document for same template
                    var pendingStatusId = (int)Helper.ApprovalStatusEnum.Pending;
                    var document = dbcontext.SPG_DOCUMENT.Where(s => s.TEMPLATEID == objFileModel.TemplateID && (s.APPROVALSTATUSID == pendingStatusId)).ToList();
                    if (document.Count > 0)
                    {
                        int statusAbandonId = (int)Helper.ApprovalStatusEnum.Abandon;
                        foreach (var item in document)
                        {
                            item.APPROVALSTATUSID = statusAbandonId;
                            dbcontext.Entry(item);
                        }
                    }
                    dbcontext.SaveChanges();
                    //End



                    string DocumentId = InsertIntoSPG_Document(objFileModel.UploderUserId,objFileModel.TemplateID, objFileModel.FileName, objFileModel.ApproverName, UploadedDocumentsFilePath, objFileModel.LockDate, objFileModel.ApproverID);
                    string datasetResults = "";

                    // Remove Empty Rows
                    dt = dt.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field =>
                            field is System.DBNull || string.Compare((field as string).Trim(),
                            string.Empty) == 0)).CopyToDataTable();

                    ////Remove Grand Total row
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i][0].ToString().Contains("Grand Total"))
                        {
                            dt.Rows[i].Delete();
                            dt.AcceptChanges();
                        }
                    }

                    int CountMaster = dbcontext.SPG_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == objFileModel.ConFigId && w.COLLECTION_TYPE == "Master" && w.IS_SHOW == "Y").Count();
                    int CountMeasure = dbcontext.SPG_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == objFileModel.ConFigId && w.COLLECTION_TYPE == "Measure").Count();
                    CountMaster = CountMaster - 1;
                    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                    {
                        int show = 0;
                        for (int k = 1; k <= CountMeasure; k++)
                        {
                            if (dt.Rows[i][CountMaster + k].ToString() != "")
                            {
                                show++;
                            }
                        }
                        if (show == 0)
                        {
                            dt.Rows[i].Delete();
                        }

                    }
                    dt.AcceptChanges();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        StringBuilder StrInsertQuery = null;
                        StrInsertQuery = new StringBuilder();
                        string FormedQueryLookupType = "SP_INSERT_DIMENSIONDETAILS";
                        StrInsertQuery.Append("INSERT INTO SPG_CONFIGDATA(");
                        listOfColumnnames = listOfInsertColumnnames.Select(x => x.Value).ToList();
                        string columnnames = string.Join(",", listOfColumnnames.ToArray());
                        string nullvalue = "NULL";
                        StrInsertQuery.Append(columnnames + ",MODIFIED_BY,CONFIG_ID,TEMPLATE_ID,DOCUMENT_ID)values(");
                        for (int j = 0; j < listOfColumnnames.Count; j++)
                        {
                            if (listOfColumnnames[j].Contains("N"))
                            {
                                StrInsertQuery.Append(!string.IsNullOrEmpty(dt.Rows[i].ItemArray[j].ToString()) ? dt.Rows[i].ItemArray[j] + "," : nullvalue + ",");
                            }
                            if (listOfColumnnames[j].Contains("VC"))
                            {

                                StrInsertQuery.Append(!string.IsNullOrEmpty(dt.Rows[i].ItemArray[j].ToString().Replace("'", "''").Replace("&", "'+'&'+'")) ? "'" + dt.Rows[i].ItemArray[j].ToString().Replace("'", "''").Replace("&", "'+'&'+'") + "'," : nullvalue + ",");
                            }
                            if (listOfColumnnames[j].Contains("DT"))
                            {
                                if (!string.IsNullOrEmpty(dt.Rows[i].ItemArray[j].ToString()))
                                    StrInsertQuery.Append("'" + Convert.ToDateTime(dt.Rows[i].ItemArray[j]).ToString("dd-MMM-yy") + "',");
                                else
                                    StrInsertQuery.Append(nullvalue + ",");
                            }
                            if (listOfColumnnames[j].Contains("P"))
                            {
                                StrInsertQuery.Append(!string.IsNullOrEmpty(dt.Rows[i].ItemArray[j].ToString()) ? dt.Rows[i].ItemArray[j] + "," : nullvalue + ",");
                            }
                        }
                        StrInsertQuery.Append("'" + userId[1] + "'," + objFileModel.ConFigId + "," + objFileModel.TemplateID + ",'" + DocumentId + "')");
                        using (GetDataSet objGetDataSetValue = new GetDataSet())
                        {
                            datasetResults = objGetDataSetValue.InsertDataSetValueStringParam(FormedQueryLookupType, StrInsertQuery.ToString());
                        }
                        if (datasetResults.Equals("Success"))
                        {
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                            // break;
                            listErros.Add(new TemplateFile { FileName = objFileModel.FileName, ErrorType = "F", ErrorMessage = "Error while inserting data in DB ! Please contact to Admin;" });
                            SentErrorMail.SentEmailtoError("DB Error Details: " + datasetResults + "   dynamic DB script: " + StrInsertQuery.ToString());

                            if (!string.IsNullOrEmpty(DocumentId))
                            {
                                string dynamicQuery = "DELETE FROM spg_document WHERE ID='" + DocumentId + "'";
                                string SpName = "SP_DELETE_DETAILS";
                                GetDataSet objDeleteSetValue = new GetDataSet();
                                objDeleteSetValue.DeleteUsingDynamicQuery(SpName, dynamicQuery.ToString());
                            }
                            return listErros;
                        }
                    }
                    if (!String.IsNullOrEmpty(DocumentId) && flag != true)
                    {
                        if (System.IO.File.Exists(FilepathAndName))
                        {
                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilepathAndName);
                            FileInfo.CopyTo(UploadedDocumentsFilePath + "\\" + DocumentId + ".xlsx", true);
                            FileInfo.Delete();
                            //UpdateModel LATEST_FLAG_FOR_DAY
                            var Documennt = dbcontext.SPG_DOCUMENT.Where(m => m.ID == DocumentId).SingleOrDefault();
                            if (Documennt != null)
                            {
                                if (Documennt.APPROVALSTATUSID == 1)
                                {
                                    Documennt.LATEST_FLAG_FOR_DAY = 0;
                                }
                                else if (Documennt.APPROVALSTATUSID == 2)
                                {
                                    Documennt.LATEST_FLAG_FOR_DAY = 1;

                                }
                                dbcontext.SaveChanges();

                                ////End
                                listErros.Add(new TemplateFile { FileName = objFileModel.FileName, ErrorType = "S", ErrorMessage = "Successfully Uploaded!" });
                                return listErros;
                            }

                        }
                        else
                        {
                            listErros.Add(new TemplateFile { FileName = objFileModel.FileName, ErrorType = "F", ErrorMessage = "Error! Filepath and name(" + FilepathAndName + ") does not exist" });
                            return listErros;
                        }
                    }
                    else
                    {
                        listErros.Add(new TemplateFile { FileName = objFileModel.FileName, ErrorType = "F", ErrorMessage = "Error! DocumentId " + DocumentId + " is not generated and flag is " + flag + "; " });
                        return listErros;
                    }
                }
            }
            catch (Exception ex)

            {
                listErros.Add(new TemplateFile { FileName = objFileModel.FileName, ErrorType = "F", ErrorMessage = ex.Message });
                return listErros;
            }
            return listErros;
        }
        public string InsertIntoSPG_Document(string uploadedby,decimal TemplateID, string FILE_NAME, string ApproverName, string FILE_PATH, DateTime? LockDate, string approverUserId)
        {
            // Check lock date.If lock date <= current date value is 1
            decimal? ApprovalStatusID = 1;
            string UserRole = HttpContext.Session.GetString("ROLE").ToString();
            if (LockDate != null && (UserRole.ToUpper() == "ADMIN" || HttpContext.Session.GetString("NAME").ToString().ToUpper() == approverUserId))
            {
                ApprovalStatusID = 2;
            }
            else if (LockDate != null)
            {
                if (LockDate <= DateTime.Now.Date)
                    ApprovalStatusID = 1;
                else if (LockDate > DateTime.Now.Date)
                    ApprovalStatusID = 2;
                else
                    ApprovalStatusID = 1;
            }
            //Rename Fine Name

            int idx = FILE_NAME.LastIndexOf('_');

            if (idx != -1)
            {
                FILE_NAME = FILE_NAME.Substring(0, idx) + "_[" + DateTime.Now.Date.ToString("yyyyMMdd") + "].txt";

            }
            //End

            SPONGE_Context dbContext = new ();
            if (ApprovalStatusID == 2)
            {
                ////If same template uploaded again,LATEST_FLAG_FOR_DAY will be as 0 for previous template which have LATEST_FLAG_FOR_DAY as 1  
                var documentresult = dbContext.SPG_DOCUMENT.Where(s => s.TEMPLATEID == TemplateID && (s.APPROVALSTATUSID == ApprovalStatusID) && (s.LATEST_FLAG_FOR_DAY == 1)).ToList();
                if (documentresult != null)
                {
                    foreach (var item in documentresult)
                    {
                        item.LATEST_FLAG_FOR_DAY = 0;
                        dbContext.Entry(item);
                    }
                }
            }
            SPG_DOCUMENT saveepdocument = new SPG_DOCUMENT()
            {
                UPLOADDATE = DateTime.Now,
                UPLOADEDBY = uploadedby,
                APPROVALSTATUSID = (int?)ApprovalStatusID,
                TEMPLATEID = (int?)TemplateID,
                FILE_NAME = FILE_NAME,
                FILE_PATH = FILE_PATH,
                APPROVERID = approverUserId,
                APPROVER_NAME = ApproverName,
                ID = Guid.NewGuid().ToString()

            };
            dbContext.SPG_DOCUMENT.Add(saveepdocument);
            dbContext.SaveChanges();
            string DocumentId = saveepdocument.ID;
            return DocumentId;
        }
        public DataTable GetUploadedExcelData(string pathToExcelFile)
        {
            var table = new DataTable();
            var fi = new FileInfo(pathToExcelFile);

            try
            {
                using (var package = new ExcelPackage(fi))
                {
                    var sheet = package.Workbook.Worksheets["DataCollectionSheet"];
                    if (sheet == null) return null;

                    for (int col = 1; col <= sheet.Dimension.Columns; col++)
                    {
                        table.Columns.Add("Col" + col);
                    }

                    for (int row = 1; row <= sheet.Dimension.Rows; row++)
                    {
                        var newRow = table.NewRow();
                        for (int col = 1; col <= sheet.Dimension.Columns; col++)
                        {
                            newRow[col - 1] = sheet.Cells[row, col].Value;
                        }
                        table.Rows.Add(newRow);
                    }
                }

                return table;
            }
            catch (Exception ex)
            {
                ErrorLog srsEx = new();
                srsEx.LogErrorInTextFile(ex);
                SentErrorMail.SentEmailtoError("Error on upload Excel File When Reading Excel file"
                    + pathToExcelFile
                    + " \n \n InnerException: " + ex.InnerException?.ToString()
                    + " StackTrace: " + ex.StackTrace.ToString()
                    + " Message" + ex.Message);
                return null;
            }
        }
        public string GeLookUpValue(decimal? ConFigId, string LookUp_DESC)
        {
            string LookVal = "";
            string LinkServerName = _configuration["LinkServerName"];
            string LINK_DWD = _configuration["LINK_DWD"];

            string Dynamic_SP_name = "SP_GET_DYNAMIC_QUERY_RESULTS";
            string dynamicsqlQuery = "Select distinct DATA_TYPE,  LOOKUP_VALUE,LOOKUP_DESC,Display_Name from " + LinkServerName + "DIM_LOOKUP " + LINK_DWD + " p, SPG_CONFIG_STRUCTURE C " +
"WHERE c.config_id ='" + ConFigId + "' and C.LOOKUP_TYPE = p.LOOKUP_TYPE and c.DISPLAY_TYPE = 'DROPDOWN' and LookUp_DESC = '" + LookUp_DESC + "' order by DATA_TYPE asc";
            DataSet dsResults = new DataSet();
            using (GetDataSet objGetDataSetValue = new GetDataSet())
            {
                dsResults = objGetDataSetValue.GetDataSetValueStringParam(Dynamic_SP_name, dynamicsqlQuery);
            }

            if (dsResults != null)
            {
                try
                {
                    LookVal = dsResults.Tables[0].Rows[0][1].ToString();

                }
                catch (Exception ex)
                {
                    ErrorLog srsEx = new();
                    srsEx.LogErrorInTextFile(ex);
                    SentErrorMail.SentEmailtoError("Error on upload Excel File on look Up Measure Column \n \n InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
                }

            }

            else if (dsResults.Tables.Count != 0)
            {
                try
                {
                    LookVal = dsResults.Tables[0].Rows[0][1].ToString();
                }
                catch (Exception ex)
                {

                    ErrorLog srsEx = new();
                    srsEx.LogErrorInTextFile(ex);
                    SentErrorMail.SentEmailtoError("Error on upload Excel File on look Up Measure Column \n \n InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
                }
            }
            else if (dsResults.Tables[0].Rows.Count != 0)
            {
                try
                {
                    LookVal = dsResults.Tables[0].Rows[0][1].ToString();
                }
                catch (Exception ex)
                {
                    ErrorLog srsEx = new();
                    srsEx.LogErrorInTextFile(ex);
                    SentErrorMail.SentEmailtoError("Error on upload Excel File on look Up Measure Column \n \n InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
                }
            }

            return LookVal;
        }

        public JsonResult LoadWarningMessageFile(string FileName, string ErrorType)
        {
            SPONGE_Context dbcontext = new();            
            string UploadedDocumentsFilePath = _configuration["AppSettings:UploadedDocumentsFilePath"];
            string WarningDocumentFilePath = _configuration["AppSettings:WarningDocumentFilePath"];
            string msgerror = "";
            string UserRole = HttpContext.Session.GetString("ROLE").ToString();
            string UserName = HttpContext.Session.GetString("NAME").ToString();
            string[] userId = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            int DataApproverRoleId = Convert.ToInt16(_configuration["AppSettings:DataApproverRoleId"]);
            string approverUserId = string.Empty;
            string FilePath = WarningDocumentFilePath + "\\" + FileName;
            List<TemplateFile> listErros = new List<TemplateFile>();
            
            try
            {

                if (!Directory.Exists(WarningDocumentFilePath))
                    Directory.CreateDirectory(WarningDocumentFilePath);
                if (System.IO.File.Exists(FilePath))
                {
                    FileModel objFileModel = new FileModel();
                    ApproverModel objApproverModel = new ApproverModel();
                    objFileModel = Checkauthorizeduser(FileName, userId, objFileModel, false);// //Check authorized user

                    if (UserRole.ToUpper() == "ADMIN")
                    {
                        objFileModel = Checkauthorizeduser(FileName, userId, objFileModel, true);// //Check authorized user
                        try
                        {
                            if (objFileModel != null)
                            {
                                approverUserId = userId[1];
                                listErros = SaveUploadedExcelFile(listErros, WarningDocumentFilePath + "\\" + FileName, WarningDocumentFilePath, userId[1], objFileModel, approverUserId, UploadedDocumentsFilePath, WarningDocumentFilePath);                               
                                if (listErros[0].ErrorType == "S")
                                    SentMailToUploader(objFileModel);
                                return Json(new { msgerror = listErros });
                            }

                        }
                        catch (Exception ex)
                        {
                            ErrorLog srsEx = new();
                            srsEx.LogErrorInTextFile(ex);
                            listErros.Add(new TemplateFile { FileName = FileName, ErrorType = "F", ErrorMessage = ex.Message });
                        }
                    }
                    else if (UserRole.ToUpper() == "DATA APPROVER")
                    {
                        objFileModel = Checkauthorizeduser(FileName, userId, objFileModel, true);// //Check authorized user
                                                                                                 // decimal? SubfunctionId = ValidateAdminDetails(fileName);
                        if (objFileModel != null)
                        {

                            approverUserId = objFileModel.ApproverID;
                            try
                            {
                                listErros = SaveUploadedExcelFile(listErros, WarningDocumentFilePath + "\\" + FileName, WarningDocumentFilePath, userId[1], objFileModel, approverUserId, UploadedDocumentsFilePath, WarningDocumentFilePath);
                                if (ErrorType.Equals("WA") && listErros[0].ErrorType == "S")
                                    SentMailToUploaderAndApprover(objFileModel, "Approver");
                                else if (ErrorType.Equals("W") && listErros[0].ErrorType == "S")
                                    SentMailToUploader(objFileModel);
                                return Json(new { msgerror = listErros });
                            }
                            catch (Exception ex)
                            {
                                ErrorLog srsEx = new();
                                srsEx.LogErrorInTextFile(ex);
                                listErros.Add(new TemplateFile { FileName = FileName, ErrorType = "F", ErrorMessage = ex.Message });
                            }

                        }
                    }

                    else
                    {
                        try
                        {
                            if (objFileModel != null)
                            {
                                approverUserId = objFileModel.ApproverID;
                                listErros = SaveUploadedExcelFile(listErros, WarningDocumentFilePath + "\\" + FileName, WarningDocumentFilePath, userId[1], objFileModel, approverUserId, UploadedDocumentsFilePath, WarningDocumentFilePath);
                                if (ErrorType.Equals("WA") && listErros[0].ErrorType == "S")
                                    SentMailToUploaderAndApprover(objFileModel, "Approver");
                                if (ErrorType.Equals("W") && listErros[0].ErrorType == "S")
                                    SentMailToUploader(objFileModel);
                                return Json(new { msgerror = listErros });
                            }

                        }
                        catch (Exception ex)
                        {
                            ErrorLog srsEx = new();
                            srsEx.LogErrorInTextFile(ex);
                            listErros.Add(new TemplateFile { FileName = FileName, ErrorType = "F", ErrorMessage = ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog srsEx = new();
                srsEx.LogErrorInTextFile(ex);
                listErros.Add(new TemplateFile { FileName = FileName, ErrorType = "F", ErrorMessage = ex.Message });
            }
            return Json(new { msgerror = listErros });
        }
        public void SentMailToUploader(FileModel objFileModel)
        {
            NameValueCollection mailBodyplaceHolders = new NameValueCollection
            {
                { "<UserName>", objFileModel.UserName },
                { "<FileName>", objFileModel.FileName.Replace(".xlsx", "") }
            };
            string DataCollectionSubject = "[Sponge] - Excel template " + objFileModel.FileName.Replace(".xlsx", "") + " Uploaded";
            string mailbody = "";
            string messageTemplatePath = _settings.Value.EmailTemplatePathForUploader;

            mailbody = _email.GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
            _email.SendMail("", DataCollectionSubject, mailbody, objFileModel.UploderEmailId);
        }
        public void SentMailToUploaderAndApprover(FileModel objFileModel, String Approver)
        {
            if (!string.IsNullOrEmpty(objFileModel.UploderUserId))
            {
                NameValueCollection mailBodyplaceHolders = new NameValueCollection
                {
                    { "<UserName>", objFileModel.UserName },
                    { "<FileName>", objFileModel.FileName.Replace(".xlsx", "") }
                };
                string DataCollectionSubject = "[Sponge] - Excel template " + objFileModel.FileName.Replace(".xlsx", "") + " Uploaded";
                string mailbody = "";
                string messageTemplatePath = _settings.Value.ApprovalmailToUploader;

                mailbody = _email.GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                _email.SendMail("", DataCollectionSubject, mailbody, objFileModel.UploderEmailId);
            }

            if (!string.IsNullOrEmpty(objFileModel.ApproverEmailID))
            {
                NameValueCollection mailBodyplaceHolders = new NameValueCollection
                {
                    { "<UploderName>", objFileModel.UserName },
                    { "<UserName>", objFileModel.ApproverName }
                };
                var DWName = GetDimensionNameForEmailTemplate(objFileModel.ConFigId).ToList();
                string DimensionName = "";
                foreach (var items in DWName)
                {
                    DimensionName += items.ToString().Replace(",", "") + "\n";

                }
                mailBodyplaceHolders.Add("<Custom>", DimensionName.ToString().Replace(",", ""));

                mailBodyplaceHolders.Add("<ForTime>", Convert.ToDateTime(objFileModel.PERIOD_FROM).ToString("MMM/dd/yyyy"));
                mailBodyplaceHolders.Add("<OnTime>", Convert.ToDateTime(objFileModel.PERIOD_TO).ToString("MMM/dd/yyyy"));
                mailBodyplaceHolders.Add("<LockDate>", Convert.ToDateTime(objFileModel.LockDate).ToString("MMM/dd/yyyy"));
                mailBodyplaceHolders.Add("<UploadDate>", DateTime.Now.Date.ToString("MMM/dd/yyyy")); 
                string DataCollectionSubject = "[Sponge] - Document approval request for " + objFileModel.FileName.Replace(".xlsx", "") + "";
                string mailbody = "";
                string messageTemplatePath = _settings.Value.MailToApprover;
                mailbody = _email.GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                _email.SendMail("", DataCollectionSubject, mailbody, objFileModel.ApproverEmailID);
            }
        }

        public string ValidateHiddenSheet(string fileName, SPONGE_Context objContext, string FileCode)
        {
            string fileCode = string.Empty;
            FileStream file = null;
            file = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet hiddenSheet = package.Workbook.Worksheets[HiddenSheetName];
                if (hiddenSheet == null)
                {
                    fileCode = string.Empty;
                }
                else
                {
                    fileCode = Convert.ToString(hiddenSheet.Cells[HiddenFileCodeRowIndex, HiddenFileCodeColIndex].Value);
                    //var IsExistFileCode= 
                    if (fileCode != FileCode)
                    {
                        fileCode = string.Empty;
                    }
                }
            }
            file.Close();
            return fileCode;
        }

        public List<TemplateFile> ValidateFileupload(List<TemplateFile> listErros, FileModel objFileModel, string Filename, string FilePath, string userId, string approverUserId)
        {
            SPONGE_Context objContext = new();
            string WarningdocumentFilePath = _configuration["AppSettings:WarningDocumentFilePath"];
            string UserRole = HttpContext.Session.GetString("ROLE").ToString();
            var FileCode = ValidateHiddenSheet(FilePath + "\\" + Filename, objContext, objFileModel.FileCode);
            if (UserRole.ToUpper() == "ADMIN" || UserRole.ToUpper() == "DATA CONFIGURE")
            {
                if (string.IsNullOrEmpty(objFileModel.FileName))//Check valid file name 
                {
                    listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file(Invalid File name)! Upload the valid excel template which is sent to you;" });
                    if (System.IO.File.Exists(FilePath + "\\" + Filename))
                    {
                        System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                        FileInfo.Delete();
                    }
                    return listErros;
                }
                else if (string.IsNullOrEmpty(objFileModel.FileCode))//Check valid  file code
                {
                    listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file(Invalid File Code)! Upload the valid excel template which is sent to you;" });
                    if (System.IO.File.Exists(FilePath + "\\" + Filename))
                    {
                        System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                        FileInfo.Delete();
                    }
                    return listErros;
                }
                else if (!string.IsNullOrEmpty(objFileModel.DocumentID))//Check valid  file code
                {

                    if (objFileModel.LockDate <= DateTime.Now.Date)
                    {
                        listErros.Add(new TemplateFile
                        {
                            FileName = objFileModel.FileName,
                            ErrorType = "WA",
                            // ErrorMessage = "Period is locked for the upload and requires approval. Do you want to continue upload and send it for approval?"
                            ErrorMessage = "Period is locked for the upload. Do you want to continue with  auto approve?"

                        });
                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {
                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                            FileInfo.Delete();
                        }
                    }
                    else
                    {
                        listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "W", ErrorMessage = "File was uploaded earlier also for same reporting period. Do you wish to upload it again?" });
                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {
                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                            FileInfo.Delete();
                        }
                    }
                    return listErros;
                }
                else if (string.IsNullOrEmpty(FileCode))
                {
                    listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file(Invalid File Code)! Upload the valid excel template which is sent to you;" });
                    if (System.IO.File.Exists(FilePath + "\\" + Filename))
                    {
                        System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                        FileInfo.Delete();
                    }
                    return listErros;
                }
                else if (objFileModel.LockDate != null)
                {
                    if (objFileModel.LockDate <= DateTime.Now.Date)
                    {
                        listErros.Add(new TemplateFile
                        {
                            FileName = objFileModel.FileName,
                            ErrorType = "WA",
                            // ErrorMessage = "Period is locked for the upload and requires approval. Do you want to continue upload and send it for approval?"
                            ErrorMessage = "Period is locked for the upload. Do you want to continue with  auto approve?"

                        });
                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {
                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                            FileInfo.Delete();
                        }

                        return listErros;
                    }

                }
                else if (objFileModel.LockDate == null)
                {
                    listErros.Add(new TemplateFile
                    {
                        FileName = objFileModel.FileName,
                        ErrorType = "F",
                        ErrorMessage = "Invalid Lock Date!"

                    });
                    if (System.IO.File.Exists(FilePath + "\\" + Filename))
                    {
                        System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                        //FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                        FileInfo.Delete();
                    }
                    return listErros;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(objFileModel.FileName))//Check valid file name 
                {
                    listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file(Invalid File name)! Upload the valid excel template which is sent to you;" });
                    if (System.IO.File.Exists(FilePath + "\\" + Filename))
                    {
                        System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                        FileInfo.Delete();
                    }
                    return listErros;
                }
                else if (string.IsNullOrEmpty(objFileModel.FileCode))//Check valid  file code
                {
                    listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file(Invalid File Code)! Upload the valid excel template which is sent to you;" });
                    if (System.IO.File.Exists(FilePath + "\\" + Filename))
                    {
                        System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                        FileInfo.Delete();
                    }
                    return listErros;
                }
                else if (objFileModel.UploderUserId != userId)//Check valid file name 
                {
                    if (objFileModel.ApproverID != userId)
                    {
                        listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "You are not authorized user! Please upload the valid excel template which is sent to you;" });
                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {
                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.Delete();
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(objFileModel.DocumentID))//Check valid  file code
                        {

                            if (objFileModel.LockDate <= DateTime.Now.Date && userId != approverUserId)
                            {
                                listErros.Add(new TemplateFile
                                {
                                    FileName = objFileModel.FileName,
                                    ErrorType = "WA",
                                    // ErrorMessage = "Period is locked for the upload. Do you want to continue with  auto approve?"

                                    ErrorMessage = "Period is locked for the upload and requires approval. Do you want to continue upload and send it for approval?"

                                });
                                if (System.IO.File.Exists(FilePath + "\\" + Filename))
                                {
                                    System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                                    FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                                    FileInfo.Delete();
                                }
                            }
                            else if (objFileModel.LockDate <= DateTime.Now.Date && userId == approverUserId)
                            {
                                listErros.Add(new TemplateFile
                                {
                                    FileName = objFileModel.FileName,
                                    ErrorType = "W",
                                    ErrorMessage = "Period is locked for the upload. Do you want to continue with auto approve?"

                                });
                                //Copy and Rename File name ,if message type  is warning

                                if (System.IO.File.Exists(FilePath + "\\" + Filename))
                                {

                                    System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                                    FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                                    FileInfo.Delete();
                                }
                                return listErros;
                            }
                            else
                            {
                                listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "W", ErrorMessage = "File was uploaded earlier also for same reporting period. Do you wish to upload it again?" });
                                if (System.IO.File.Exists(FilePath + "\\" + Filename))
                                {
                                    System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                                    FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                                    FileInfo.Delete();
                                }
                            }
                            return listErros;
                        }
                    }
                    return listErros;
                }
                else if (!string.IsNullOrEmpty(objFileModel.DocumentID))//Check valid  file code
                {

                    if (objFileModel.LockDate <= DateTime.Now.Date && userId != approverUserId)
                    {
                        listErros.Add(new TemplateFile
                        {
                            FileName = objFileModel.FileName,
                            ErrorType = "WA",
                            // ErrorMessage = "Period is locked for the upload. Do you want to continue with  auto approve?"

                            ErrorMessage = "Period is locked for the upload and requires approval. Do you want to continue upload and send it for approval?"

                        });
                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {
                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                            FileInfo.Delete();
                        }
                    }
                    else if (objFileModel.LockDate <= DateTime.Now.Date && userId == approverUserId)
                    {
                        listErros.Add(new TemplateFile
                        {
                            FileName = objFileModel.FileName,
                            ErrorType = "W",
                            ErrorMessage = "Period is locked for the upload. Do you want to continue with auto approve?"

                        });
                        //Copy and Rename File name ,if message type  is warning

                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {

                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                            FileInfo.Delete();
                        }
                        return listErros;
                    }
                    else
                    {
                        listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "W", ErrorMessage = "File was uploaded earlier also for same reporting period. Do you wish to upload it again?" });
                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {
                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                            FileInfo.Delete();
                        }
                    }
                    return listErros;
                }
                else if (string.IsNullOrEmpty(FileCode))
                {
                    listErros.Add(new TemplateFile { FileName = Filename, ErrorType = "F", ErrorMessage = "Error in file(Invalid File Code)! Upload the valid excel template which is sent to you;" });
                    if (System.IO.File.Exists(FilePath + "\\" + Filename))
                    {
                        System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                        FileInfo.Delete();
                    }
                    return listErros;
                }
                else if (objFileModel.LockDate != null)
                {
                    if (objFileModel.LockDate <= DateTime.Now.Date && userId != approverUserId)
                    {
                        listErros.Add(new TemplateFile
                        {
                            FileName = objFileModel.FileName,
                            ErrorType = "WA",
                            // ErrorMessage = "Period is locked for the upload. Do you want to continue with  auto approve?"

                            ErrorMessage = "Period is locked for the upload and requires approval. Do you want to continue upload and send it for approval?"

                        });
                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {
                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                            FileInfo.Delete();
                        }

                        return listErros;
                    }
                    else if (objFileModel.LockDate <= DateTime.Now.Date && userId == approverUserId)
                    {
                        listErros.Add(new TemplateFile
                        {
                            FileName = objFileModel.FileName,
                            ErrorType = "W",
                            ErrorMessage = "Period is locked for the upload. Do you want to continue with auto approve?"

                        });
                        //Copy and Rename File name ,if message type  is warning

                        if (System.IO.File.Exists(FilePath + "\\" + Filename))
                        {

                            System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                            FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                            FileInfo.Delete();
                        }
                        return listErros;
                    }
                }
                else if (objFileModel.LockDate == null)
                {
                    listErros.Add(new TemplateFile
                    {
                        FileName = objFileModel.FileName,
                        ErrorType = "F",
                        ErrorMessage = "Invalid Lock Date!"

                    });
                    if (System.IO.File.Exists(FilePath + "\\" + Filename))
                    {
                        System.IO.FileInfo FileInfo = new System.IO.FileInfo(FilePath + "\\" + Filename);
                        //FileInfo.CopyTo(WarningdocumentFilePath + "\\" + Filename, true);
                        FileInfo.Delete();
                    }
                    return listErros;
                }
            }
            return listErros;
        }

        public void KillExcelProcess()
        {
            var excelProcesses = Process.GetProcessesByName("EXCEL");
            string[] currentUser = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            foreach (Process theprocess in excelProcesses)
            {
                foreach (var process in excelProcesses)
                {
                    if (process.MainWindowTitle.Contains(currentUser[1]))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception ex)
                        {
                            // Log error
                        }
                    }
                }
            }
        }

        public List<string> GetDimensionNameForEmailTemplate(decimal ConfigId)
        {
            SPONGE_Context _Context = new();
            List<string> _Displayname = new();
            var displayName = (from DM in _Context.SPG_SUBJECT_DIMENSION
                               join PC in _Context.SPG_CONFIG_STRUCTURE on DM.DIMENSION_TABLE equals PC.DIMENSION_TABLE
                               where PC.CONFIG_ID == ConfigId && PC.COLLECTION_TYPE.Equals("Master")
                               select new
                               {
                                   DIMENSION_NAME = DM.DIMENSION_TABLE,
                                   DISPLAY_NAME = DM.MPP_DIMENSION_NAME
                               }).ToList();
            foreach(var items in displayName)
            {
                _Displayname.Add(items.DIMENSION_NAME.ToString() + ":" + items.DISPLAY_NAME.ToString());
            }
            return _Displayname;
        }

        //public ApproverModel GetApproverDetails(decimal DataApproverRoleId, ApproverModel objApproverModel, string userid)
        //{
        //    PortalModelDev objContext = new PortalModelDev();
        //    objApproverModel = (from EP in objContext.EP_USERS
        //                        where
        //                                            //EP.ID.Equals(userid) &&
        //                                            EP.ROLE_ID == DataApproverRoleId
        //                        select new ApproverModel
        //                        {
        //                            AppRoleId = Convert.ToDecimal(EP.ROLE_ID),
        //                            AppMailid = EP.EMAILID,
        //                            AppName = EP.NAME,
        //                            AppUserId = EP.ID

        //                        }).FirstOrDefault();
        //    return objApproverModel;
        //}
        //public string ValidateHiddenSheetandFileCode(string fileName, string FileCode = null)
        //{

        //    string fileCode = string.Empty;
        //    FileStream file = null;
        //    file = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        //    using (ExcelPackage package = new ExcelPackage(file))
        //    {
        //        ExcelWorksheet hiddenSheet = package.Workbook.Worksheets[HiddenSheetName];
        //        if (hiddenSheet == null)
        //        {
        //            fileCode = string.Empty;

        //        }
        //        else
        //        {
        //            fileCode = Convert.ToString(hiddenSheet.Cells[HiddenFileCodeRowIndex, HiddenFileCodeColIndex].Value);

        //        }
        //    }
        //    file.Close();
        //    return fileCode;

        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
       
    }
}