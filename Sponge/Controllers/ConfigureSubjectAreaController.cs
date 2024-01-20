using DAL;
using DAL.Common;
using DAL.Models;
using LinqToExcel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Packaging;
using Sponge.Common;
using Sponge.Models;
using Sponge.ViewModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ConfigureSubjectAreaController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public ConfigureSubjectAreaController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult ConfigureSubjectArea()
        {
            SPONGE_Context spONGE_Context = new();
            var lst = spONGE_Context.SPG_SUBJECTAREA.Select(o => new { o.SUBJECTAREA_NAME, o.SUBJECTAREA_ID }).Distinct();
            ViewBag.SubjectArea = new SelectList(lst.ToList(), "SUBJECTAREA_NAME", "SUBJECTAREA_ID");
            var lstDimensions = spONGE_Context.SPG_MPP_MASTER.Select(o => new { o.MPP_DIMENSION_NAME, o.DIMENSION_TABLE }).Distinct();
            ViewBag.lstDimensions = new SelectList(lstDimensions.ToList(), "DIMENSION_TABLE", "MPP_DIMENSION_NAME");
            return View();
        }
        [HttpGet]
        public IActionResult SelectedDimension(int? subjectAreaId)
        {
            SPONGE_Context spONGE_Ctx = new();


            var dimensionlist = (from user in spONGE_Ctx.SPG_SUBJECT_DIMENSION
                                 where user.SUBJECTAREA_ID == subjectAreaId
                                 select new
                                 {
                                     DIMENSION_NAME = user.MPP_DIMENSION_NAME,
                                     DIMENSIONTABLENAME = user.DIMENSION_TABLE
                                 }).Distinct().ToList();
            if (dimensionlist.Count <= 0)
            {
                var lstDimensions = spONGE_Ctx.SPG_MPP_MASTER.Select(o => new { o.MPP_DIMENSION_NAME, o.DIMENSION_TABLE }).Distinct();
                ViewBag.lstDimensions = new SelectList(lstDimensions.ToList(), "DIMENSION_TABLE", "MPP_DIMENSION_NAME");
                //return View();
            }

            return Json(dimensionlist);
        }
        public IActionResult SaveMastersGroup(List<Dimension> dimensions, int? selectedSubjectArea)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            TempData["selectedSubjectArea"] = selectedSubjectArea;
            SPONGE_Context sPONGE_Context = new();
            // Code to store the subject area name to display in next subsequent views
            var subjectArea = sPONGE_Context.SPG_SUBJECTAREA.Find(selectedSubjectArea);
            string subjectAreaName = String.Empty;
            if (subjectArea != null)
            {
                subjectAreaName = subjectArea.SUBJECTAREA_NAME;
            }
            HttpContext.Session.SetString("SubjectAreaName", subjectAreaName); // To show subject area name on the next pages
            HttpContext.Session.SetString("Dimensions", JsonConvert.SerializeObject(dimensions)); // Storing dimensions in order to get them on the users page

            List<SPG_MPP_MASTER> spg_Masters = new List<SPG_MPP_MASTER>();

            foreach (var Dimension in dimensions)
            {
                if (!Dimension.IsSelected)
                {
                    //Code to save Data in SPG_SUBJECT_GENERIC_MASTER table
                    //for (int i = 1; i <= 3; i++)
                    //{
                    //    SPG_SUBJECT_GENERIC_MASTER sPG_SGM = new();
                    //    {
                    //        sPG_SGM.DIMENSION_TABLE = Dimension.Key;
                    //        sPG_SGM.MASTER_NAME = null;
                    //        switch (i)
                    //        {
                    //            case 1:
                    //                sPG_SGM.DISPLAY_NAME = Dimension.Value + "_Level";
                    //                sPG_SGM.FIELD_NAME = Dimension.Value + "_Level";
                    //                break;
                    //            case 2:
                    //                sPG_SGM.DISPLAY_NAME = Dimension.Value + "_Code";
                    //                sPG_SGM.FIELD_NAME = Dimension.Value + "_Code";
                    //                break;
                    //            case 3:
                    //                sPG_SGM.DISPLAY_NAME = Dimension.Value + "_Description";
                    //                sPG_SGM.FIELD_NAME = Dimension.Value + "_Description";
                    //                break;
                    //        }
                    //        sPG_SGM.SUBJECTAREA_ID = selectedSubjectArea;
                    //        sPG_SGM.IS_KEY = "Y";
                    //        sPG_SGM.IS_SHOW = "Y";

                    //    }
                    //    sPONGE_Context.SPG_SUBJECT_GENERIC_MASTER.Add(sPG_SGM);
                    //    sPONGE_Context.SaveChanges();
                    //}

                    SPG_SUBJECT_DIMENSION sPG_1 = new();
                    {
                        sPG_1.MPP_DIMENSION_NAME = Dimension.Value;
                        sPG_1.DIMENSION_TABLE = Dimension.Key;
                        sPG_1.SUBJECTAREA_ID = selectedSubjectArea;
                        sPG_1.ACTIVE_FLAG = "Y";
                        sPG_1.CREATED_DATE = DateTime.Now;
                        sPG_1.CREATED_BY = userName[1].ToString();
                    }
                    sPONGE_Context.SPG_SUBJECT_DIMENSION.Add(sPG_1);
                    sPONGE_Context.SaveChanges();
              }

                    List<SPG_MPP_MASTER> spg_mpp_master = (from user in sPONGE_Context.SPG_MPP_MASTER
                                                           where user.MPP_DIMENSION_NAME == Dimension.Value
                                                           select new SPG_MPP_MASTER
                                                           {
                                                               MASTER_NAME = user.MASTER_NAME,
                                                               MASTER_DISPLAY_NAME = user.MASTER_DISPLAY_NAME
                                                           }).Distinct().ToList();
                    spg_Masters.AddRange(spg_mpp_master);
                  
                

            }
            List<SPG_SUBJECT_MASTER> selectedMasters = new();
            foreach (var Master in dimensions)
            {
                var selectedMaster = sPONGE_Context.SPG_SUBJECT_MASTER
                    .Where(x => x.DIMENSION_TABLE == Master.Key && x.SUBJECTAREA_ID == selectedSubjectArea && x.IS_SHOW == "Y")
                    .Select(o => new SPG_SUBJECT_MASTER { MASTER_NAME = o.MASTER_NAME, FIELD_NAME = o.FIELD_NAME, DISPLAY_NAME = o.DISPLAY_NAME })
                        .Distinct().ToList();
                selectedMasters.AddRange(selectedMaster);

            }
              ViewBag.SPG_MASTER = new SelectList(spg_Masters.ToList(), "MASTER_NAME", "MASTER_DISPLAY_NAME");
            ViewBag.SelectedMaster = selectedMasters.ToList();

            ViewBag.SubjectAreaName = subjectAreaName;


            return View("Views\\ConfigureSubjectArea\\ConfigureMasters.cshtml");

        }
        public IActionResult GetFieldName(string? masterName)
        {

            SPONGE_Context sPONGE_Context = new();
            var fieldName = sPONGE_Context.SPG_MPP_MASTER.Where(o => o.MASTER_NAME == masterName && o.IS_KEY != "Y")
                .Select(o => new { o.COLUMN_DISPLAY_NAME, o.COLUMN_NAME }).Distinct();
            //ViewBag.FieldName = new SelectList(fieldName.ToList(), "COLUMN_NAME", "COLUMN_DISPLAY_NAME");
            return Json(fieldName);
        }

        [HttpPost]
        public IActionResult SaveMasters(List<SaveMaster> data)
        {

            SPONGE_Context sPONGE_Context = new();
            var selectedSubjectArea = TempData["selectedSubjectArea"] as int?;
            TempData.Keep();
            foreach (var master in data)
            {
                var dimensionData = sPONGE_Context.SPG_MPP_MASTER
                    .Where(x => x.MASTER_NAME == master.Master && x.IS_KEY == "Y")
                    .Select(x => new { x.DIMENSION_TABLE, x.COLUMN_NAME })
                    .ToList();

                foreach (var item in dimensionData)
                {
                    var newSubjectMasterEntity1 = new SPG_SUBJECT_MASTER
                    {
                        DIMENSION_TABLE = item.DIMENSION_TABLE,
                        FIELD_NAME = item.COLUMN_NAME,
                        SUBJECTAREA_ID = selectedSubjectArea,
                        IS_KEY = "Y",
                        IS_SHOW = "N",
                        DISPLAY_NAME = master.DisplayName + " Code",
                        MASTER_NAME = master.Master
                    };

                    sPONGE_Context.SPG_SUBJECT_MASTER.Add(newSubjectMasterEntity1);

                    var newSubjectMasterEntity2 = new SPG_SUBJECT_MASTER
                    {
                        DIMENSION_TABLE = item.DIMENSION_TABLE,
                        FIELD_NAME = master.FieldName,
                        SUBJECTAREA_ID = selectedSubjectArea,
                        IS_KEY = "N",
                        IS_SHOW = "Y",
                        DISPLAY_NAME = master.DisplayName,
                        MASTER_NAME = master.Master
                    };

                    sPONGE_Context.SPG_SUBJECT_MASTER.Add(newSubjectMasterEntity2);
                }
                sPONGE_Context.SaveChanges();
            }
            List<SPG_SUBJECT_DATACOLLECTION> selectedDataCollection = new();

            var selectedMaster = sPONGE_Context.SPG_SUBJECT_DATACOLLECTION
                .Where(x => x.SUBJECTAREA_ID == selectedSubjectArea)
                .Select(o => new SPG_SUBJECT_DATACOLLECTION
                {
                    DISPLAY_NAME = o.DISPLAY_NAME,
                    FIELD_NAME = o.FIELD_NAME,
                    IS_LOOKUP = o.IS_LOOKUP,
                    LOOKUP_TYPE = o.LOOKUP_TYPE,
                    DATA_TYPE = o.DATA_TYPE,
                    UOM = o.UOM
                })
                    .Distinct().ToList();
            selectedDataCollection.AddRange(selectedMaster);

            ViewBag.SelectedDataCollection = selectedDataCollection.ToList();

            var subjectAreaName = HttpContext.Session.GetString("SubjectAreaName");
            ViewBag.SubjectAreaName = subjectAreaName;

            return View("Views\\ConfigureSubjectArea\\ConfigureDataCollection.cshtml");
        }
        public IActionResult GetMasterName()
        {
            SPONGE_Context sPONGE_Context = new();
            var fieldName = (from s in sPONGE_Context.SPG_MPP_MASTER
                          join h in sPONGE_Context.SPG_MPP_HIERARCHY on s.ENTITY_TYPE_ID equals h.ID into Details
                          from m in Details.DefaultIfEmpty()
                          where m.ID == null && !s.MASTER_DISPLAY_NAME.Contains("Mapping")
                          select new { s.COLUMN_NAME, s.COLUMN_DISPLAY_NAME }).Distinct().ToList();
            return Json(fieldName);
        }
        public IActionResult GetUOM()
        {
            SPONGE_Context sPONGE_Context = new();
            var fieldName = sPONGE_Context.SPG_UOM.Where(o => o.ACTIVE_FLAG == "Y")
                .Select(o => new { o.UOM_CODE, o.UOM_DESC }).Distinct().ToList();
            return Json(fieldName);
        }
        [HttpPost]
        public IActionResult SaveDataCollection(List<SaveDataCollection> data)
        {
            SPONGE_Context sPONGE_Ctx = new();
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            var selectedSubjectArea = TempData["selectedSubjectArea"] as int?;
            TempData.Keep();

            // Retrieving the list of selected dimensions
            string dimensionsString = HttpContext.Session.GetString("Dimensions");
            List<Dimension> dimensions = JsonConvert.DeserializeObject<List<Dimension>>(dimensionsString);

            foreach (var collection in data)
            {
                if (collection.DisplayName == null && collection.FieldName == null)
                {
                    //To avoid empty row from saving don't add the data in Model
                }
                else
                {
                    SPG_SUBJECT_DATACOLLECTION sPG_1 = new();
                    {
                        sPG_1.SUBJECTAREA_ID = selectedSubjectArea;
                        sPG_1.DISPLAY_NAME = collection.DisplayName;
                        sPG_1.FIELD_NAME = collection.FieldName;
                        sPG_1.DATA_TYPE = collection.DataType;
                        sPG_1.UOM = collection.UOM;
                        sPG_1.IS_LOOKUP = collection?.IsLookUp;
                        if (collection?.IsLookUp == "Y")
                            sPG_1.DISPLAY_TYPE = "DropDown";
                        else sPG_1.DISPLAY_TYPE = "TextBox";
                        sPG_1.LOOKUP_TYPE = collection?.LookUpType;
                        sPG_1.ACTIVE_FLAG = "Y";
                        sPG_1.CREATED_DATE = DateTime.Now;
                        sPG_1.CREATED_BY = userName[1].ToString();
                    }
                    sPONGE_Ctx.SPG_SUBJECT_DATACOLLECTION.Add(sPG_1);
                    sPONGE_Ctx.SaveChanges();
                }
            }


            var selectedUser = (from config in sPONGE_Ctx.SPG_CONFIGURATION
                                join user in sPONGE_Ctx.SPG_USERS on config.USER_ID equals user.USER_ID
                                where config.SUBJECTAREA_ID == selectedSubjectArea
                                select new
                                {
                                    UserId = config.USER_ID,
                                    UserName = user.Name
                                }).Distinct().ToList();

            ViewBag.SelectedUser = selectedUser.ToList();

            var subjectAreaName = HttpContext.Session.GetString("SubjectAreaName");
            ViewBag.SubjectAreaName = subjectAreaName;


            //To show the list of Mastres on user page
            // Create a Dictionary where each key is a dimension and the value is a list of corresponding names.
            Dictionary<string, List<string>> dimensionData = new Dictionary<string, List<string>>();

            foreach (var dimension in dimensions)
            {
                List<SPG_MPP_MASTER> spg_mpp_master =
                                                (
                                                    from user in sPONGE_Ctx.SPG_MPP_MASTER
                                                    where user.MPP_DIMENSION_NAME == dimension.Value
                                                    group user by user.MASTER_DISPLAY_NAME into g
                                                    select g.First()
                                                ).ToList();

                List<string> names = new List<string>();

                foreach (var item in spg_mpp_master)
                {
                    names.Add(item.MASTER_DISPLAY_NAME);
                }

                dimensionData.Add(dimension.Value, names);
            }

            ViewBag.DimensionData = dimensionData;

            return View("Views\\ConfigureSubjectArea\\AssignUsers.cshtml");
        }
        public IActionResult GetUserList()
        {
            var selectedSubjectArea = TempData["selectedSubjectArea"] as int?;
            TempData.Keep();
            SPONGE_Context context = new();

            var usernames = from S in context.SPG_SUBJECTAREA
                            join SF in context.SPG_SUBFUNCTION on S.SUBFUNCTION_ID equals SF.SUBFUNCTION_ID
                          
                            join UF in context.SPG_USERS_FUNCTION on SF.SUBFUNCTION_ID equals UF.SUB_FUNCTION_ID

                            join U in context.SPG_USERS on UF.USER_ID equals U.USER_ID

                            join R in context.SPG_ROLE on UF.ROLE_ID equals R.ROLE_ID
                            where S.SUBJECTAREA_ID == selectedSubjectArea
                            group new { U, UF, SF, R } by
                                new { U.USER_ID, U.EMAIL_ID, U.Name, U.ACTIVE_FLAG }
           into g
                       select new
                            {
                                username = g.Key.Name,
                                userid = g.Key.USER_ID
                            }
                       ;

            // UserInfo = query.ToList();
            return Json(usernames);
        }
        public IActionResult SaveUsersConfiguration(List<SaveUsers> selectedusers, IFormCollection formData)
        {
            SPONGE_Context sPONGE_Ctx = new();
            SPG_CONFIG_STRUCTURE SPG_CONFIG_STRUCTURE = new SPG_CONFIG_STRUCTURE();
            // List<UserConfiguration>UserConfigurations =new List<UserConfiguration>();
            var selectedSubjectArea = TempData["selectedSubjectArea"] as int?;

            var timeDefinition = sPONGE_Ctx.SPG_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID == selectedSubjectArea).FirstOrDefault();
            string headerName = string.Empty;
            string headerlist_definer = "N";
            string grpcolumnname_definer = "N";
            List<string> headerList = new List<string>();
            if (timeDefinition.FREQUENCY.Equals("MONTHLY"))
            {
                if (timeDefinition.FREQUENCY.Equals("MONTHLY") && (timeDefinition.TIME_LEVEL.Equals("MONTHLY") || timeDefinition.TIME_LEVEL.Equals("DAILY")) && timeDefinition.PERIOD.Equals("1"))
                {
                    headerlist_definer = "Y";
                    grpcolumnname_definer = "Y";
                }
                else
                {
                    headerList = new Sponge.Common.TimeFrequency().MonthlyFrequency(timeDefinition.FREQUENCY, timeDefinition.TIME_LEVEL, timeDefinition.REPORTING_PERIOD, timeDefinition.PERIOD);
                }
            }
            else if (timeDefinition.FREQUENCY.Equals("YEARLY"))
            {

                if (timeDefinition.FREQUENCY.Equals("YEARLY") && timeDefinition.TIME_LEVEL.Equals("YEARLY") && timeDefinition.PERIOD.Equals("1"))
                {
                    headerlist_definer = "Y";
                    grpcolumnname_definer = "Y";
                }
                else
                {
                    headerList = new Sponge.Common.TimeFrequency().AnnualFrequency(timeDefinition.FREQUENCY, timeDefinition.TIME_LEVEL, timeDefinition.REPORTING_PERIOD, timeDefinition.PERIOD);
                }
            }
            else if (timeDefinition.FREQUENCY.Equals("HALF_YEARLY"))
            {
                if (timeDefinition.FREQUENCY.Equals("HALF_YEARLY") && timeDefinition.TIME_LEVEL.Equals("HALF_YEARLY") && timeDefinition.PERIOD.Equals("1"))
                {
                    headerlist_definer = "Y";
                    grpcolumnname_definer = "Y";
                }
                else
                {
                    headerList = new Sponge.Common.TimeFrequency().HalfYearlyFrequency(timeDefinition.FREQUENCY, timeDefinition.TIME_LEVEL, timeDefinition.REPORTING_PERIOD, timeDefinition.PERIOD);

                }
            }
            else if (timeDefinition.FREQUENCY.Equals("QUARTERLY"))
            {
                if (timeDefinition.FREQUENCY.Equals("QUARTERLY") && timeDefinition.TIME_LEVEL.Equals("QUARTERLY") && timeDefinition.PERIOD.Equals("1"))
                {
                    headerlist_definer = "Y";
                    grpcolumnname_definer = "Y";
                }
                else
                {
                    headerList = new Sponge.Common.TimeFrequency().QuarterlyFrequency(timeDefinition.FREQUENCY, timeDefinition.TIME_LEVEL, timeDefinition.REPORTING_PERIOD, timeDefinition.PERIOD);
                }
            }
            else if (timeDefinition.FREQUENCY.Equals("WEEKLY"))
            {

                if (timeDefinition.FREQUENCY.Equals("WEEKLY") && timeDefinition.TIME_LEVEL.Equals("WEEKLY") && timeDefinition.PERIOD.Equals("1"))
                {
                    headerlist_definer = "Y";
                    grpcolumnname_definer = "Y";
                }
                else
                {
                    headerList = new Sponge.Common.TimeFrequency().WeeklyFrequency(timeDefinition.FREQUENCY, timeDefinition.TIME_LEVEL, timeDefinition.REPORTING_PERIOD, timeDefinition.PERIOD);
                }
            }
            else if (timeDefinition.FREQUENCY.Equals("DAILY"))
            {

                if (timeDefinition.FREQUENCY.Equals("DAILY") && timeDefinition.TIME_LEVEL.Equals("DAILY") && timeDefinition.PERIOD.Equals("1"))
                {
                    headerlist_definer = "Y";
                    grpcolumnname_definer = "Y";
                }
            }
            else
            {

            }
            try
            {

                if (selectedusers.Count > 0)
                {


                    var DataCollection = (from p in sPONGE_Ctx.SPG_SUBJECT_DATACOLLECTION
                                          where p.SUBJECTAREA_ID == selectedSubjectArea
                                          select new UserConfiguration
                                          {
                                              FieldName = p.FIELD_NAME,
                                              DisplayName = p.DISPLAY_NAME,
                                              UOM = p.UOM,
                                              DisplayType = p.DISPLAY_TYPE,
                                              CollectionType = "Measure",
                                              LookUpType = p.LOOKUP_TYPE,
                                              IsLookUp = p.IS_LOOKUP,
                                              DataType = p.DATA_TYPE,
                                              MasterName = "",
                                              DimensionTable = ""

                                          }).ToList();

                    

                    var Masters = (from p in sPONGE_Ctx.SPG_SUBJECT_MASTER
                                   where p.SUBJECTAREA_ID == selectedSubjectArea
                                   select new UserConfiguration
                                   {
                                       FieldName = p.FIELD_NAME,
                                       DisplayName = p.DISPLAY_NAME,
                                       UOM = "",
                                       DisplayType = "",
                                       CollectionType = "Master",
                                       LookUpType = "",
                                       IsLookUp = "N",
                                       DataType = "",
                                       IsShow = p.IS_SHOW,
                                       MasterName = p.MASTER_NAME,
                                       DimensionTable = p.DIMENSION_TABLE
                                   }).ToList();
                    var UserConfigurations = Masters.Concat(DataCollection).ToList();

                    //Code to save Masters at Assign users page
                    //var formDataValues = formData.Keys.Select(k => formData[k].ToString()).ToList();
                    //var matchingData = sPONGE_Ctx.SPG_MPP_MASTER
                    //                    .Where(o => formDataValues.Contains(o.MASTER_DISPLAY_NAME))
                    //                    .Select(o => o.COLUMN_NAME)
                    //                    .ToList();
                    //// TO get the values with only SHORT_NAME and CODE
                    //var shortNameAndCode = matchingData.Where(s => s.Contains("_SHORT_NAME") || s.Contains("_CODE")).ToList();

                    //var Masters2 = (from p in sPONGE_Ctx.SPG_SUBJECT_MASTER
                    //               where p.SUBJECTAREA_ID == selectedSubjectArea
                    //               select new UserConfiguration
                    //               {
                    //                   //FieldName = ShortName from spg_mpp_master
                    //                   DisplayName = null,
                    //                   UOM = "",
                    //                   DisplayType = "",
                    //                   CollectionType = "Master",
                    //                   LookUpType = "",
                    //                   IsLookUp = "N",
                    //                   DataType = "",
                    //                   IsShow = "Y",       //Enter 2 rows When ENT_COUNTRY_CODE then isShow will be "N" if shortname then "Y" 
                    //                   MasterName = p.MASTER_NAME,
                    //                   DimensionTable = p.DIMENSION_TABLE
                    //               }).ToList();

                    

                    SaveConfigStructure(selectedusers, sPONGE_Ctx, selectedSubjectArea, UserConfigurations, headerList, headerlist_definer, grpcolumnname_definer);
                    try
                    {

                        var IsGroupNameExist = sPONGE_Ctx.SPG_CONFIG_STRUCTURE.Where(w => w.SUBJECTAREA_ID == selectedSubjectArea && w.GROUPCOLUMNNAME != null && w.COLLECTION_TYPE == "Measure").FirstOrDefault();
                        if (IsGroupNameExist != null)
                            CreateSubjectAreaView(selectedSubjectArea, true);//Call SP which is creating View for respective  Subject Area, if Group Column name is not null
                        else
                            CreateSubjectAreaView(selectedSubjectArea, false);//Call SP which is creating View for respective  Subject Area

                    }
                    catch (Exception ex)
                    {
                        ErrorLog lgerr = new ErrorLog();
                        lgerr.LogErrorInTextFile(ex);
                    }
                }

            }
            catch (Exception ex)
            { }

            return RedirectToAction("ConfigureTemplate", "ConfigureTemplate", selectedSubjectArea);
        }

        public void CreateSubjectAreaView(int? SubjectAreaId, bool IsGroupColumnNameExist)
        {
            try
            {
                SPONGE_Context sPONGE_Context = new SPONGE_Context();

                //string FormedQueryLookupType = string.Empty;
                if (IsGroupColumnNameExist)
                {
                    var SubjectAreaIdValue = new SqlParameter("p_SubjectAreaId", SubjectAreaId);
                    var pSuccessParameter = new SqlParameter("@p_success", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    sPONGE_Context.Database.ExecuteSqlRaw(
                            "EXEC dbo.SP_CREATEETLVIEW_GROUPCOLUMN @p_SubjectAreaId, @p_success",
                            SubjectAreaIdValue,
                            pSuccessParameter
                    );
                }
                else
                {
                    var SubjectAreaIdValue = new SqlParameter("p_SubjectAreaId", SubjectAreaId);
                    var pSuccessParameter = new SqlParameter("@p_success", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output,


                    };

                    sPONGE_Context.Database.ExecuteSqlRaw(
                            "EXEC dbo.SP_CREATEETLVIEW_NORMAL @p_SubjectAreaId, @p_success",
                            SubjectAreaIdValue,
                            pSuccessParameter
                    );
                }

            }
            catch (Exception ex)
            {

            }
        }
        private List<int> SaveConfiguration(SPONGE_Context objModel, string configName, int? subjectAreaId, string userId)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

            List<int> configId_List = objModel.SPG_CONFIGURATION.Where(s => s.SUBJECTAREA_ID == subjectAreaId).Where(s => s.USER_ID == userId).Select(s => s.CONFIG_ID).ToList();
            if (configId_List.Count > 0)
            {
                return configId_List;
            }
            else
            {
                SPG_CONFIGURATION config = new SPG_CONFIGURATION()
                {
                    Config_Name = configName,
                    SUBJECTAREA_ID = subjectAreaId,
                    USER_ID = userId,
                    Created_By = userName[1],
                    Created_On = DateTime.Now
                };
                objModel.SPG_CONFIGURATION.Add(config);
                objModel.SaveChanges();
                configId_List.Add(config.CONFIG_ID);
                return configId_List;
            }
        }

        public void SaveConfigStructure(List<SaveUsers> model, SPONGE_Context objModel, int? id, List<UserConfiguration> data, List<string> headerList, string headerlist_definer, string grpcolumnname_definer)
        {

            foreach (var i in model)
            {
                List<int> lstConfigId = SaveConfiguration(objModel, "", id, i.UserId);

                foreach (var configId in lstConfigId)
                {

                    foreach (var item in data)
                    {

                        if (!string.IsNullOrEmpty(item.DisplayName))
                        {

                            int ConfigUserId = objModel.SPG_CONFIG_STRUCTURE.Where(x => x.FIELD_NAME == item.FieldName && x.SUBJECTAREA_ID == id && x.USER_ID == i.UserId && x.CONFIG_ID == configId).Select(x => x.CONFIGUSER_ID).FirstOrDefault();

                            if (item.CollectionType == "Measure" && ConfigUserId == 0)
                            {

                                if (headerList.Count <= 0 && headerlist_definer == "Y")
                                {
                                    SaveConfigMeasure(objModel, ConfigUserId, i.UserId, id, configId, item, item.DisplayName, grpcolumnname_definer);
                                }
                                else
                                {
                                    foreach (var header in headerList)
                                    {
                                        SaveConfigMeasure(objModel, ConfigUserId, i.UserId, id, configId, item, header, headerlist_definer);

                                    }
                                }
                            }
                            else if (item.CollectionType == "Master" && ConfigUserId == 0)
                            {
                                SPG_CONFIG_STRUCTURE o = new SPG_CONFIG_STRUCTURE();
                                if (ConfigUserId > 0)
                                {
                                    o = objModel.SPG_CONFIG_STRUCTURE.Where(x => x.FIELD_NAME == item.FieldName && x.SUBJECTAREA_ID == id && x.USER_ID == i.UserId && x.CONFIG_ID == configId).FirstOrDefault();

                                }
                                o.USER_ID = i.UserId;
                                o.SUBJECTAREA_ID = (int)id;
                                o.FIELD_NAME = item.FieldName;
                                //o.DISPLAY_NAME = item.DisplayName;
                                o.DIMENSION_TABLE = item.DimensionTable;
                                o.CONFIG_ID = configId;
                                string DataType = objModel.SPG_CONFIG_STRUCTURE.Where(x => x.FIELD_NAME == item.FieldName && x.SUBJECTAREA_ID == id && x.COLLECTION_TYPE == "Master").Select(x => x.DATA_TYPE).FirstOrDefault();

                                if (DataType == null)
                                {

                                    item.DataType = "VC";

                                    DataType = GetDataTypeCounter(objModel, id, item.DataType);

                                }
                                o.COLLECTION_TYPE = "Master";
                                o.GROUPCOLUMNNAME = string.Empty;
                                o.IS_ETL = "Y";
                                o.IS_SHOW = Convert.ToString(item.IsShow);
                                o.DISPLAY_NAME = item.DisplayName;
                                o.DATA_TYPE = DataType;
                                o.DISPLAY_TYPE = "Label";
                                o.MASTER_NAME = item.MasterName;
                                objModel.SPG_CONFIG_STRUCTURE.Add(o);
                                objModel.SaveChanges();

                            }
                            else if (item.CollectionType == "Master" && ConfigUserId > 0)
                            {
                                SPG_CONFIG_STRUCTURE o = new SPG_CONFIG_STRUCTURE();
                                if (ConfigUserId > 0)
                                {
                                    o = objModel.SPG_CONFIG_STRUCTURE.Where(x => x.FIELD_NAME == item.FieldName && x.SUBJECTAREA_ID == id && x.USER_ID == i.UserId && x.CONFIG_ID == configId).FirstOrDefault();

                                }

                                o.IS_SHOW = (Convert.ToString(item.IsShow));
                                objModel.SaveChanges();

                            }
                        }
                    }
                }
            }
        }
        public string GetDataTypeCounter(SPONGE_Context objModel, int? id, string dataType)
        {
            var outputParameter = new SqlParameter()
            {
                ParameterName = "@outputParameter",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Size = 50,
                Direction = System.Data.ParameterDirection.Output
            };

            objModel.Database.ExecuteSqlRaw("dbo.SP_GETDATATYPECOUNTER @p_subjectAreaID, @p_DATA_TYPE,@outputParameter OUTPUT",
              parameters: new[] {
      new SqlParameter("@p_subjectAreaID", id),
      new SqlParameter("@p_DATA_TYPE", SqlDbType.VarChar, 100) { Value = dataType },
      outputParameter
              }
            );

            var DataType = (string)outputParameter.Value;

            if (DataType == null)
            {
                DataType = dataType + "0";
            }

            return DataType;
        }
        [HttpPost]
        public IActionResult ConfigureMasterGroup(IFormCollection function, string Command)
        {

            return RedirectToAction("Function");
        }
        private void SaveConfigMeasure(SPONGE_Context objModel, int ConfigUserId, string userId, int? id, int configId, UserConfiguration item, string header, string grpcolumnname_definer)
        {

            SPG_CONFIG_STRUCTURE o = new SPG_CONFIG_STRUCTURE();
            if (ConfigUserId > 0)
            {
                o = objModel.SPG_CONFIG_STRUCTURE.Where(x => x.FIELD_NAME == item.FieldName && x.SUBJECTAREA_ID == id && x.USER_ID == userId && x.CONFIG_ID == configId).FirstOrDefault();

            }

            o.USER_ID = userId;
            o.SUBJECTAREA_ID = (int)id;
            o.FIELD_NAME = item.FieldName;
            o.MASTER_NAME = item.MasterName;
            o.CONFIG_ID = configId;
            o.COLLECTION_TYPE = "Measure";
            string DataType = objModel.SPG_CONFIG_STRUCTURE.Where(x => x.DISPLAY_NAME == header && x.SUBJECTAREA_ID == id && x.COLLECTION_TYPE == "Measure").Select(x => x.DATA_TYPE).FirstOrDefault();
            if (DataType == null)
            {
                DataType = GetDataTypeCounter(objModel, id, item.DataType);

            }

            o.IS_ETL = "Y";
            o.IS_SHOW = "Y";

            o.DISPLAY_NAME = header;
            o.FIELD_NAME = item.FieldName;
            o.GROUPCOLUMNNAME = grpcolumnname_definer == "Y" ? null : item.DisplayName;
            o.DISPLAY_TYPE = item.DisplayType;
            o.UOM = item.UOM;
            o.LOOKUP_TYPE = item.LookUpType;
            o.DATA_TYPE = DataType;
            objModel.SPG_CONFIG_STRUCTURE.Add(o);
            objModel.SaveChanges();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




    }
}