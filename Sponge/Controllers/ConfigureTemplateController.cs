using DAL.Common;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sponge.Common;
using Sponge.Models;
using Sponge.ViewModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ConfigureTemplateController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;
        public ConfigureTemplateController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }


        public IActionResult ConfigureTemplate()
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_SUBJECTAREA.Select(o => new { o.SUBJECTAREA_NAME, o.SUBJECTAREA_ID }).Distinct();
            ViewBag.SubjectArea = new SelectList(lst.ToList(), "SUBJECTAREA_NAME", "SUBJECTAREA_ID");


            return View();
        }
        public IActionResult SetUp(int configID)
        {


            using (SPONGE_Context context = new SPONGE_Context())
            {
                var result = context.SPG_CONFIGURATION
                     .Join(context.SPG_USERS,
                         config => config.USER_ID,
                         user => user.USER_ID,
                         (config, user) => new { CONFIGURATION = config, USER = user })
                     .Join(context.SPG_SUBJECTAREA,
                         configUser => configUser.CONFIGURATION.SUBJECTAREA_ID,
                         subject => subject.SUBJECTAREA_ID,
                         (configUser, subject) => new
                         {
                             configUser.CONFIGURATION.CONFIG_ID,
                             NAME = configUser.USER.Name,
                             subject.SUBJECTAREA_NAME,
                             subject.FREQUENCY,
                             subject.TIME_LEVEL,
                             ACTIVE_FLAG = configUser.CONFIGURATION.ACTIVE_FLAG ?? "Y",
                             SCHEDULED = configUser.CONFIGURATION.SCHEDULED ?? "N",
                             configUser.CONFIGURATION.LOCK_DATE,
                             configUser.CONFIGURATION.PATTERN_MONTH,
                             configUser.CONFIGURATION.PATTERN,
                             configUser.CONFIGURATION.REMMINDER_DATE,
                             configUser.CONFIGURATION.ESCALATION_DATE,
                             configUser.CONFIGURATION.APPROVER_EMAILD,
                             configUser.CONFIGURATION.APPROVER_NAME,
                             configUser.CONFIGURATION.APPROVER_ID,
                             configUser.CONFIGURATION.EFFECTIVE_TO,
                             configUser.CONFIGURATION.Created_On,
                             configUser.CONFIGURATION.Config_Name

                         })
                     .Where(x => x.CONFIG_ID == configID)
                     .FirstOrDefault();


                if (result != null)
                {

                    ViewBag.Result = result;
                    ViewBag.Months = GetMonths();
                    return View();
                }
            }
            return NotFound();
        }
        public List<SelectListItem> GetMonths()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items = DateTimeFormatInfo
        .InvariantInfo
        .MonthNames
        .TakeWhile(monthName => monthName != String.Empty)
        .Select((monthName, index) => new SelectListItem
        {
            Value = (index + 1).ToString(CultureInfo.InvariantCulture),
            Text = string.Format("({0}) {1}", index + 1, monthName)
        }).ToList();
            return items;
        }
        public IActionResult SaveSetUp(SetupUser data, int configID)
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            // Fetch the record to be updated
            var configRecord = spONGE_Context.SPG_CONFIGURATION.FirstOrDefault(x => x.CONFIG_ID == configID);
            // TO set the default effective to date
            var defaultEffectiveToDate = _configuration.GetSection("AppSettings:DefaultEffectiveToDate").Value;

            if (configRecord != null)
            {
                // Update the fields
                configRecord.ACTIVE_FLAG = data.ACTIVE_FLAG;
                configRecord.Config_Name = data.CONFIG_NAME;
                configRecord.SCHEDULED = data.SCHEDULED;
                configRecord.LOCK_DATE = data.LOCK_DATE;
                configRecord.PATTERN = data.PATTERN;
                configRecord.DATA_COLLECTION = "OFFLINE";
                configRecord.PATTERN_MONTH = data.PATTERN_MONTH;
                configRecord.REMMINDER_DATE = data.REMMINDER_DATE;
                configRecord.ESCALATION_DATE = data.ESCALATION_DATE;
                configRecord.APPROVER_NAME = data.APPROVER_NAME;
                configRecord.APPROVER_EMAILD = data.APPROVER_EMAILID;
                configRecord.APPROVER_ID = data.APPROVER_ID;
                configRecord.MODIFIED_BY = userName[1];
                configRecord.MODIFIED_DATE = DateTime.Now;
                if (data.ACTIVE_FLAG == "Y")
                {
                    configRecord.EFFECTIVE_TO = Convert.ToDateTime(defaultEffectiveToDate);
                }
                else
                {
                    configRecord.EFFECTIVE_TO = data.EFFECTIVE_TO;
                }
                // Save the changes
                spONGE_Context.SaveChanges();
            }
            // Redirect to ConfigureTemplate action
            return RedirectToAction("ConfigureTemplate");
        }
        public JsonResult GetUserInfoByEmail(string email)
        {
            UserInfo userInfo = new UserInfo();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, "USAWS1ESI56.apac.ko.com"))
                {
                    UserPrincipal userPrincipal = new UserPrincipal(context);
                    userPrincipal.EmailAddress = email;

                    PrincipalSearcher search = new PrincipalSearcher(userPrincipal);

                    var user = (UserPrincipal)search.FindOne();

                    if (user != null)
                    {
                        userInfo.UserId = user.SamAccountName;
                        userInfo.UserName = user.DisplayName;
                        userInfo.UserEmail = user.EmailAddress;
                        userInfo.ErrorMsg = "";
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(userInfo);
        }
        [HttpGet]
        public IActionResult GetUserList(int subjectAreaId)
        {

            SPONGE_Context context = new();
            var usernames = from U in context.SPG_USERS
                            join SPG in context.SPG_CONFIGURATION on U.USER_ID equals SPG.USER_ID
                            where SPG.SUBJECTAREA_ID == subjectAreaId
                            group U by new { SPG.CONFIG_ID, U.Name, U.USER_ID, SPG.ACTIVE_FLAG } into g
                            select new
                            {
                                subjectAreaid = subjectAreaId,
                                configID = g.Key.CONFIG_ID,
                                username = g.Key.Name,
                                activeflag = g.Key.ACTIVE_FLAG
                            };

            // TO check if the data is already saved in SPG_Config_filter for this config id
            var savedConfigIDs = (from U in usernames
                                  join CFV in context.SPG_CONFIG_FILTERS_VALUE on U.configID equals CFV.CONFIG_ID
                                  select U.configID).Distinct().ToList();

            // UserInfo = query.ToList();
            return Json(new { Usernames = usernames, SavedConfigIDs = savedConfigIDs });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [Route("ConfigureTemplate/DataFilter/subjectAreaId/{subjectAreaId:int}/configID/{configId:int}")]
        public IActionResult DataFilter(int subjectAreaId, int configId)
        {
            SPONGE_Context spONGE_Ctx = new();
            //var subjectAreaId = spONGE_Ctx.SPG_CONFIG_STRUCTURE.Where(x => x.CONFIG_ID == configID).Select(x => x.SUBJECTAREA_ID).FirstOrDefault();
            if (configId != null)
                ViewBag.configID = configId;
            // To reterive and get the current user name for selected data filter
            var selectedUser = (from config in spONGE_Ctx.SPG_CONFIGURATION
                                join user in spONGE_Ctx.SPG_USERS
                                on config.USER_ID equals user.USER_ID
                                where config.CONFIG_ID == configId
                                select new
                                {
                                    UserId = config.USER_ID,
                                    UserName = user.Name
                                }).FirstOrDefault();

            var selectedSubjectAreaName = (from sbjArea in spONGE_Ctx.SPG_SUBJECTAREA
                                           where sbjArea.SUBJECTAREA_ID == subjectAreaId
                                           select sbjArea.SUBJECTAREA_NAME).FirstOrDefault();
            ViewBag.ThisUserName = selectedUser.UserName;
            ViewBag.ThisSubjectAreaName = selectedSubjectAreaName;
            if (selectedUser != null)
            {
                HttpContext.Session.SetString("thisUserName", selectedUser.UserName);
            }

            if (!string.IsNullOrEmpty(selectedSubjectAreaName))
            {
                HttpContext.Session.SetString("thisSubjectAreaName", selectedSubjectAreaName);
            }
            // TO check if the data is already saved in SPG_Config_filter for this config id
            bool checkIfSaved = (from x in spONGE_Ctx.SPG_CONFIG_FILTERS
                                 where x.CONFIG_ID == configId
                                 select x).Any();

            ViewBag.CheckIfSaved = checkIfSaved;
            var dimensionList = (from x in spONGE_Ctx.SPG_SUBJECT_DIMENSION
                                 where x.SUBJECTAREA_ID == subjectAreaId
                                 select new
                                 {
                                     DIMENSION_NAME = x.MPP_DIMENSION_NAME,
                                     DIMENSIONTABLENAME = x.DIMENSION_TABLE
                                 }).Distinct().ToList();
            ViewBag.Dimensions = new SelectList(dimensionList, "DIMENSIONTABLENAME", "DIMENSION_NAME");

            return View();
        }
        [HttpPost]
        public JsonResult GetSubDimensionsList(List<string> dimensions, int configID)
        {
            SPONGE_Context spONGE_Ctx = new();
            //To get the subdimensions list
            //Create a Dictionary where each key is a dimension and the value is a list of corresponding names.
            Dictionary<string, Dictionary<string, string>> dimensionData = new Dictionary<string, Dictionary<string, string>>();

            List<string> savedMasterNames = new List<string>();

            foreach (var dimension in dimensions)
            {
                List<SPG_MPP_MASTER> spg_mpp_master =
                                                (
                                                    from x in spONGE_Ctx.SPG_MPP_MASTER
                                                    where x.MPP_DIMENSION_NAME == dimension
                                                    group x by x.MASTER_DISPLAY_NAME into g
                                                    select g.First()
                                                ).ToList();

                Dictionary<string, string> names = new Dictionary<string, string>();

                foreach (var item in spg_mpp_master)
                {
                    names.Add(item.MASTER_DISPLAY_NAME, item.MASTER_NAME);
                    // check if MASTER_NAME is in SPG_CONFIG_FILTERS
                    if (spONGE_Ctx.SPG_CONFIG_FILTERS.Any(cf => cf.MASTER_COLUMN == item.MASTER_NAME && cf.CONFIG_ID == configID))
                    {
                        if (!savedMasterNames.Contains(item.MASTER_NAME))
                        {
                            savedMasterNames.Add(item.MASTER_NAME);
                        }
                    }
                }

                if (!dimensionData.ContainsKey(dimension))
                {
                    dimensionData.Add(dimension, names);
                }
            }
            return Json(new { DimensionData = dimensionData, SavedMasterNames = savedMasterNames });
        }
        public async Task<IActionResult> GetEmailSuggestions(string email, int configid)
        {
            using (var sPONGE_Context = new SPONGE_Context())
            {
                var matchingEmails = await (from u in sPONGE_Context.SPG_USERS
                                            join o in sPONGE_Context.SPG_USERS_FUNCTION on u.USER_ID equals o.USER_ID

                                            join c in sPONGE_Context.SPG_CONFIGURATION on configid equals c.CONFIG_ID
                                            join s in sPONGE_Context.SPG_SUBJECTAREA on c.SUBJECTAREA_ID equals s.SUBJECTAREA_ID
                                            where u.EMAIL_ID.Contains(email)
                                           && (o.ROLE_ID == 4 || o.ROLE_ID == 5) && o.SUB_FUNCTION_ID == s.SUBFUNCTION_ID
                                            select new
                                            {
                                                EMAIL_ID = u.EMAIL_ID,

                                            }).Distinct().ToListAsync();
                if (!matchingEmails.Any()) return NotFound();

                return Ok(matchingEmails);
            }
        }
        //public IActionResult SaveDataFilter(IFormCollection formData)
        //{
        //    int count = 0;
        //    foreach (var key in formData.Keys)
        //    {
        //        if (key.StartsWith("dimensions"))
        //        {
        //            count++;
        //        }
        //    }
        //    try
        //    {
        //        if (count > 0)
        //        {
        //            SPONGE_Context sPONGE_Context = new();
        //            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
        //            foreach (var dmensionName in formData["dimensionSelect"])
        //            {
        //                var currentDimensionName = dmensionName;

        //                for (int i = 0; i < count; i++)
        //                {
        //                    // Fetch the value associated with each completeText key
        //                    string completeTextKey = "completeText" + i;
        //                    string completeTextValue = formData[completeTextKey];

        //                    // Split and store in a new array.
        //                    string[] array = completeTextValue
        //                                        .Split(',')
        //                                        .Select(p => p.Trim())
        //                                        .Where(p => !string.IsNullOrEmpty(p))
        //                                        .ToArray();

        //                    foreach (var item in array)
        //                    {
        //                        //Get the Master name for current selected sub dimension checkbox item
        //                        var currentMasterName = (
        //                                                    from x in sPONGE_Context.SPG_MPP_MASTER
        //                                                    where x.MASTER_DISPLAY_NAME == item
        //                                                    select x.MASTER_NAME
        //                                                ).FirstOrDefault();
        //                        // Create object to save the data
        //                        SPG_CONFIG_FILTERS sPG_CONFIG_FILTERS = new()
        //                        {
        //                            CONFIG_ID = Int32.Parse(formData["configID"].ToString()),
        //                            DIMENSION_TABLE = dmensionName,
        //                            MASTER_COLUMN = currentMasterName,
        //                            ACTIVE_FLAG = "Y",
        //                            CREATED_BY = userName[1],
        //                            CREATED_ON = DateTime.Now,
        //                            MODIFIED_BY = null,
        //                            MODIFIED_ON = null,
        //                            MASTER_COLUMN_LEVEL = null,

        //                        };
        //                        sPONGE_Context.Add(sPG_CONFIG_FILTERS);
        //                        sPONGE_Context.SaveChanges();
        //                    }
        //                }
        //            }                 
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ViewBag.ErrorMessage = e.Message;
        //    }
        //    return View(SaveDataFilter);
        //}
        public class DataFilterModel
        {
            public int ConfigId { get; set; }
            public List<string> Dimensions { get; set; }
            public Dictionary<string, List<string>> MasterNames { get; set; }
        }
        [HttpPost]
        public IActionResult SaveDataFilterNext([FromBody] DataFilterModel data)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context _Context = new();
            var configId = data.ConfigId;
            //var dimensions = data.Dimensions;
            var masterNames = data.MasterNames;
            foreach (var dimension in masterNames)
            {

                foreach (string masterName in dimension.Value)
                {
                    var dimensioncode = _Context.SPG_MPP_MASTER.Where(x => x.MPP_DIMENSION_NAME == dimension.Key)
                    .Select(x => x.DIMENSION_TABLE).FirstOrDefault();
                    var mastercode = _Context.SPG_MPP_MASTER.Where(x => x.MASTER_DISPLAY_NAME == masterName && x.MPP_DIMENSION_NAME == dimension.Key)
                    .Select(x => x.MASTER_NAME).FirstOrDefault();

                    if (!_Context.SPG_CONFIG_FILTERS.Any(x => x.MASTER_COLUMN == mastercode && x.CONFIG_ID == configId))
                    {
                        // Create a new entity for your table
                        SPG_CONFIG_FILTERS entity = new SPG_CONFIG_FILTERS
                        {
                            CONFIG_ID = configId,
                            DIMENSION_TABLE = dimensioncode,
                            MASTER_COLUMN = mastercode,
                            CREATED_BY = userName[1],
                            CREATED_ON = DateTime.Now
                        };
                        // Add the entity to your context
                        _Context.SPG_CONFIG_FILTERS.Add(entity);
                    }
                }
            }
            TempData["Masters"] = JsonConvert.SerializeObject(masterNames);
            TempData["ConfigId"] = configId;
            // Save changes to the database
            _Context.SaveChanges();

            ViewBag.ThisUserName = HttpContext.Session.GetString("thisUserName");
            ViewBag.ThisSubjectAreaName = HttpContext.Session.GetString("thisSubjectAreaName");

            return Json(Url.Action("SaveDataFilter", "ConfigureTemplate"));
        }

        public async Task<IActionResult> SaveDataFilter()
        {
            var serializedData = TempData["Masters"];
            var configID = TempData["ConfigId"];
            var masterNames = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>((string)serializedData);
            SPONGE_Context _Context = new();
            var masterValuesDictionary = new Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>();

            foreach (var dimension in masterNames)
            {
                var dimensionDict = new Dictionary<string, List<Dictionary<string, string>>>();

                foreach (var mastervalue in dimension.Value)
                {

                    using (var command = _Context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = "SP_GETFILTERATION_DATA_FINAL";
                        var dimensionParam = new SqlParameter("@p_DimensionName", dimension.Key);
                        var masterParam = new SqlParameter("@p_MasterName", mastervalue);
                        var config_ID = new SqlParameter("@v_configId", configID);
                        command.Parameters.Add(dimensionParam);
                        command.Parameters.Add(masterParam);
                        command.Parameters.Add(config_ID);

                        command.CommandType = CommandType.StoredProcedure;

                        _Context.Database.OpenConnection();

                        using (var result = command.ExecuteReader())
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(result);

                            // parse datatable into list of dictionaries
                            var dataList = new List<Dictionary<string, string>>();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                var dict = new Dictionary<string, string>();
                                foreach (DataColumn column in dataTable.Columns)
                                {
                                    dict[column.ColumnName] = row[column].ToString();
                                }
                                dataList.Add(dict);
                            }

                            dimensionDict.Add(mastervalue, dataList);
                        }
                    }
                }

                masterValuesDictionary.Add(dimension.Key, dimensionDict);
            }





            return View(masterValuesDictionary);
        }
        public class SavedDataFilterValues
        {
            public string DIMENSION_TABLE { get; set; }
            public string MASTER_NAME { get; set; }
            public List<FilterValueItem> FILTER_VALUE_List { get; set; }
        }
        public struct FilterValueItem
        {
            public string FILTER_TEXT { get; set; }
            public string FILTER_VALUE { get; set; }
        }

        public JsonResult GetSavedFilterValues()
        {
            SPONGE_Context sPONGE_Context = new();
            var configID = TempData["ConfigId"];

            //To fetch and show the values from the SPG_CONFIG_FILTER_VALUE
            if (configID != null)
            {
                var savedData = sPONGE_Context.SPG_CONFIG_FILTERS_VALUE
                                .Where(x => x.CONFIG_ID == (int)configID)
                                .GroupBy(x => new { x.DIMENSION_TABLE, x.MASTER_NAME })
                                .Select(g => new SavedDataFilterValues
                                {
                                    DIMENSION_TABLE = g.Key.DIMENSION_TABLE,
                                    MASTER_NAME = sPONGE_Context.SPG_MPP_MASTER
                                                  .Where(x => x.MASTER_NAME == g.Key.MASTER_NAME)
                                                  .Select(x => x.MASTER_DISPLAY_NAME ).FirstOrDefault(),
                                    FILTER_VALUE_List = g.Select(x => new FilterValueItem { FILTER_TEXT = x.FILTER_TEXT, FILTER_VALUE = x.FILTER_VALUE }).ToList()
                                }).ToList();
                return Json(savedData);
            }
            else
            {
                return Json(new List<SavedDataFilterValues>());
            }
        }

        [HttpPost]
        public IActionResult SaveSelectedValues(string data, int configId)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            Dictionary<string, Dictionary<string, List<object>>> selectedValues =
                                                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<object>>>>(data);
            SPONGE_Context _context = new();
            var newEntities = new List<SPG_CONFIG_FILTERS_VALUE>();
            // To delete existing data from the table

            var dataToDelete = _context.SPG_CONFIG_FILTERS_VALUE.Where(item => item.CONFIG_ID == configId).ToList();
            if(dataToDelete.Count > 0)
            {
                _context.SPG_CONFIG_FILTERS_VALUE.RemoveRange(dataToDelete);
                _context.SaveChanges();
            }
            
            foreach (var dimension in selectedValues)
            {
                var dimensionCode = _context.SPG_MPP_MASTER.Where(x => x.MPP_DIMENSION_NAME == dimension.Key)
                                      .Select(x => x.DIMENSION_TABLE).FirstOrDefault();
                foreach (var master in dimension.Value)
                {
                    var mastercode = _context.SPG_MPP_MASTER.Where(x => x.MASTER_DISPLAY_NAME == master.Key && x.MPP_DIMENSION_NAME == dimension.Key)
                                      .Select(x => x.MASTER_NAME).FirstOrDefault();
                    foreach (var item in master.Value)
                    {
                        // Convert the item to a dictionary
                        var values = item as Newtonsoft.Json.Linq.JObject;

                        // Access the 'value' and 'text'
                        var value = values["value"].ToString();
                        var text = values["text"].ToString();

                        if (!_context.SPG_CONFIG_FILTERS_VALUE.Any(x => (x.FILTER_VALUE == value || x.FILTER_TEXT == text) && x.CONFIG_ID == configId))
                        {
                            var entity = new SPG_CONFIG_FILTERS_VALUE
                            {
                                DIMENSION_TABLE = dimensionCode,
                                MASTER_NAME = mastercode,
                                FILTER_TEXT = text, // save the text
                                FILTER_VALUE = value, // save the value
                                CONFIG_ID = configId,
                                CREATED_ON = DateTime.Now,
                                CREATED_BY = userName[1]
                            };
                            newEntities.Add(entity);
                        }
                    }
                }
            }
            try
            {
                _context.SPG_CONFIG_FILTERS_VALUE.AddRange(newEntities);
                _context.SaveChanges();

                var test = _context.Procedures.SP_UPDATEWHERECLAUSE_WITHCONFIGIDAsync(configId);
            }
            catch (Exception e)
            {
                ErrorLog srsEx = new();
                srsEx.LogErrorInTextFile(e);
                throw;
            }

            return Json(new { Success = true });
        }
        //#region Filtration
        //public ActionResult GetAllDimensions(int id, int subjectAreaId, string UserName)
        //{
        //    PortalModelDev dp = new PortalModelDev();
        //    SubjectAreaMapping samp = new SubjectAreaMapping();
        //    int editCondition = 0;
        //    string SubAreaName = dp.EP_SUBJECTAREA.Where(s => s.SUBJECTAREA_ID == subjectAreaId).FirstOrDefault().SUBJECTAREA_NAME;
        //    var u = (from eddm in dp.EP_CONFIG_FILTERS
        //             where eddm.CONFIG_ID == id
        //             where eddm.ACTIVE_FLAG == "Y"
        //             select new { DIMENSIONCODE = eddm.DIMENSION_CODE }).Distinct().ToList();
        //    var uv = (from ecf in dp.EP_CONFIG_FILTERS
        //              join eddm in dp.EP_DW_DIMENSION_MAPPING on ecf.DIMENSION_CODE equals eddm.DIMENSION_CODE
        //              where ecf.CONFIG_ID == id
        //              where ecf.ACTIVE_FLAG == "Y"
        //              select (eddm.DIMENSION_NAME)).Distinct().ToList();
        //    var uuv = (from eddm in dp.EP_CONFIG_FILTERS
        //               where eddm.CONFIG_ID == id
        //               where eddm.ACTIVE_FLAG == "Y"
        //               select (eddm.DIMENSION_CODE)).Distinct().ToList();
        //    if (u.Count == 0)
        //    {

        //        List<SelectListItem> items = new List<SelectListItem>();
        //        var v = (from eddm in dp.EP_DW_DIMENSION_MAPPING
        //                 join ere in dp.EP_SUBJECT_DIMENSION on eddm.DIMENSION_CODE equals ere.DIMENSION_NAME
        //                 where ere.SUBJECTAREA_ID == subjectAreaId
        //                 select new { DIMENSION_DISPLAY_NAME = eddm.DIMENSION_NAME, DIMENSION_NAME = eddm.DIMENSION_CODE }).Distinct().ToList();

        //        foreach (var ep in v)
        //        {
        //            if (ep.DIMENSION_DISPLAY_NAME != null)
        //            {
        //                SelectListItem lst = new SelectListItem()
        //                {
        //                    Text = ep.DIMENSION_DISPLAY_NAME,
        //                    Value = ep.DIMENSION_NAME.ToString()
        //                };
        //                items.Add(lst);
        //            }
        //        }
        //        samp.DimensionList = items.Distinct().ToList();
        //        samp.configId = id;
        //        samp.subjectAreaId = subjectAreaId;
        //        ViewBag.SelectedDimList = null;
        //        ViewBag.SelectedDimListVal = null;
        //        ViewData["EffectiveFrom"] = DateTime.Now;
        //    }
        //    else
        //    {
        //        var ef = (from pc in dp.POC_CONFIGURATION
        //                  where pc.CONFIG_ID == id
        //                  select new { EffectiveFrom = pc.EFFECTIVE_FROM }).FirstOrDefault();
        //        DateTime eff = ef.EffectiveFrom == null ? DateTime.MinValue : Convert.ToDateTime(ef.EffectiveFrom);

        //        List<SelectListItem> items = new List<SelectListItem>();
        //        var v = (from eddm in dp.EP_DW_DIMENSION_MAPPING
        //                 join ere in dp.EP_SUBJECT_DIMENSION on eddm.DIMENSION_CODE equals ere.DIMENSION_NAME
        //                 where ere.SUBJECTAREA_ID == subjectAreaId
        //                 select new { DIMENSION_DISPLAY_NAME = eddm.DIMENSION_NAME, DIMENSION_NAME = eddm.DIMENSION_CODE }).Distinct().ToList();

        //        foreach (var ep in v)
        //        {
        //            SelectListItem lst = new SelectListItem();

        //            foreach (var eq in u)
        //            {
        //                if (ep.DIMENSION_NAME.ToString() == eq.DIMENSIONCODE)
        //                {
        //                    lst.Text = ep.DIMENSION_DISPLAY_NAME;
        //                    lst.Value = ep.DIMENSION_NAME.ToString();
        //                    lst.Selected = true;
        //                    editCondition = 1;
        //                }
        //                else
        //                {
        //                    lst.Text = ep.DIMENSION_DISPLAY_NAME;
        //                    lst.Value = ep.DIMENSION_NAME.ToString();
        //                }
        //            }
        //            items.Add(lst);
        //        }
        //        samp.DimensionList = items.Distinct().ToList();
        //        samp.configId = id;
        //        samp.subjectAreaId = subjectAreaId;
        //        ViewBag.SelectedDimList = uv;
        //        ViewBag.SelectedDimListVal = uuv;
        //        ViewData["EffectiveFrom"] = eff;
        //    }
        //    ViewData["editCondition"] = editCondition;
        //    ViewData["UserName"] = UserName;
        //    ViewData["SubAreaName"] = SubAreaName;
        //    return View(samp);
        //}
        //public JsonResult GetLevelledDimensionPartialView(string DimCode, int Config_id)
        //{
        //    PortalModelDev db = new PortalModelDev();
        //    List<SelectListItem> items = new List<SelectListItem>();

        //    var u = (from eddm in db.EP_CONFIG_FILTERS
        //             where eddm.CONFIG_ID == Config_id
        //             where eddm.ACTIVE_FLAG == "Y"
        //             where eddm.DIMENSION_CODE == DimCode
        //             select new { DIMCOLUMNCODE = eddm.DIM_COLUMN_CODE }).Distinct().ToList();
        //    var v = db.EP_DW_DIMENSION_MAPPING.Where(o => o.DIMENSION_CODE == DimCode).Select(o => new { DIMENSIONLEVELNAME = o.DIMENSION_LEVEL_NAME, DIMENSIONLEVEL = o.DIMENSION_LEVEL }).Distinct().OrderBy(o => o.DIMENSIONLEVEL);
        //    string DimensionName = db.EP_DW_DIMENSION_MAPPING.Where(o => o.DIMENSION_CODE == DimCode).Select(o => o.DIMENSION_NAME).FirstOrDefault();
        //    if (u.Count == 0)
        //    {
        //        foreach (var ep in v)
        //        {
        //            if (ep.DIMENSIONLEVELNAME != null)
        //            {
        //                SelectListItem lst = new SelectListItem()
        //                {
        //                    Text = ep.DIMENSIONLEVELNAME,
        //                    Value = ep.DIMENSIONLEVEL.ToString() + "-" + ep.DIMENSIONLEVELNAME.ToString()
        //                };
        //                items.Add(lst);

        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var ep in v)
        //        {
        //            SelectListItem lst = new SelectListItem();
        //            foreach (var eq in u)
        //            {
        //                if (ep.DIMENSIONLEVELNAME.ToString() == eq.DIMCOLUMNCODE)
        //                {
        //                    lst.Text = ep.DIMENSIONLEVELNAME;
        //                    lst.Value = ep.DIMENSIONLEVEL.ToString() + "-" + ep.DIMENSIONLEVELNAME.ToString();
        //                    lst.Selected = true;
        //                }
        //                else
        //                {
        //                    lst.Text = ep.DIMENSIONLEVELNAME;
        //                    lst.Value = ep.DIMENSIONLEVEL.ToString() + "-" + ep.DIMENSIONLEVELNAME.ToString();
        //                }
        //            }
        //            items.Add(lst);
        //        }
        //    }
        //    return Json(new { items = items.Distinct().ToList(), DimensionName = DimensionName }, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult SaveLevelledDimensionData(string DIMVAL)
        //{
        //    PortalModelDev objModel = new PortalModelDev();
        //    List<FilterationValues> Items = JsonConvert.DeserializeObject<List<FilterationValues>>(DIMVAL);
        //    EP_CONFIG_FILTERS config_filter = new EP_CONFIG_FILTERS();
        //    POC_CONFIGURATION poc_config = new POC_CONFIGURATION();
        //    List<decimal> configfilteridval = new List<decimal>();
        //    decimal conffilval = 0;
        //    decimal Configid = 0;
        //    DateTime effectivefrom = DateTime.Now;
        //    string returnVal = string.Empty;
        //    try
        //    {
        //        foreach (var item in Items)
        //        {
        //            Configid = item.ConfigID;
        //            var u = (from eddm in objModel.EP_CONFIG_FILTERS
        //                     where eddm.CONFIG_ID == Configid
        //                     where eddm.ACTIVE_FLAG == "Y"
        //                     select new { DIMCOLUMNCODE = eddm.DIM_COLUMN_CODE, DIMCODE = eddm.DIMENSION_CODE, LEVEL = eddm.DIM_COLUMN_LEVEL, CONFIGFILTERID = eddm.CONFIG_FILTER_ID }).ToList();

        //            effectivefrom = Convert.ToDateTime(item.EffectiveFrom);
        //            for (int i = 0; i < item.DimensionName.Length; i++)
        //            {
        //                var uv = u.Where(s => s.DIMCODE == item.DimensionName[i]).Select(s => s.LEVEL + "-" + s.DIMCOLUMNCODE).ToList();
        //                if (uv.Count == 0)
        //                {
        //                    for (int j = 0; j < item.SubDimensionName[i].Length; j++)
        //                    {
        //                        config_filter.DIMENSION_CODE = item.DimensionName[i];
        //                        config_filter.ACTIVE_FLAG = "Y";
        //                        config_filter.CONFIG_ID = item.ConfigID;
        //                        config_filter.CREATED_BY = Session["UserName"].ToString();
        //                        config_filter.CREATED_ON = DateTime.Now;
        //                        config_filter.DIM_COLUMN_CODE = item.SubDimensionName[i][j].Split('-')[1];
        //                        config_filter.DIM_COLUMN_LEVEL = Convert.ToInt32(item.SubDimensionName[i][j].Split('-')[0]);
        //                        objModel.EP_CONFIG_FILTERS.Add(config_filter);
        //                        objModel.SaveChanges();
        //                    }
        //                }
        //                else if (item.SubDimensionName[i] == null)
        //                {
        //                    for (int k = 0; k < uv.Count; k++)
        //                    {
        //                        string DimCde = item.DimensionName[i];
        //                        string colcode = uv[k].Split('-')[1];
        //                        decimal levelcode = Convert.ToDecimal(uv[k].Split('-')[0]);
        //                        var DCCode = objModel.EP_CONFIG_FILTERS.Where(s => s.DIMENSION_CODE == DimCde && s.DIM_COLUMN_CODE == colcode && s.CONFIG_ID == item.ConfigID && s.DIM_COLUMN_LEVEL == levelcode && s.ACTIVE_FLAG == "Y").SingleOrDefault();
        //                        DCCode.ACTIVE_FLAG = "N";
        //                        objModel.SaveChanges();
        //                        var CFValues = objModel.EP_CONFIG_FILTERS_VALUE.Where(s => s.CONFIG_FILTER_ID == DCCode.CONFIG_FILTER_ID).ToList();
        //                        CFValues.ForEach(m => m.ACTIVE_FLAG = "N");
        //                        objModel.SaveChanges();
        //                    }
        //                }
        //                else
        //                {
        //                    var differedLIst_increased = item.SubDimensionName[i].Except(uv).ToList();
        //                    var differedLIst_reduced = uv.Except(item.SubDimensionName[i]).ToList();
        //                    var differedLIst_common = item.SubDimensionName[i].Intersect(uv).ToList();
        //                    for (int k = 0; k < differedLIst_reduced.Count; k++)
        //                    {
        //                        string DimCde = item.DimensionName[i];
        //                        string colcode = differedLIst_reduced[k].Split('-')[1];
        //                        decimal levelcode = Convert.ToDecimal(differedLIst_reduced[k].Split('-')[0]);
        //                        var DCCode = objModel.EP_CONFIG_FILTERS.Where(s => s.DIMENSION_CODE == DimCde && s.DIM_COLUMN_CODE == colcode && s.CONFIG_ID == item.ConfigID && s.DIM_COLUMN_LEVEL == levelcode && s.ACTIVE_FLAG == "Y").SingleOrDefault();
        //                        DCCode.ACTIVE_FLAG = "N";
        //                        objModel.SaveChanges();
        //                        var CFValues = objModel.EP_CONFIG_FILTERS_VALUE.Where(s => s.CONFIG_FILTER_ID == DCCode.CONFIG_FILTER_ID).ToList();
        //                        CFValues.ForEach(m => m.ACTIVE_FLAG = "N");
        //                        objModel.SaveChanges();
        //                    }
        //                    for (int k = 0; k < differedLIst_increased.Count; k++)
        //                    {
        //                        config_filter.DIMENSION_CODE = item.DimensionName[i];
        //                        config_filter.ACTIVE_FLAG = "Y";
        //                        config_filter.CONFIG_ID = item.ConfigID;
        //                        config_filter.CREATED_BY = Session["UserName"].ToString();
        //                        config_filter.CREATED_ON = DateTime.Now;
        //                        config_filter.DIM_COLUMN_CODE = differedLIst_increased[k].Split('-')[1];
        //                        config_filter.DIM_COLUMN_LEVEL = Convert.ToInt32(differedLIst_increased[k].Split('-')[0]);
        //                        objModel.EP_CONFIG_FILTERS.Add(config_filter);
        //                        objModel.SaveChanges();
        //                        conffilval = config_filter.CONFIG_FILTER_ID;
        //                        configfilteridval.Add(conffilval);
        //                    }
        //                    for (int ij = 0; ij < differedLIst_common.Count(); ij++)
        //                    {
        //                        string DimCde = item.DimensionName[i];
        //                        string colcode = differedLIst_common[ij].Split('-')[1];
        //                        decimal levelcode = Convert.ToDecimal(differedLIst_common[ij].Split('-')[0]);
        //                        var DCCode = objModel.EP_CONFIG_FILTERS.Where(s => s.DIMENSION_CODE == DimCde && s.DIM_COLUMN_CODE == colcode && s.CONFIG_ID == item.ConfigID && s.DIM_COLUMN_LEVEL == levelcode && s.ACTIVE_FLAG == "Y").SingleOrDefault();
        //                        conffilval = DCCode.CONFIG_FILTER_ID;
        //                        configfilteridval.Add(conffilval);
        //                    }

        //                }
        //            }
        //        }
        //        var config = objModel.POC_CONFIGURATION.Where(s => s.CONFIG_ID == Configid).SingleOrDefault();
        //        config.EFFECTIVE_FROM = effectivefrom;
        //        config.EFFECTIVE_TO = Convert.ToDateTime("12/31/9999");
        //        objModel.SaveChanges();
        //        returnVal = "true";
        //    }
        //    catch (Exception ex)
        //    {
        //        returnVal = "false";
        //    }
        //    return Json(new { returnVal = returnVal, ConfigFilterIDVals = configfilteridval }, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult UserFilteration(int Config_Id, int subjectAreaId, List<string> ConfigFilterId_List)
        //{
        //    PortalModelDev db = new PortalModelDev();
        //    UserSecurity model = new UserSecurity();
        //    List<FiltrationEdit> FiltrationEditList = new List<FiltrationEdit>();
        //    string[] cl_count = (ConfigFilterId_List[0].Trim('[').Trim(']')).Split(',');
        //    if (cl_count[0] != "")
        //    {
        //        for (int i = 0; i < cl_count.Length; i++)
        //        {
        //            decimal confilid = Convert.ToDecimal(cl_count[i]);
        //            var configfiltervalue = db.EP_CONFIG_FILTERS_VALUE.Where(s => s.CONFIG_FILTER_ID == confilid).ToList();
        //            foreach (var item in configfiltervalue)
        //            {
        //                FiltrationEdit fe = new FiltrationEdit();
        //                fe.ACTIVE_FLAG = item.ACTIVE_FLAG;
        //                fe.CONFIG_FILTER_ID = item.CONFIG_FILTER_ID;
        //                fe.CONFIG_FILTER_VAL_ID = item.CONFIG_FILTER_VAL_ID;
        //                fe.CREATED_BY = item.CREATED_BY;
        //                fe.CREATED_ON = item.CREATED_ON;
        //                fe.FILTER_VALUE = item.FILTER_VALUE;
        //                fe.FILTER_TEXT = item.FILTER_TEXT;
        //                fe.MODIFIED_BY = item.MODIFIED_BY;
        //                fe.MODIFIED_ON = Convert.ToDateTime(item.MODIFIED_ON);
        //                fe.FLAG_CODE = item.FLAG_CODE;
        //                fe.FLAG_NAME = item.FLAG_NAME;
        //                fe.DIMENSION_NAME = item.DIMENSION_NAME;
        //                FiltrationEditList.Add(fe);
        //            }

        //        }
        //    }
        //    string dynamicsqlquery = "SELECT DISTINCT * FROM(";
        //    string LinkTableNale = "";
        //    var ResultRole = (from ecf in db.EP_CONFIG_FILTERS
        //                      join eddm in db.EP_DW_DIMENSION_MAPPING on ecf.DIM_COLUMN_CODE equals eddm.DIMENSION_LEVEL_NAME
        //                      where ecf.CONFIG_ID == Config_Id
        //                      where ecf.ACTIVE_FLAG == "Y"
        //                      select new { DimensionName = ecf.DIMENSION_CODE, Level = ecf.DIM_COLUMN_LEVEL, Flag = eddm.DIM_COLUMN_CODE, DimensionLevelName = eddm.DIMENSION_LEVEL_NAME, ConfigFilterId = ecf.CONFIG_FILTER_ID, configID = ecf.CONFIG_ID }).Distinct().OrderBy(o => o.DimensionLevelName).ThenBy(o => o.ConfigFilterId).ToList();
        //    foreach (var DIM in ResultRole)
        //    {
        //        try
        //        {
        //            //string linkservername = "QA_EBIDW.";
        //            //string LINK_DWD = "@LINK_DWD";
        //            LinkTableNale = LinkServerName + DIM.DimensionName.ToString() + LINK_DWD;
        //            var Dim = db.EP_DW_DIMENSION_MAPPING.Where(m => m.DIMENSION_CODE == DIM.DimensionName && m.DIMENSION_LEVEL_NAME == DIM.DimensionLevelName).Select(o => new { DIMENSION_CODE = o.DIMENSION_CODE, DIM_COLUMN_CODE = o.DIM_COLUMN_CODE, DIMENSION_LEVEL_NAME = o.DIMENSION_LEVEL_NAME }).OrderBy(o => o.DIM_COLUMN_CODE).ToList();
        //            string[] ArrDimColumnNames = Dim.Select(x => x.DIM_COLUMN_CODE).ToArray();
        //            dynamicsqlquery += "select distinct ";
        //            dynamicsqlquery += "'" + ArrDimColumnNames[1] + "' as ColumnTextName, '" + ArrDimColumnNames[0] + "' as Flag, TO_NUMBER(" + DIM.Level + ") as LevelOrder,'" + DIM.DimensionName.ToString() + "'  as DIMENSION, '" + Dim[0].DIMENSION_LEVEL_NAME + "' as DIMENSIONLEVELNAME, " + DIM.ConfigFilterId + " as CONFIGFILTERID, " + DIM.configID + " as CONFIGID from " + LinkTableNale + " union All ";
        //        }
        //        catch (Exception ex)
        //        {
        //            string ex1 = Convert.ToString(ex);
        //            return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    if (!string.IsNullOrEmpty(dynamicsqlquery) && dynamicsqlquery.Length > 10)
        //    {
        //        dynamicsqlquery = dynamicsqlquery.Substring(0, dynamicsqlquery.Length - 10);
        //        dynamicsqlquery += ") t ORDER BY t.DIMENSION ASC, t.LevelOrder ASC";
        //    }

        //    string FormedQueryLookupType = "SP_GETDIMENSION_DETAILS";
        //    DataSet ds2 = new DataSet();
        //    using (GetDataSet objGetDataSetValue = new GetDataSet())
        //    {

        //        try
        //        {
        //            ds2 = objGetDataSetValue.GetDataSetValueStringParam(FormedQueryLookupType, dynamicsqlquery);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogError lgerr = new LogError();
        //            lgerr.LogErrorInTextFile(ex);
        //            SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name:" + FormedQueryLookupType + " and dynamic SQL query:" + dynamicsqlquery + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);

        //            return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    DynamicControl dn = new DynamicControl();
        //    List<DynamicControl> ListofDynamicControl = new List<DynamicControl>();
        //    var Dimension = model.ListofDynamicControl = (from DataRow row in ds2.Tables[0].Rows
        //                                                  select new DynamicControl()
        //                                                  {
        //                                                      Dimension = row.Field<string>("DIMENSION")
        //                                                  }).OrderBy(x => x.Level).ToList();
        //    var DimensionObject = Dimension.Select(s => new { DimensionName = s.Dimension }).Distinct().ToList();
        //    model.ListofDynamicControl = (from DataRow row in ds2.Tables[0].Rows
        //                                  select new DynamicControl()
        //                                  {
        //                                      Flag = row["FLAG"].ToString(),
        //                                      Level = row.Field<decimal?>("LEVELORDER"),
        //                                      Dimension = row.Field<string>("DIMENSION"),
        //                                      DimensionLevelName = row.Field<string>("DIMENSIONLEVELNAME"),
        //                                      configFilterId = Convert.ToInt32(row["CONFIGFILTERID"]),
        //                                      configID = Convert.ToInt32(row["CONFIGID"]),
        //                                      columntextname = Convert.ToString(row["ColumnTextName"])
        //                                  }).OrderBy(x => x.Level).ToList();
        //    var SubDimensionObjectlist = model.ListofDynamicControl.GroupBy(g => g.Level).Select(grp => grp.ToList()).ToList();
        //    return Json(new { DimensionObject = DimensionObject, SubDimensionListJq = SubDimensionObjectlist, EditFilteredList = FiltrationEditList }, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult FillFirstDimensionControls(string DimensionName, decimal? levelval, string Flag, string DimensionLevelName, int ConfigFilterId, string FilteredEditList, decimal? Nextlevel)
        //{
        //    List<FiltrationEdit> Items = JsonConvert.DeserializeObject<List<FiltrationEdit>>(FilteredEditList);
        //    string LinkTableName = ""; string dynamicsqlquery1 = "";
        //    string FormedQueryLookupType = "SP_GETDIMENSION_DETAILS";
        //    foreach (var confil in Items)
        //    {
        //        LinkTableName = LinkServerName + confil.DIMENSION_NAME.ToString() + LINK_DWD;
        //        dynamicsqlquery1 = "SELECT DISTINCT " + confil.FLAG_NAME + " as FILTER_TEXT FROM " + LinkTableName + " WHERE " + confil.DIMENSION_NAME + "." + confil.FLAG_CODE + " = '" + confil.FILTER_VALUE + "'";
        //        FormedQueryLookupType = "SP_GETDIMENSION_DETAILS";
        //        DataSet datasetDimension1 = new DataSet();
        //        using (GetDataSet objGetDataSetValue = new GetDataSet())
        //        {
        //            try
        //            {
        //                datasetDimension1 = objGetDataSetValue.GetDataSetValueStringParam(FormedQueryLookupType, dynamicsqlquery1);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogError lgerr = new LogError();
        //                lgerr.LogErrorInTextFile(ex);
        //                SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name:" + FormedQueryLookupType + " and dynamic SQL query:" + dynamicsqlquery1 + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        try
        //        {
        //            confil.FILTER_TEXT = Convert.ToString(datasetDimension1.Tables[0].Rows[0]["FILTER_TEXT"]);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogError lgerr = new LogError();
        //            lgerr.LogErrorInTextFile(ex);
        //            SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name:" + FormedQueryLookupType + " and dynamic SQL query:" + dynamicsqlquery1 + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //            return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //        }
        //    }

        //    PortalModelDev db = new PortalModelDev();
        //    string dynamicsqlquery = "";
        //    string LinkTableNale = "";
        //    //string linkservername = "QA_EBIDW.";
        //    //string LINK_DWD = "@LINK_DWD";
        //    string[] ArrDimColumnNames = new string[2];
        //    try
        //    {
        //        LinkTableNale = LinkServerName + DimensionName.ToString() + LINK_DWD;
        //        ArrDimColumnNames = db.EP_DW_DIMENSION_MAPPING.Where(w => w.DIMENSION_CODE == DimensionName && w.DIMENSION_LEVEL == levelval && w.DIMENSION_LEVEL_NAME == DimensionLevelName).OrderBy(w => w.DIM_COLUMN_CODE).Select(s => s.DIM_COLUMN_CODE).ToArray();// Dim.Select(x => x.DIM_COLUMN_CODE).ToArray();
        //        dynamicsqlquery = "select distinct " + ArrDimColumnNames[0] + " as VAL," + ArrDimColumnNames[1] + " as TEXT from " + LinkTableNale + " where " + ArrDimColumnNames[0] + " is not null and " + ArrDimColumnNames[1] + " is not null order by " + ArrDimColumnNames[1];
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError lgerr = new LogError();
        //        lgerr.LogErrorInTextFile(ex);
        //        SentErrorMail.SentEmailtoError("Error on Filteration.see the below details:  dynamic SQL query:" + dynamicsqlquery + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //        return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //    }
        //    DataSet datasetDimension = new DataSet();
        //    using (GetDataSet objGetDataSetValue = new GetDataSet())
        //    {
        //        try
        //        {
        //            datasetDimension = objGetDataSetValue.GetDataSetValueStringParam(FormedQueryLookupType, dynamicsqlquery);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogError lgerr = new LogError();
        //            lgerr.LogErrorInTextFile(ex);
        //            SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name:" + FormedQueryLookupType + " and dynamic SQL query:" + dynamicsqlquery + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //            return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    var Dim_List = datasetDimension.Tables[0].AsEnumerable().Select(dataRow => new { TEXT = dataRow.Field<string>("TEXT"), VAL = dataRow.Field<string>("VAL") }).ToList();
        //    var itemsList = Items.Select(s => new { TEXT = s.FILTER_TEXT, VAL = s.FILTER_VALUE }).ToList();
        //    var initialFiltered_List = Dim_List.Except(itemsList).ToList().Count > 0 ? Dim_List.Except(itemsList).ToList() : Dim_List;
        //    var userSelectedList = Dim_List.Intersect(itemsList).ToList();
        //    JsonSerializerSettings jss = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        //    var result = JsonConvert.SerializeObject(initialFiltered_List, Formatting.None, jss);
        //    var result1 = JsonConvert.SerializeObject(userSelectedList, Formatting.None, jss);
        //    var result2 = JsonConvert.SerializeObject(Items, Formatting.None, jss);
        //    return Json(new { DimensionList = result, EditFilteredList = result2, Controlid = DimensionName + "-" + ArrDimColumnNames[0], ConfigFilterID = ConfigFilterId, SelectedList = result1, DIMName = DimensionName, FLAGName = Flag, LevelVal = Nextlevel }, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult FillDimensionControls(string DimensionName, decimal? levelval, string Flag, string Text, string Val, string FiltLIst, string ColumnTextName)
        //{
        //    List<FiltrationEdit> Items = FiltLIst == "" ? new List<FiltrationEdit>() : JsonConvert.DeserializeObject<List<FiltrationEdit>>(FiltLIst);
        //    string LinkTableName = ""; string dynamicsqlquery1 = "";
        //    string FormedQueryLookupType = "SP_GETDIMENSION_DETAILS";
        //    foreach (var confil in Items)
        //    {
        //        LinkTableName = LinkServerName + confil.DIMENSION_NAME.ToString() + LINK_DWD;
        //        dynamicsqlquery1 = "SELECT DISTINCT " + confil.FLAG_NAME + " as FILTER_TEXT FROM " + LinkTableName + " WHERE " + confil.DIMENSION_NAME + "." + confil.FLAG_CODE + " = '" + confil.FILTER_VALUE + "'";
        //        FormedQueryLookupType = "SP_GETDIMENSION_DETAILS";
        //        DataSet datasetDimension1 = new DataSet();
        //        using (GetDataSet objGetDataSetValue = new GetDataSet())
        //        {
        //            try
        //            {
        //                datasetDimension1 = objGetDataSetValue.GetDataSetValueStringParam(FormedQueryLookupType, dynamicsqlquery1);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogError lgerr = new LogError();
        //                lgerr.LogErrorInTextFile(ex);
        //                SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name:" + FormedQueryLookupType + " and dynamic SQL query:" + dynamicsqlquery1 + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        confil.FILTER_TEXT = Convert.ToString(datasetDimension1.Tables[0].Rows[0]["FILTER_TEXT"]);
        //    }
        //    PortalModelDev db = new PortalModelDev();
        //    string dynamicsqlquery = "";
        //    string LinkTableNale = "";
        //    //string linkservername = "QA_EBIDW.";
        //    //string LINK_DWD = "@LINK_DWD";
        //    string[] ArrDimColumnNames = new string[2];
        //    try
        //    {
        //        LinkTableNale = LinkServerName + DimensionName.ToString() + LINK_DWD;
        //        ArrDimColumnNames = db.EP_DW_DIMENSION_MAPPING.Where(w => w.DIMENSION_CODE == DimensionName && w.DIMENSION_LEVEL == levelval).OrderBy(w => w.DIM_COLUMN_CODE).Select(s => s.DIM_COLUMN_CODE).ToArray();// Dim.Select(x => x.DIM_COLUMN_CODE).ToArray();
        //        dynamicsqlquery = "select distinct " + ArrDimColumnNames[0] + " as VAL," + ArrDimColumnNames[1] + " as TEXT from " + LinkTableNale + " where " + Flag + " in('" + Val.Replace("[\"", "").Replace("\"]", "").Replace("\"", "'") + "') and " + ArrDimColumnNames[0] + " is not null and " + ArrDimColumnNames[1] + " is not null order by " + ArrDimColumnNames[1];
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError lgerr = new LogError();
        //        lgerr.LogErrorInTextFile(ex);
        //        SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name:" + FormedQueryLookupType + " and dynamic SQL query:" + dynamicsqlquery + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //        return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //    }
        //    DataSet datasetDimension = new DataSet();
        //    using (GetDataSet objGetDataSetValue = new GetDataSet())
        //    {
        //        try
        //        {
        //            datasetDimension = objGetDataSetValue.GetDataSetValueStringParam(FormedQueryLookupType, dynamicsqlquery);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogError lgerr = new LogError();
        //            lgerr.LogErrorInTextFile(ex);
        //            SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name:" + FormedQueryLookupType + " and dynamic SQL query:" + dynamicsqlquery + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //            return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    var Dim_List = datasetDimension.Tables[0].AsEnumerable().Select(dataRow => new { TEXT = dataRow.Field<string>("TEXT"), VAL = dataRow.Field<string>("VAL") }).ToList();
        //    var itemsList = (Items == null) ? null : Items.Select(s => new { TEXT = s.FILTER_TEXT, VAL = s.FILTER_VALUE }).ToList();
        //    var initialFiltered_List = (itemsList == null) ? Dim_List : (Dim_List.Except(itemsList).ToList().Count > 0 ? Dim_List.Except(itemsList).ToList() : Dim_List);
        //    var userSelectedList = (itemsList == null) ? null : Dim_List.Intersect(itemsList).ToList();
        //    JsonSerializerSettings jss = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        //    var result = JsonConvert.SerializeObject((initialFiltered_List.Count == userSelectedList.Count ? null : initialFiltered_List), Formatting.None, jss);
        //    var result1 = JsonConvert.SerializeObject(userSelectedList, Formatting.None, jss);
        //    return Json(new { DimensionList = result, SelectedList = result1, Controlid = DimensionName + "-" + ArrDimColumnNames[0], DIMName = DimensionName, FLAGName = ArrDimColumnNames[0], filtrationList = FiltLIst }, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult FillPreviousDimensionControls(string DimensionName, decimal? levelval, string Flag, string Text, string Val)
        //{
        //    PortalModelDev db = new PortalModelDev();
        //    string dynamicsqlquery = "";

        //    string LinkTableNale = "";
        //    //string linkservername = "QA_EBIDW.";
        //    //string LINK_DWD = "@LINK_DWD";
        //    string[] ArrDimColumnNames = new string[2];
        //    try
        //    {
        //        LinkTableNale = LinkServerName + DimensionName.ToString() + LINK_DWD;
        //        ArrDimColumnNames = db.EP_DW_DIMENSION_MAPPING.Where(w => w.DIMENSION_CODE == DimensionName && w.DIMENSION_LEVEL == levelval).OrderBy(w => w.DIM_COLUMN_CODE).Select(s => s.DIM_COLUMN_CODE).ToArray();// Dim.Select(x => x.DIM_COLUMN_CODE).ToArray();
        //        dynamicsqlquery = "select distinct " + ArrDimColumnNames[0] + " as VAL," + ArrDimColumnNames[1] + " as TEXT from " + LinkTableNale + " where " + Flag + " in('" + Val.Replace("[\"", "").Replace("\"]", "").Replace("\"", "'") + "') and " + ArrDimColumnNames[0] + " is not null and " + ArrDimColumnNames[1] + " is not null order by " + ArrDimColumnNames[1];
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError lgerr = new LogError();
        //        lgerr.LogErrorInTextFile(ex);
        //        SentErrorMail.SentEmailtoError("Error on Filteration.see the below details:dynamic SQL query:" + dynamicsqlquery + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //        return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //    }
        //    string FormedQueryLookupType = "SP_GETDIMENSION_DETAILS";
        //    DataSet datasetDimension = new DataSet();
        //    using (GetDataSet objGetDataSetValue = new GetDataSet())
        //    {
        //        try
        //        {
        //            datasetDimension = objGetDataSetValue.GetDataSetValueStringParam(FormedQueryLookupType, dynamicsqlquery);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogError lgerr = new LogError();
        //            lgerr.LogErrorInTextFile(ex);
        //            SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name " + FormedQueryLookupType + " And dynamic SQL query:" + dynamicsqlquery + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //            return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    JsonSerializerSettings jss = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        //    var result = JsonConvert.SerializeObject(datasetDimension, Formatting.None, jss);
        //    return Json(new { DimensionList = result, Controlid = DimensionName + "-" + ArrDimColumnNames[0], nextflag = ArrDimColumnNames[0] }, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult SaveFilteredValues(string DIMVAL)
        //{
        //    PortalModelDev objModel = new PortalModelDev();
        //    List<ConfigFilterValues> Items = JsonConvert.DeserializeObject<List<ConfigFilterValues>>(DIMVAL);
        //    EP_CONFIG_FILTERS_VALUE config_filter_values = new EP_CONFIG_FILTERS_VALUE();
        //    string returnVal = string.Empty; int ConfigurationID = 0; string whereclause = "WHERE ";
        //    string dynamicQuery = "DELETE FROM EP_CONFIG_FILTERS_VALUE WHERE CONFIG_FILTER_ID IN(";
        //    List<FiltrationEdit> filtrationlist = new List<FiltrationEdit>();
        //    try
        //    {
        //        if (Items[0].FiltrationList != null && Items[0].FiltrationList != "")
        //        {
        //            filtrationlist = JsonConvert.DeserializeObject<List<FiltrationEdit>>(Items[0].FiltrationList);
        //            for (int i = 0; i < filtrationlist.Count; i++)
        //            {
        //                dynamicQuery += filtrationlist[i].CONFIG_FILTER_ID;
        //                dynamicQuery = dynamicQuery.Substring(0, dynamicQuery.Length - 2);
        //                dynamicQuery += ", ";
        //            }
        //            dynamicQuery = dynamicQuery.Substring(0, dynamicQuery.Length - 2);
        //            dynamicQuery += ")";
        //            string FormedQueryLookupType = "SP_DELETE_DETAILS";
        //            GetDataSet objDeleteSetValue = new GetDataSet();
        //            objDeleteSetValue.DeleteUsingDynamicQuery(FormedQueryLookupType, dynamicQuery.ToString());
        //        }
        //        foreach (var item in Items)
        //        {
        //            if (item.filterValue != null)
        //            {
        //                whereclause += item.dimenname + "." + item.flagname + " IN (";
        //                for (int i = 0; i < item.filterValue.Length; i++)
        //                {
        //                    whereclause += "'" + item.filterValue[i];
        //                    ConfigurationID = item.configID;
        //                    config_filter_values.CONFIG_FILTER_ID = item.configfilterid;
        //                    config_filter_values.FILTER_TEXT = null;
        //                    config_filter_values.FILTER_VALUE = item.filterValue[i];
        //                    config_filter_values.ACTIVE_FLAG = "Y";
        //                    config_filter_values.CREATED_BY = Session["UserName"].ToString();
        //                    config_filter_values.CREATED_ON = DateTime.Now;
        //                    config_filter_values.FLAG_CODE = item.flagname;
        //                    config_filter_values.FLAG_NAME = item.columnTextName;
        //                    config_filter_values.DIMENSION_NAME = item.dimenname;
        //                    objModel.EP_CONFIG_FILTERS_VALUE.Add(config_filter_values);
        //                    objModel.SaveChanges();
        //                    whereclause += "',";
        //                }

        //            }
        //            whereclause = whereclause.Substring(0, whereclause.Length - 1);
        //            whereclause += (item.filterValue != null) ? ") and " : " ";
        //        }
        //        whereclause = whereclause.Substring(0, whereclause.Length - 4);
        //        var config = objModel.POC_CONFIGURATION.Where(s => s.CONFIG_ID == ConfigurationID).SingleOrDefault();
        //        config.WHERECLAUSE = whereclause;
        //        objModel.SaveChanges();

        //        //Need the procedure in QA environment, currently its in Dev.
        //        //decimal config_id = 242;
        //        //string whereclause = "WHERE V_DIM_MFG_ENTITY.ZONE_CODE IN ('BEH','East','NSK','North','South','West') and V_DIM_MFG_ENTITY.OPS_UNIT_CD IN ('119AIL','101APM','101AJD','101RNV','101BVS','101BWL','101ALD','101DAU','101GSB','101GDL','101HGB','101KOH','118LLM','101NSB','108NCS','101NVD','101OBL','202PIL','101RKD','101RLP','101RLL','101RDL','101SBB','101SDB','101SBL','101SVD','101SHV','101UBL','101AND','101ADC') and V_DIM_MFG_ENTITY.OPS_WORKCENTER_CD IN ('101ADCL1','101ADCL2','101ADCL4','101ADCL5','101AJDL1','101ALDL1','101ALDL2','101ALDL3','101ALDL4','101ANDL1','101ANDL2','101ANDL3','101ANDL4','101APML1','101APML2','101APML3','101APML4','101APML5','101APML6','101APML7','119AILL5','118LLML1','118LLML2','118LLML3','108NCSL1','108NCSL2','119AILL1','119AILL2','119AILL3','119AILL4','101GDLL10','101GDLL2','101GDLL3','101GDLL4','101GDLL5','101GDLL6','101GDLL7','101GDLL8','101GDLL9','101GSBL1','101GSBL2','101HGBL1','101HGBL2','101NSBL1','101NSBL2','101NSBL3','101NSBL4','101NSBL5','101NSBL6','101NSBL7','101NSBL8','101SVDL1','101SVDL2','101SVDL3','101NVDL1','101NVDL2','101NVDL3','101NVDL4','101NVDL5','101UBLL1','101UBLL2','101BVSL1','101BVSL2','101BVSL3','101OBLL1','101OBLL2','101OBLL3','101UBLL3','101UBLL4','101BVSL4','101BWLL1','101BWLL2','101BWLL3','101BWLL4','101SBBL1','101SBBL2','101SBBL3','101SBLL1','101RDLL1','101RDLL2','101DAUL1','101KOHL1','101KOHL2','101RDLL3','101RKDL1','101RKDL2','101SVDL4','101DAUL2','101DAUL3','101DAUL4','101DAUL5','101RLLL1','101RLLL10','101RLLL11','101RLLL2','101RLLL3','101RLLL4','101RLLL5','101SBLL2','101SBLL3','101SBLL4','101SBLL5','101SDBL1','101SDBL2','101RLLL6','101RLLL7','101RLLL8','101RLLL9','101RLPL1','101RLPL2','101RLPL3','101RLPL4','101RLPL5','101RNVL1','101RNVL2','101RNVL3','101SDBL3','101SHVL1','101GDLL1','202PILL1','202PILL2','202PILL3')and V_MAP_JDESKU.JDE_SKU_CD IN('BPBRQ010DIACVL','BPBRQ010EXPEXP','BPRL010WBLCVL','BPRN010ANPCVL','BPRN010ARPCVL','BPRN010ASSCVL','BPRN010BHRCVL','BPRN010CHDCVL','BPRN010CHTCVL','BPRN010DAMCVL','BPRN010DELCVL','BPRN010DIUCVL','BPRN010DNHCVL','BPRN010GOACVL','BPRN010GUJCVL','BPRN010HARCVL','BPRN010HIMCVL','BPRN010JHACVL','BPRN010JNKCVL','BPRN010KARCVL','BPRN010MAHCVL','BPRN010MDPCVL','BPRN010MEGCVL','BPRN010MHECVL','BPRN010ORICVL','BPRN010PJBCVL','BPRN010PONCVL','BPRN010RAJCVL','BPRN010SKMCVL','BPRN010TLGCVL','BPRN010TRICVL','BPRN010UCHCVL','BPRN010UTPCVL','BPRN010WBLCVL','BPRN010YANCVL','BPRP010ANPCVL','BPRP010ARPCVL','BPRP010ASSCVL','BPRP010BHRCVL','BPRP010CHDCVL','BPRP010CHTCVL','BPRP010DAMCVL','BPRP010DELCVL','BPRP010DIUCVL','BPRP010DNHCVL','BPRP010GOACVL','BPRP010GUJCVL','BPRP010HARCVL','BPRP010HIMCVL','BPRP010JHACVL','BPRP010JNKCVL','BPRP010KARCVL','BPRP010MAHCVL','BPRP010MDPCVL','BPRP010MEGCVL','BPRP010MHECVL','BPRP010MZRCVL','BPRP010ORICVL','BPRP010PJBCVL','BPRP010PONCVL','BPRP010RAJCVL','BPRP010SKMCVL','BPRP010TLGCVL','BPRP010TRICVL','BPRP010UCHCVL','BPRP010UTPCVL','BPRP010WBLCVL','BPRP010YANCVL','BPRQ010ANPCVL','BPRQ010ARPCVL','BPRQ010ASSCVL','BPRQ010BHRCSD','BPRQ010BHRCVL','BPRQ010CHDCVL','BPRQ010CHTCVL','BPRQ010DAMCVL','BPRQ010DELCSD','BPRQ010DELCVL','BPRQ010DIUCVL','BPRQ010DNHCVL','BPRQ010EXPEXP','BPRQ010GOACVL','BPRQ010GUJCVL','BPRQ010HARCVL','BPRQ010HIMCVL','BPRQ010JHACVL','BPRQ010JNKCVL','BPRQ010KARCVL','BPRQ010MAHCVL','BPRQ010MDPCSD','BPRQ010MDPCVL','BPRQ010MEGCVL','BPRQ010MHECVL','BPRQ010MZRCVL','BPRQ010ORICVL','BPRQ010PJBCVL','BPRQ010PONCVL','BPRQ010RAJCVL','BPRQ010SKMCVL','BPRQ010TLGCVL','BPRQ010TRICVL','BPRQ010UCHCSD','BPRQ010UCHCVL','BPRQ010UTPCVL','BPRQ010WBLCVL','BPRQ010YANCVL','BPRT010WBLCVL','BPWD010DAMCVL','BPWD010KARCVL','BPWD010MAHCVL','BPWD010TLGCVL','BPWD010UTPCVL','BPWDSP1ANPCVL','BPWDSP1TLGCVL','BPWDUP1MDPCVL','BPWL010EXPEXP','BPWL010GOACVL','BPWL010TLGCVL','BPWL020DELCVL','BPWL020EXPEXP','BPWL020MAHCVL','BPWL030EXPEXP','BPWLSP1ANPCVL','BPWLSP1TLGCVL','BPWLUP1WBLCVL','BPWM010GOACVL','BPWM010KARCVL','BPWM010MAHCVL','BPWN010ANNCVL','BPWN010ANPCVL','BPWN010ARPCVL','BPWN010ASSCVL','BPWN010BHRCVL','BPWN010CHDCVL','BPWN010DAMCVL','BPWN010DELCVL','BPWN010DIUCVL','BPWN010DNHCVL','BPWN010EXPEXP','BPWN010GENCVL','BPWN010GOACVL','BPWN010GUJCVL','BPWN010HIMCVL','BPWN010JHACVL','BPWN010JNKCVL','BPWN010KARCVL','BPWN010MAHCVL','BPWN010MEGCVL','BPWN010MHECVL','BPWN010MZRCVL','BPWN010ORICVL','BPWN010PJBCVL','BPWN010PONCVL','BPWN010RAJCVL','BPWN010SKMCVL','BPWN010TLGCVL','BPWN010TRICVL','BPWN010UCHCVL','BPWN010UTPCVL','BPWN010YANCVL','BPWN020EXPEXP','BPWN030ANPCVL','BPWNUP1CHTCVL','BPWNUP1HARCVL','BPWNUP1MDPCVL','BPWNUP1ORICVL','BPWNUP1PJBCVL','BPWNUP1WBLCVL','BPWP010ANNCVL','BPWP010ANPCVL','BPWP010ARPCVL','BPWP010ASSCVL','BPWP010BHRCVL','BPWP010CHDCVL','BPWP010DAMCVL','BPWP010DELCVL','BPWP010DIUCVL','BPWP010DNHCVL','BPWP010EXPEXP','BPWP010GENCVL','BPWP010GOACVL','BPWP010GUJCVL','BPWP010HIMCVL','BPWP010JHACVL','BPWP010JNKCVL','BPWP010KARCVL','BPWP010MAHCVL','BPWP010MEGCVL','BPWP010MHECVL','BPWP010MZRCVL','BPWP010ORICVL','BPWP010PJBCVL','BPWP010PONCVL','BPWP010RAJCVL','BPWP010SKMCVL','BPWP010TLGCVL','BPWP010TRICVL','BPWP010UCHCVL','BPWP010UTPCVL','BPWP010YANCVL','BPWP020EXPEXP','BPWP030ANPCVL','BPWPUP1CHTCVL','BPWPUP1HARCVL','BPWPUP1MDPCVL','BPWPUP1ORICVL','BPWPUP1PJBCVL','BPWPUP1WBLCVL','BPWQ010ANNCSD','BPWQ010ANNCVL','BPWQ010ANPCSD','BPWQ010ANPCVL','BPWQ010ARPCVL','BPWQ010ASSBSF','BPWQ010ASSCSD','BPWQ010ASSCVL','BPWQ010ASSPMF','BPWQ010BHRCSD','BPWQ010BHRCVL','BPWQ010BHRPMF','BPWQ010CHDCVL','BPWQ010CHDPMF','BPWQ010CHTCSD','BPWQ010DAMCVL','BPWQ010DELCSD','BPWQ010DELCVL','BPWQ010DELPMF','BPWQ010DIUCVL','BPWQ010DNHCVL','BPWQ010EXPEXP','BPWQ010GENBSF','BPWQ010GENCSD','BPWQ010GENCVL','BPWQ010GOACVL','BPWQ010GUJBSF','BPWQ010GUJCSD','BPWQ010GUJCVL','BPWQ010HARCSD','BPWQ010HARPMF','BPWQ010HIMCSD','BPWQ010HIMCVL','BPWQ010HIMPMF','BPWQ010JHABSF','BPWQ010JHACSD','BPWQ010JHACVL','BPWQ010JHAPMF','BPWQ010JNKBSF','BPWQ010JNKCSD','BPWQ010JNKCVL','BPWQ010KARBSF','BPWQ010KARCSD','BPWQ010KARCVL','BPWQ010KERCSD','BPWQ010MAHCSD','BPWQ010MAHCVL','BPWQ010MDPBSF','BPWQ010MDPCSD','BPWQ010MEGCVL','BPWQ010MHECVL','BPWQ010MNPBSF','BPWQ010MNPPMF','BPWQ010MZRBSF','BPWQ010MZRCVL','BPWQ010MZRPMF','BPWQ010NGLCSD','BPWQ010NGLPMF','BPWQ010ORICSD','BPWQ010ORICVL','BPWQ010PJBBSF','BPWQ010PJBCSD','BPWQ010PJBCVL','BPWQ010PONCVL','BPWQ010RAJCSD','BPWQ010RAJCVL','BPWQ010RAJPMF','BPWQ010SKMCSD','BPWQ010SKMCVL','BPWQ010TLGCSD','BPWQ010TLGCVL','BPWQ010TNDCSD','BPWQ010TRIBSF','BPWQ010TRICVL','BPWQ010UCHCSD','BPWQ010UCHCVL','BPWQ010UTPCSD','BPWQ010UTPCVL','BPWQ010WBLBSF','BPWQ010WBLCSD','BPWQ010YANCVL','BPWQ020EXPEXP','BPWQ030ANPCVL','BPWQUP1CHTCVL','BPWQUP1HARCVL','BPWQUP1MDPCVL','BPWQUP1ORICVL','BPWQUP1PJBCVL','BPWQUP1WBLCVL','BPWT010CHDCVL','BPWT010DAMCVL','BPWT010GOACVL','BPWT010MAHCVL','BPWT010TLGCVL','BPWTSP2ANPCVL','BPWTSP2TLGCVL','BPWTUP1HARCVL','BPWTUP1PJBCVL','BPWTUP1WBLCVL','FUVD015MAHCVL','FUVDGA1MAHCVL','FUVDOR1MAHCVL','FUVL015WBLCVL','FUVN010BHRCVL','FUVN010CHDCVL','FUVN010CHTCVL','FUVN010DAMCVL','FUVN010DELCVL','FUVN010DIUCVL','FUVN010DNHCVL','FUVN010GENCVL','FUVN010GOACVL','FUVN010HARCVL','FUVN010HIMCVL','FUVN010JHACVL','FUVN010JNKCVL','FUVN010KARCVL','FUVN010MAHCVL','FUVN010MDPCVL','FUVN010MHECVL','FUVN010ORICVL','FUVN010PJBCVL','FUVN010PONCVL','FUVN010RAJCVL','FUVN010UCHCVL','FUVN010UTPCVL','FUVN010WBLCVL','FUVN010YANCVL','FUVN014ANPCVL','FUVN015ANPCVL','FUVN015ARPCVL','FUVN015ASSCVL','FUVN015BHRCVL','FUVN015CHDCVL','FUVN015CHTCVL','FUVN015DAMCVL','FUVN015DELCVL','FUVN015DIUCVL','FUVN015DNHCVL','FUVN015EXPEXP','FUVN015GOACVL','FUVN015GUJCVL','FUVN015HARCVL','FUVN015HIMCVL','FUVN015JHACVL','FUVN015JNKCVL','FUVN015KARCVL','FUVN015MAHCVL','FUVN015MDPCVL','FUVN015MEGCVL','FUVN015MHECVL','FUVN015ORICVL','FUVN015PJBCVL','FUVN015PONCVL','FUVN015RAJCVL','FUVN015SKMCVL','FUVN015TLGCVL','FUVN015TRICVL','FUVN015UCHCVL','FUVN015UTPCVL','FUVN015WBLCVL','FUVN015YANCVL','FUVN020ANPCVL','FUVN020BHRCVL','FUVN020CHDCVL','FUVN020DAMCVL','FUVN020DELCVL','FUVN020DIUCVL','FUVN020DNHCVL','FUVN020GOACVL','FUVN020HARCVL','FUVN020HIMCVL','FUVN020JHACVL','FUVN020KARCVL','FUVN020MAHCVL','FUVN020MHECVL','FUVN020ORICVL','FUVN020PJBCVL','FUVN020PONCVL','FUVN020RAJCVL','FUVN020TLGCVL','FUVN020WBLCVL','FUVN020YANCVL','FUVN030ANPCVL','FUVN030BHRCVL','FUVN030CHDCVL','FUVN030DAMCVL','FUVN030DELCVL','FUVN030DIUCVL','FUVN030DNHCVL','FUVN030GOACVL','FUVN030HARCVL','FUVN030HIMCVL','FUVN030JHACVL','FUVN030KARCVL','FUVN030MAHCVL','FUVN030MHECVL','FUVN030ORICVL','FUVN030PJBCVL','FUVN030PONCVL','FUVN030RAJCVL','FUVN030TLGCVL','FUVN030WBLCVL','FUVN030YANCVL','FUVNGA1ANPCVL','FUVNGA1ARPCVL','FUVNGA1ASSCVL','FUVNGA1BHRCVL','FUVNGA1CHDCVL','FUVNGA1CHTCVL','FUVNGA1DAMCVL','FUVNGA1DELCVL','FUVNGA1DIUCVL','FUVNGA1DNHCVL','FUVNGA1EXPEXP','FUVNGA1GOACVL','FUVNGA1GUJCVL','FUVNGA1HARCVL','FUVNGA1JHACVL','FUVNGA1KARCVL','FUVNGA1MAHCVL','FUVNGA1MDPCVL','FUVNGA1MEGCVL','FUVNGA1MHECVL','FUVNGA1ORICVL','FUVNGA1PJBCVL','FUVNGA1PONCVL','FUVNGA1RAJCVL','FUVNGA1SKMCVL','FUVNGA1TLGCVL','FUVNGA1TRICVL','FUVNGA1WBLCVL','FUVNGA1YANCVL','FUVNOR1ANPCVL','FUVNOR1ARPCVL','FUVNOR1ASSCVL','FUVNOR1BHRCVL','FUVNOR1CHDCVL','FUVNOR1CHTCVL','FUVNOR1DAMCVL','FUVNOR1DELCVL','FUVNOR1DIUCVL','FUVNOR1DNHCVL','FUVNOR1EXPEXP','FUVNOR1GOACVL','FUVNOR1GUJCVL','FUVNOR1HARCVL','FUVNOR1JHACVL','FUVNOR1KARCVL','FUVNOR1MAHCVL','FUVNOR1MDPCVL','FUVNOR1MEGCVL','FUVNOR1MHECVL','FUVNOR1ORICVL','FUVNOR1PJBCVL','FUVNOR1PONCVL','FUVNOR1RAJCVL','FUVNOR1SKMCVL','FUVNOR1TLGCVL','FUVNOR1TRICVL','FUVNOR1WBLCVL','FUVNOR1YANCVL','FUVP010BHRCVL','FUVP010CHDCVL','FUVP010CHTCVL','FUVP010DAMCVL','FUVP010DELCVL','FUVP010DIUCVL','FUVP010DNHCVL','FUVP010GENCVL','FUVP010GOACVL','FUVP010HARCVL','FUVP010HIMCVL','FUVP010JHACVL','FUVP010JNKCVL','FUVP010KARCVL','FUVP010MAHCVL','FUVP010MDPCVL','FUVP010MHECVL','FUVP010ORICVL','FUVP010PJBCVL','FUVP010PONCVL','FUVP010RAJCVL','FUVP010UCHCVL','FUVP010UTPCVL','FUVP010WBLCVL','FUVP010YANCVL','FUVP014ANPCVL','FUVP015ANPCVL','FUVP015ARPCVL','FUVP015ASSCVL','FUVP015BHRCVL','FUVP015CHDCVL','FUVP015CHTCVL','FUVP015DAMCVL','FUVP015DELCVL','FUVP015DIUCVL','FUVP015DNHCVL','FUVP015EXPEXP','FUVP015GOACVL','FUVP015GUJCVL','FUVP015HARCVL','FUVP015HIMCVL','FUVP015JHACVL','FUVP015JNKCVL','FUVP015KARCVL','FUVP015MAHCVL','FUVP015MDPCVL','FUVP015MEGCVL','FUVP015MHECVL','FUVP015ORICVL','FUVP015PJBCVL','FUVP015PONCVL','FUVP015RAJCVL','FUVP015SKMCVL','FUVP015TLGCVL','FUVP015TRICVL','FUVP015UCHCVL','FUVP015UTPCVL','FUVP015WBLCVL','FUVP015YANCVL','FUVP020ANPCVL','FUVP020BHRCVL','FUVP020CHDCVL','FUVP020DAMCVL','FUVP020DELCVL','FUVP020DIUCVL','FUVP020DNHCVL','FUVP020GOACVL','FUVP020HARCVL','FUVP020HIMCVL','FUVP020JHACVL','FUVP020KARCVL','FUVP020MAHCVL','FUVP020MHECVL','FUVP020ORICVL','FUVP020PJBCVL','FUVP020PONCVL','FUVP020RAJCVL','FUVP020TLGCVL','FUVP020WBLCVL','FUVP020YANCVL','FUVP030ANPCVL','FUVP030BHRCVL','FUVP030CHDCVL','FUVP030DAMCVL','FUVP030DELCVL','FUVP030DIUCVL','FUVP030DNHCVL','FUVP030GOACVL','FUVP030HARCVL','FUVP030HIMCVL','FUVP030JHACVL','FUVP030KARCVL','FUVP030MAHCVL','FUVP030MHECVL','FUVP030ORICVL','FUVP030PJBCVL','FUVP030PONCVL','FUVP030RAJCVL','FUVP030TLGCVL','FUVP030WBLCVL','FUVP030YANCVL','FUVPGA1ANPCVL','FUVPGA1ARPCVL','FUVPGA1ASSCVL','FUVPGA1BHRCVL','FUVPGA1CHDCVL','FUVPGA1CHTCVL','FUVPGA1DAMCVL','FUVPGA1DELCVL','FUVPGA1DIUCVL','FUVPGA1DNHCVL','FUVPGA1EXPEXP','FUVPGA1GOACVL','FUVPGA1GUJCVL','FUVPGA1HARCVL','FUVPGA1JHACVL','FUVPGA1KARCVL','FUVPGA1MAHCVL','FUVPGA1MDPCVL','FUVPGA1MEGCVL','FUVPGA1MHECVL','FUVPGA1ORICVL','FUVPGA1PJBCVL','FUVPGA1PONCVL','FUVPGA1RAJCVL','FUVPGA1SKMCVL','FUVPGA1TLGCVL','FUVPGA1TRICVL','FUVPGA1WBLCVL','FUVPGA1YANCVL','FUVPOR1ANPCVL','FUVPOR1ARPCVL','FUVPOR1ASSCVL','FUVPOR1BHRCVL','FUVPOR1CHDCVL','FUVPOR1CHTCVL','FUVPOR1DAMCVL','FUVPOR1DELCVL','FUVPOR1DIUCVL','FUVPOR1DNHCVL','FUVPOR1EXPEXP','FUVPOR1GOACVL','FUVPOR1GUJCVL','FUVPOR1HARCVL','FUVPOR1JHACVL','FUVPOR1KARCVL','FUVPOR1MAHCVL','FUVPOR1MDPCVL','FUVPOR1MEGCVL','FUVPOR1MHECVL','FUVPOR1ORICVL','FUVPOR1PJBCVL','FUVPOR1PONCVL','FUVPOR1RAJCVL','FUVPOR1SKMCVL','FUVPOR1TLGCVL','FUVPOR1TRICVL','FUVPOR1WBLCVL','FUVPOR1YANCVL','FUVQ010GENCVL','FUVQ011ANNCSD','FUVQ011ANPCSD','FUVQ011ASSCSD','FUVQ011BHRCSD','FUVQ011BHRCVL','FUVQ011CHDCVL','FUVQ011CHTCVL','FUVQ011DAMCVL','FUVQ011DELCSD','FUVQ011DELCVL','FUVQ011DELPMF','FUVQ011DIUCVL','FUVQ011DNHCVL','FUVQ011GENCSD','FUVQ011GOACVL','FUVQ011HARCSD','FUVQ011HARCVL','FUVQ011HIMCSD','FUVQ011HIMCVL','FUVQ011HIMPMF','FUVQ011JHACSD','FUVQ011JHACVL','FUVQ011JNKBSF','FUVQ011JNKCSD','FUVQ011JNKCVL','FUVQ011KARCSD','FUVQ011KARCVL','FUVQ011KERCSD','FUVQ011MAHCSD','FUVQ011MAHCVL','FUVQ011MDPCSD','FUVQ011MDPCVL','FUVQ011MHECVL','FUVQ011ORICVL','FUVQ011PJBBSF','FUVQ011PJBCSD','FUVQ011PJBCVL','FUVQ011PONCVL','FUVQ011RAJCSD','FUVQ011RAJCVL','FUVQ011SKMCSD','FUVQ011TNDCSD','FUVQ011UCHCSD','FUVQ011UCHCVL','FUVQ011UTPCSD','FUVQ011UTPCVL','FUVQ011WBLBSF','FUVQ011WBLCSD','FUVQ011WBLCVL','FUVQ011YANCVL','FUVQ014ANPCVL','FUVQ015ANNCSD','FUVQ015ANPCSD','FUVQ015ANPCVL','FUVQ015ARPCVL','FUVQ015ASSCSD','FUVQ015ASSCVL','FUVQ015ASSPMF','FUVQ015BHRCSD','FUVQ015BHRCVL','FUVQ015BHRPMF','FUVQ015CHDCVL','FUVQ015CHTCVL','FUVQ015DAMCVL','FUVQ015DELCSD','FUVQ015DELCVL','FUVQ015DIUCVL','FUVQ015DNHCVL','FUVQ015EXPEXP','FUVQ015GOACVL','FUVQ015GUJBSF','FUVQ015GUJCSD','FUVQ015GUJCVL','FUVQ015HARCSD','FUVQ015HARCVL','FUVQ015HIMCVL','FUVQ015JHACSD','FUVQ015JHACVL','FUVQ015JHAPMF','FUVQ015JNKCVL','FUVQ015KARBSF','FUVQ015KARCSD','FUVQ015KARCVL','FUVQ015MAHCSD','FUVQ015MAHCVL','FUVQ015MDPBSF','FUVQ015MDPCSD','FUVQ015MDPCVL','FUVQ015MEGCVL','FUVQ015MHECVL','FUVQ015MNPBSF','FUVQ015MNPPMF','FUVQ015MZRPMF','FUVQ015NGLCSD','FUVQ015NGLPMF','FUVQ015ORICVL','FUVQ015PJBCVL','FUVQ015PONCVL','FUVQ015RAJCVL','FUVQ015SKMCVL','FUVQ015TLGCSD','FUVQ015TLGCVL','FUVQ015TNDCSD','FUVQ015TRIBSF','FUVQ015TRICVL','FUVQ015UCHCSD','FUVQ015UCHCVL','FUVQ015UTPCSD','FUVQ015UTPCVL','FUVQ015WBLCSD','FUVQ015WBLCVL','FUVQ015YANCVL','FUVQ020ANPCVL','FUVQ020BHRCVL','FUVQ020CHDCVL','FUVQ020DAMCVL','FUVQ020DELCVL','FUVQ020DIUCVL','FUVQ020DNHCVL','FUVQ020GOACVL','FUVQ020HARCVL','FUVQ020HIMCVL','FUVQ020JHACVL','FUVQ020KARCVL','FUVQ020MAHCVL','FUVQ020MHECVL','FUVQ020ORICVL','FUVQ020PJBCVL','FUVQ020PONCVL','FUVQ020RAJCVL','FUVQ020TLGCVL','FUVQ020WBLCVL','FUVQ020YANCVL','FUVQ030ANPCVL','FUVQ030BHRCVL','FUVQ030CHDCVL','FUVQ030DAMCVL','FUVQ030DELCVL','FUVQ030DIUCVL','FUVQ030DNHCVL','FUVQ030GOACVL','FUVQ030HARCVL','FUVQ030HIMCVL','FUVQ030JHACVL','FUVQ030KARCVL','FUVQ030MAHCVL','FUVQ030MHECVL','FUVQ030ORICVL','FUVQ030PJBCVL','FUVQ030PONCVL','FUVQ030RAJCVL','FUVQ030TLGCVL','FUVQ030WBLCVL','FUVQ030YANCVL','FUVQGA1ANPCVL','FUVQGA1ARPCVL','FUVQGA1ASSCVL','FUVQGA1BHRCVL','FUVQGA1CHDCVL','FUVQGA1CHTCVL','FUVQGA1DAMCVL','FUVQGA1DELCVL','FUVQGA1DIUCVL','FUVQGA1DNHCVL','FUVQGA1EXPEXP','FUVQGA1GOACVL','FUVQGA1GUJCVL','FUVQGA1HARCVL','FUVQGA1JHACVL','FUVQGA1KARCVL','FUVQGA1MAHCVL','FUVQGA1MDPBSF','FUVQGA1MDPCSD','FUVQGA1MDPCVL','FUVQGA1MEGCVL','FUVQGA1MHECVL','FUVQGA1ORICVL','FUVQGA1PJBCVL','FUVQGA1PONCVL','FUVQGA1RAJCVL','FUVQGA1SKMCVL','FUVQGA1TLGCVL','FUVQGA1TRICVL','FUVQGA1WBLCVL','FUVQGA1YANCVL','FUVQOR1ANPCVL','FUVQOR1ARPCVL','FUVQOR1ASSCVL','FUVQOR1BHRCVL','FUVQOR1CHDCVL','FUVQOR1CHTCVL','FUVQOR1DAMCVL','FUVQOR1DELCVL','FUVQOR1DIUCVL','FUVQOR1DNHCVL','FUVQOR1EXPEXP','FUVQOR1GOACVL','FUVQOR1GUJCVL','FUVQOR1HARCVL','FUVQOR1JHACVL','FUVQOR1KARCVL','FUVQOR1MAHCVL','FUVQOR1MDPBSF','FUVQOR1MDPCSD','FUVQOR1MDPCVL','FUVQOR1MEGCVL','FUVQOR1MHECVL','FUVQOR1ORICVL','FUVQOR1PJBCVL','FUVQOR1PONCVL','FUVQOR1RAJCVL','FUVQOR1SKMCVL','FUVQOR1TLGCVL','FUVQOR1TRICVL','FUVQOR1WBLCVL','FUVQOR1YANCVL','HCRN010ANPCVL','HCRN010CHDCVL','HCRN010DELCVL','HCRN010GOACVL','HCRN010HARCVL','HCRN010KARCVL','HCRN010MAHCVL','HCRN010PJBCVL','HCRN010WBLCVL','HCRQ011ANPCVL','HCRQ011CHDCVL','HCRQ011DELCVL','HCRQ011GOACVL','HCRQ011HARCVL','HCRQ011KARCVL','HCRQ011MAHCVL','HCRQ011PJBCVL','HCRQ011WBLCVL','HCTN010ANPCVL','HCTN010CHDCVL','HCTN010DELCVL','HCTN010GOACVL','HCTN010HARCVL','HCTN010KARCVL','HCTN010MAHCVL','HCTN010PJBCVL','HCTN010WBLCVL','HCTQ010ANPCVL','HCTQ010CHDCVL','HCTQ010DELCVL','HCTQ010GOACVL','HCTQ010HARCVL','HCTQ010KARCVL','HCTQ010MAHCVL','HCTQ010PJBCVL','HCTQ010WBLCVL','HPSL010GENCVL','HPSL010GOACVL','HPSL010GUJCVL','HPSL010MAHCVL','HPSL010TNDCVL','HPSL022DELCVL','HPSL040CHTCVL','HPSL040KARCVL','HPSL040SKMCVL','HPSL040TNDCVL','HPSL040WBLCVL','HPSM040ANPCVL','HPSM040DELCVL','HPSM040GOACVL','HPSM040KARCVL','HPSM040MAHCVL','HPSM040TLGCVL','HPSM040UTPCVL','HPSN040ANNCVL','HPSN040ANPCVL','HPSN040ARPCVL','HPSN040ASSCVL','HPSN040BHRCVL','HPSN040CHDCVL','HPSN040CHTCVL','HPSN040DAMCVL','HPSN040DELCVL','HPSN040DIUCVL','HPSN040DNHCVL','HPSN040GOACVL','HPSN040HARCVL','HPSN040HIMCVL','HPSN040JHACVL','HPSN040JNKCVL','HPSN040KARCVL','HPSN040MAHCVL','HPSN040MDPCVL','HPSN040MEGCVL','HPSN040MHECVL','HPSN040MZRCVL','HPSN040ORICVL','HPSN040PJBCVL','HPSN040PONCVL','HPSN040RAJCVL','HPSN040SKMCVL','HPSN040TLGCVL','HPSN040TRICVL','HPSN040UCHCVL','HPSN040UTPCVL','HPSN040WBLCVL','HPSN040YANCVL','HPSN041ANPCVL','HPSP040ANNCVL','HPSP040ANPCVL','HPSP040ARPCVL','HPSP040ASSCVL','HPSP040BHRCVL','HPSP040CHDCVL','HPSP040CHTCVL','HPSP040DAMCVL','HPSP040DELCVL','HPSP040DIUCVL','HPSP040DNHCVL','HPSP040GENCVL','HPSP040GOACVL','HPSP040GUJCVL','HPSP040HARCVL','HPSP040HIMCVL','HPSP040JHACVL','HPSP040JNKCVL','HPSP040KARCVL','HPSP040MAHCVL','HPSP040MDPCVL','HPSP040MEGCVL','HPSP040MHECVL','HPSP040MZRCVL','HPSP040ORICVL','HPSP040PJBCVL','HPSP040PONCVL','HPSP040RAJCVL','HPSP040SKMCVL','HPSP040TLGCVL','HPSP040TRICVL','HPSP040UCHCVL','HPSP040UTPCVL','HPSP040WBLCVL','HPSP040YANCVL','HPSP041ANPCVL','HPSQ040ANNCSD','HPSQ040ANNCVL','HPSQ040ANPCSD','HPSQ040ANPCVL','HPSQ040ARPCVL','HPSQ040ASSBSF','HPSQ040ASSCSD','HPSQ040ASSCVL','HPSQ040ASSPMF','HPSQ040BHRCSD','HPSQ040BHRCVL','HPSQ040BHRPMF','HPSQ040CHDCVL','HPSQ040CHDPMF','HPSQ040CHTCSD','HPSQ040CHTCVL','HPSQ040DAMCVL','HPSQ040DELCSD','HPSQ040DELCVL','HPSQ040DELPMF','HPSQ040DIUCVL','HPSQ040DNHCVL','HPSQ040GENBSF','HPSQ040GENCSD','HPSQ040GENCVL','HPSQ040GOACVL','HPSQ040GUJBSF','HPSQ040GUJCSD','HPSQ040GUJCVL','HPSQ040HARCSD','HPSQ040HARCVL','HPSQ040HARPMF','HPSQ040HIMCSD','HPSQ040HIMCVL','HPSQ040HIMPMF','HPSQ040JHACSD','HPSQ040JHACVL','HPSQ040JHAPMF','HPSQ040JNKBSF','HPSQ040JNKCSD','HPSQ040JNKCVL','HPSQ040KARBSF','HPSQ040KARCSD','HPSQ040KARCVL','HPSQ040KERCSD','HPSQ040MAHCSD','HPSQ040MAHCVL','HPSQ040MDPBSF','HPSQ040MDPCSD','HPSQ040MDPCVL','HPSQ040MEGCVL','HPSQ040MHECVL','HPSQ040MNPBSF','HPSQ040MNPPMF','HPSQ040MZRBSF','HPSQ040MZRCVL','HPSQ040MZRPMF','HPSQ040NGLCSD','HPSQ040NGLPMF','HPSQ040ORICVL','HPSQ040PJBBSF','HPSQ040PJBCSD','HPSQ040PJBCVL','HPSQ040PONCVL','HPSQ040RAJCSD','HPSQ040RAJCVL','HPSQ040RAJPMF','HPSQ040SKMCVL','HPSQ040TLGCSD','HPSQ040TLGCVL','HPSQ040TNDCSD','HPSQ040TNDCVL','HPSQ040TRIBSF','HPSQ040TRICVL','HPSQ040UCHCSD','HPSQ040UCHCVL','HPSQ040UTPCSD','HPSQ040UTPCVL','HPSQ040WBLCSD','HPSQ040WBLCVL','HPSQ040YANCVL','HPSQ041ANPCVL','HPTL010DIACVL','HPTL010WBLCVL','HPTQ010ANPCVL','HPTQ010ARPCVL','HPTQ010ASSCVL','HPTQ010BHRCVL','HPTQ010CHDCVL','HPTQ010CHTCVL','HPTQ010DAMCVL','HPTQ010DELCSD','HPTQ010DELCVL','HPTQ010DIUCVL','HPTQ010DNHCVL','HPTQ010GOACVL','HPTQ010GUJCVL','HPTQ010HARCVL','HPTQ010HIMCVL','HPTQ010JHACVL','HPTQ010KARCVL','HPTQ010MAHCVL','HPTQ010MDPCSD','HPTQ010MDPCVL','HPTQ010MEGCVL','HPTQ010MZRCVL','HPTQ010ORICVL','HPTQ010PJBCVL','HPTQ010RAJCVL','HPTQ010SKMCVL','HPTQ010TLGCVL','HPTQ010TRICVL','HPTQ010UCHCSD','HPTQ010UCHCVL','HPTQ010UTPCVL','HPTQ010WBLCVL','IBWD010EXPEXP','IBWD010KARCVL','IBWD010MAHCVL','IBWD010RAJCVL','IBWD011MAHCVL','IBWD037ANPCVL','IBWDAG1MDPCVL','IBWDAG2MDPCVL','IBWDCG1ANPCVL','IBWDCG1TLGCVL','IBWDCG2ANPCVL','IBWDCG2TLGCVL','IBWDFG1ORICVL','IBWDFG1ORICVL ','IBWDFG2ORICVL','IBWDSG1KARCVL','IBWDSG2KARCVL','IBWL010EXPEXP','IBWL010MAHCVL','IBWL011MAHCVL','IBWL020EXPEXP','IBWLAG1WBLCVL','IBWLAG2WBLCVL','IBWN010ANNCVL','IBWN010ARPCVL','IBWN010ASSCVL','IBWN010CHDCVL','IBWN010DAMCVL','IBWN010DIUCVL','IBWN010DNHCVL','IBWN010EXPEXP','IBWN010GENCVL','IBWN010GOACVL','IBWN010GUJCVL','IBWN010HIMCVL','IBWN010JNKCVL','IBWN010KARCVL','IBWN010MAHCVL','IBWN010MEGCVL','IBWN010MHECVL','IBWN010MZRCVL','IBWN010PONCVL','IBWN010RAJCVL','IBWN010SKMCVL','IBWN010TRICVL','IBWN010UCHCVL','IBWN010YANCVL','IBWN011ANNCVL','IBWN011ARPCVL','IBWN011ASSCVL','IBWN011CHDCVL','IBWN011DAMCVL','IBWN011DIUCVL','IBWN011DNHCVL','IBWN011EXPEXP','IBWN011GOACVL','IBWN011HIMCVL','IBWN011JNKCVL','IBWN011KARCVL','IBWN011MAHCVL','IBWN011MEGCVL','IBWN011MHECVL','IBWN011MZRCVL','IBWN011PONCVL','IBWN011RAJCVL','IBWN011SKMCVL','IBWN011TRICVL','IBWN011UCHCVL','IBWN011YANCVL','IBWN020DELCVL','IBWN020UCHCVL','IBWN020UTPCVL','IBWN021DELCVL','IBWN021UTPCVL','IBWN033ANPCVL','IBWN035ANPCVL','IBWN037ANPCVL','IBWNAG1CHTCVL','IBWNAG1HARCVL','IBWNAG1MDPCVL','IBWNAG1PJBCVL','IBWNAG1WBLCVL','IBWNAG2CHTCVL','IBWNAG2HARCVL','IBWNAG2MDPCVL','IBWNAG2PJBCVL','IBWNAG2WBLCVL','IBWNBG1BHRCVL','IBWNBG2BHRCVL','IBWNCG1ANPCVL','IBWNCG1TLGCVL','IBWNCG2ANPCVL','IBWNCG2TLGCVL','IBWNFG1BHRCVL','IBWNFG1JHACVL','IBWNFG1ORICVL','IBWNFG2BHRCVL','IBWNFG2JHACVL','IBWNFG2ORICVL','IBWP010ANNCVL','IBWP010ARPCVL','IBWP010ASSCVL','IBWP010CHDCVL','IBWP010DAMCVL','IBWP010DIUCVL','IBWP010DNHCVL','IBWP010EXPEXP','IBWP010GENCVL','IBWP010GOACVL','IBWP010GUJCVL','IBWP010HIMCVL','IBWP010JNKCVL','IBWP010KARCVL','IBWP010MAHCVL','IBWP010MEGCVL','IBWP010MHECVL','IBWP010MZRCVL','IBWP010PONCVL','IBWP010RAJCVL','IBWP010SKMCVL','IBWP010TRICVL','IBWP010UCHCVL','IBWP010YANCVL','IBWP011ANNCVL','IBWP011ARPCVL','IBWP011ASSCVL','IBWP011CHDCVL','IBWP011DAMCVL','IBWP011DIUCVL','IBWP011DNHCVL','IBWP011EXPEXP','IBWP011GOACVL','IBWP011GUJCVL','IBWP011HIMCVL','IBWP011JNKCVL','IBWP011KARCVL','IBWP011MAHCVL','IBWP011MEGCVL','IBWP011MHECVL','IBWP011MZRCVL','IBWP011PONCVL','IBWP011RAJCVL','IBWP011SKMCVL','IBWP011TRICVL','IBWP011UCHCVL','IBWP011YANCVL','IBWP020DELCVL','IBWP020UCHCVL','IBWP020UTPCVL','IBWP021DELCVL','IBWP021UTPCVL','IBWP033ANPCVL','IBWP035ANPCVL','IBWP037ANPCVL','IBWPAG1CHTCVL','IBWPAG1HARCVL','IBWPAG1MDPCVL','IBWPAG1PJBCVL','IBWPAG1WBLCVL','IBWPAG2CHTCVL','IBWPAG2HARCVL','IBWPAG2MDPCVL','IBWPAG2PJBCVL','IBWPAG2WBLCVL','IBWPBG1BHRCVL','IBWPBG2BHRCVL','IBWPCG1ANPCVL','IBWPCG1TLGCVL','IBWPCG2ANPCVL','IBWPCG2TLGCVL','IBWPFG1BHRCVL','IBWPFG1JHACVL','IBWPFG1ORICVL','IBWPFG2BHRCVL','IBWPFG2JHACVL','IBWPFG2ORICVL','IBWQ010ANNCVL','IBWQ010ARPCVL','IBWQ010ASSCVL','IBWQ010CHDCVL','IBWQ010DAMCVL','IBWQ010DIUCVL','IBWQ010DNHCVL','IBWQ010EXPEXP','IBWQ010GENCVL','IBWQ010GOACVL','IBWQ010GUJCVL','IBWQ010HIMCVL','IBWQ010JNKBSF','IBWQ010JNKCVL','IBWQ010KARCVL','IBWQ010MAHCVL','IBWQ010MEGCVL','IBWQ010MHECVL','IBWQ010MZRCVL','IBWQ010PJBBSF','IBWQ010PONCVL','IBWQ010RAJCVL','IBWQ010SKMCVL','IBWQ010TRICVL','IBWQ010UCHCVL','IBWQ010YANCVL','IBWQ011ANNCVL','IBWQ011ARPCVL','IBWQ011ASSCVL','IBWQ011CHDCVL','IBWQ011DAMCVL','IBWQ011DIUCVL','IBWQ011DNHCVL','IBWQ011EXPEXP','IBWQ011GOACVL','IBWQ011GUJCVL','IBWQ011HIMCVL','IBWQ011JNKCVL','IBWQ011KARCVL','IBWQ011MAHCVL','IBWQ011MEGCVL','IBWQ011MHECVL','IBWQ011MZRCVL','IBWQ011PONCVL','IBWQ011RAJCVL','IBWQ011SKMCVL','IBWQ011TRICVL','IBWQ011UCHCVL','IBWQ011YANCVL','IBWQ020DELCVL','IBWQ020EXPEXP','IBWQ020HIMCSD','IBWQ020JNKCSD','IBWQ020MAHCSD','IBWQ020PJBCSD','IBWQ020UCHCVL','IBWQ020UTPCVL','IBWQ021DELCVL','IBWQ021EXPEXP','IBWQ021UTPCVL','IBWQ033ANPCVL','IBWQ035ANPCVL','IBWQ037ANPCVL','IBWQAG1CHTCVL','IBWQAG1HARCVL','IBWQAG1MDPCVL','IBWQAG1PJBCVL','IBWQAG1WBLCVL','IBWQAG2CHTCVL','IBWQAG2HARCVL','IBWQAG2MDPCVL','IBWQAG2PJBCVL','IBWQAG2WBLCVL','IBWQBG1BHRCVL','IBWQBG2BHRCVL','IBWQCG1ANPCVL','IBWQCG1TLGCVL','IBWQCG2ANPCVL','IBWQCG2TLGCVL','IBWQFG1BHRCVL','IBWQFG1JHACVL','IBWQFG1ORICVL','IBWQFG2BHRCVL','IBWQFG2JHACVL','IBWQFG2ORICVL','IBWT010GOACVL','IBWT010MAHCVL','IBWT011DAMCVL','IBWT011GOACVL','IBWT011MAHCVL','IBWTAG1WBLCVL','MBWD010EXPEXP','MBWD010MAHCVL','MBWL010WBLCVL','MBWN010CHTCVL','MBWN010EXPEXP','MBWN010HIMCVL','MBWN010MAHCVL','MBWN010MDPCVL','MBWN010WBLCVL','MBWP010CHTCVL','MBWP010EXPEXP','MBWP010HIMCVL','MBWP010MAHCVL','MBWP010MDPCVL','MBWP010WBLCVL','MBWQ010CHTCVL','MBWQ010EXPEXP','MBWQ010HIMCVL','MBWQ010MAHCVL','MBWQ010MDPCVL','MBWQ010WBLCVL','MBWQ020EXPEXP','NHNP010ANPCVL','NHNP010ARPCVL','NHNP010ASSCVL','NHNP010CHDCVL','NHNP010DAMCVL','NHNP010DELCVL','NHNP010DIUCVL','NHNP010DNHCVL','NHNP010GOACVL','NHNP010GUJCVL','NHNP010HARCVL','NHNP010HIMCVL','NHNP010KARCVL','NHNP010MAHCVL','NHNP010MDPCVL','NHNP010MEGCVL','NHNP010ORICVL','NHNP010PJBCVL','NHNP010PONCVL','NHNP010RAJCVL','NHNP010TLGCVL','NHNP010TNDCVL','NHNP010UCHCVL','NHNP010UTPCVL','NHNP010WBLCVL','NHNP020ANPCVL','NHNP020ARPCVL','NHNP020ASSCVL','NHNP020CHDCVL','NHNP020DAMCVL','NHNP020DELCVL','NHNP020DIUCVL','NHNP020DNHCVL','NHNP020GOACVL','NHNP020GUJCVL','NHNP020HARCVL','NHNP020HIMCVL','NHNP020KARCVL','NHNP020MAHCVL','NHNP020MDPCVL','NHNP020MEGCVL','NHNP020ORICVL','NHNP020PJBCVL','NHNP020PONCVL','NHNP020RAJCVL','NHNP020TLGCVL','NHNP020TNDCVL','NHNP020UTPCVL','NHNP020WBLCVL','NHNP030ANPCVL','NHNP030ARPCVL','NHNP030ASSCVL','NHNP030CHDCVL','NHNP030DAMCVL','NHNP030DELCVL','NHNP030DIUCVL','NHNP030DNHCVL','NHNP030GOACVL','NHNP030GUJCVL','NHNP030HARCVL','NHNP030HIMCVL','NHNP030KARCVL','NHNP030MAHCVL','NHNP030MDPCVL','NHNP030MEGCVL','NHNP030ORICVL','NHNP030PJBCVL','NHNP030PONCVL','NHNP030RAJCVL','NHNP030TLGCVL','NHNP030TNDCVL','NHNP030UCHCVL','NHNP030UTPCVL','NHNP030WBLCVL','NHNP040ANPCVL','NHNP040ARPCVL','NHNP040ASSCVL','NHNP040CHDCVL','NHNP040DAMCVL','NHNP040DELCVL','NHNP040DIUCVL','NHNP040DNHCVL','NHNP040GOACVL','NHNP040GUJCVL','NHNP040HARCVL','NHNP040HIMCVL','NHNP040KARCVL','NHNP040MAHCVL','NHNP040MDPCVL','NHNP040MEGCVL','NHNP040ORICVL','NHNP040PJBCVL','NHNP040PONCVL','NHNP040RAJCVL','NHNP040TLGCVL','NHNP040TNDCVL','NHNP040UTPCVL','NHNP040WBLCVL','NHNPCBS','NHNPCBW','NHNPCSR','NHNPSBW','NHNPSHW','NHNPSYR','NHNPVIG','NHNQ010ANPCVL','NHNQ010ARPCVL','NHNQ010ASSCVL','NHNQ010CHDCVL','NHNQ010DAMCVL','NHNQ010DELCVL','NHNQ010DELDFR','NHNQ010DIACVL','NHNQ010DIUCVL','NHNQ010DNHCVL','NHNQ010EXPEXP','NHNQ010GOACVL','NHNQ010GUJCVL','NHNQ010HARCVL','NHNQ010HIMCVL','NHNQ010KARCVL','NHNQ010MAHCVL','NHNQ010MDPCVL','NHNQ010MEGCVL','NHNQ010MHECVL','NHNQ010ORICVL','NHNQ010PJBCVL','NHNQ010PONCVL','NHNQ010RAJCVL','NHNQ010TLGCVL','NHNQ010TNDCVL','NHNQ010UCHCVL','NHNQ010UTPCVL','NHNQ010WBLCVL','NHNQ010YANCVL','NHNQ020ANPCVL','NHNQ020ARPCVL','NHNQ020ASSCVL','NHNQ020CHDCVL','NHNQ020DAMCVL','NHNQ020DELCVL','NHNQ020DELDFR','NHNQ020DIUCVL','NHNQ020DNHCVL','NHNQ020GOACVL','NHNQ020GUJCVL','NHNQ020HARCVL','NHNQ020HIMCVL','NHNQ020KARCVL','NHNQ020MAHCVL','NHNQ020MDPCVL','NHNQ020MEGCVL','NHNQ020MHECVL','NHNQ020ORICVL','NHNQ020PJBCVL','NHNQ020PONCVL','NHNQ020RAJCVL','NHNQ020TLGCVL','NHNQ020TNDCVL','NHNQ020UTPCVL','NHNQ020WBLCVL','NHNQ020YANCVL','NHNQ030ANPCVL','NHNQ030ARPCVL','NHNQ030ASSCVL','NHNQ030CHDCVL','NHNQ030DAMCVL','NHNQ030DELCVL')";
        //        //string FormedQueryLookupType2 = "SP_UPDATE_WHERECLAUSE_TEST";
        //        //GetDataSet objDeleteSetValue2 = new GetDataSet();
        //        //objDeleteSetValue2.UpdateWhereClause(FormedQueryLookupType2, whereclause, ConfigurationID);
        //        returnVal = "true";
        //    }
        //    catch (Exception ex)
        //    {
        //        returnVal = "false";
        //    }
        //    return Json(returnVal, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult GetNextLevels(string DimensionName, string DimensionLevelName)
        //{
        //    string dynamicQuery = "select distinct dimension_level from ep_dw_dimension_mapping where dimension_code = '" + DimensionName + "' and dimension_level_name = '" + DimensionLevelName + "'";
        //    string FormedQueryLookupType = "SP_GETDIMENSION_DETAILS";
        //    DataSet datasetDimension = new DataSet();
        //    using (GetDataSet objGetDataSetValue = new GetDataSet())
        //    {
        //        try
        //        {
        //            datasetDimension = objGetDataSetValue.GetDataSetValueStringParam(FormedQueryLookupType, dynamicQuery);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogError lgerr = new LogError();
        //            lgerr.LogErrorInTextFile(ex);
        //            SentErrorMail.SentEmailtoError("Error on Filteration.see the below details: SP Name " + FormedQueryLookupType + " And dynamic SQL query:" + dynamicQuery + " InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //            return Json("Insufficient Data", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    string levelVal = Convert.ToString(datasetDimension.Tables[0].Rows[0]["DIMENSION_LEVEL"]);
        //    return Json(levelVal, JsonRequestBehavior.AllowGet);
        //}
        //#endregion

    }
}