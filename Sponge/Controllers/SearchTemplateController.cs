using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sponge.Common;
using Sponge.Models;
using Sponge.ViewModel;
using System.Diagnostics;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class SearchTemplateController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly string AdminRoleId = System.Configuration.ConfigurationManager.AppSettings["AdminRoleId"];

        public SearchTemplateController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SearchTemplate()
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            List<SPG_SUBFUNCTION> subfunctions = new();
            var role = sPONGE_Context.SPG_USERS_FUNCTION.FirstOrDefault(x => x.USER_ID == userName[1] && (x.ROLE_ID == 5 || x.ROLE_ID == 3));
            int RoleID = 0;
            if (role != null)
            {
                RoleID = (int)role.ROLE_ID;
            }
            if ((RoleID == 5) || (RoleID == 3))
            {
                subfunctions = sPONGE_Context.SPG_SUBFUNCTION.ToList();
            }
            else
            {
                subfunctions = (from subFunc in sPONGE_Context.SPG_SUBFUNCTION
                                join userFunc in sPONGE_Context.SPG_USERS_FUNCTION on subFunc.SUBFUNCTION_ID equals userFunc.SUB_FUNCTION_ID
                                where userFunc.USER_ID == userName[1] && userFunc.ACTIVE_FLAG == "Y"
                                select subFunc).Distinct().ToList();
            }
            return View(subfunctions);
        }
        public JsonResult GetSubjectAreas(int subFunctionId)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            List<SPG_SUBJECTAREA> subjectArea = new();
            var role = sPONGE_Context.SPG_USERS_FUNCTION.FirstOrDefault(x => x.USER_ID == userName[1] && (x.ROLE_ID == 5 || x.ROLE_ID == 3));
            int RoleID = 0;
            if (role != null)
            {
                RoleID = (int)role.ROLE_ID;
            }
            if ((RoleID == 5) || (RoleID == 3))
            {
                subjectArea = sPONGE_Context.SPG_SUBJECTAREA.Where(x => x.SUBFUNCTION_ID == subFunctionId).ToList();
            }
            else
            {
                subjectArea = (from SubjectArea in sPONGE_Context.SPG_SUBJECTAREA
                               join userconfig in sPONGE_Context.SPG_CONFIGURATION on SubjectArea.SUBJECTAREA_ID equals userconfig.SUBJECTAREA_ID
                               where SubjectArea.SUBFUNCTION_ID == subFunctionId && (userconfig.USER_ID == userName[1] || userconfig.APPROVER_ID == userName[1])
                               select SubjectArea).ToList();
            }
            return Json(subjectArea);
        }

        public JsonResult GetAssignedUsers(int subjectAreaId)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            List<SPG_USERS> userlist = new();
            var role = sPONGE_Context.SPG_USERS_FUNCTION.FirstOrDefault(x => x.USER_ID == userName[1] && (x.ROLE_ID == 5 || x.ROLE_ID == 3));
            int RoleID = 0;
            if (role != null)
            {
                RoleID = (int)role.ROLE_ID;
            }
            if ((RoleID == 5) || RoleID == 3)
            {
                userlist = (from SubjectArea in sPONGE_Context.SPG_SUBJECTAREA
                            join userconfig in sPONGE_Context.SPG_CONFIGURATION on SubjectArea.SUBJECTAREA_ID equals userconfig.SUBJECTAREA_ID
                            join users in sPONGE_Context.SPG_USERS on userconfig.USER_ID equals users.USER_ID
                            where SubjectArea.SUBJECTAREA_ID == subjectAreaId
                            select users).ToList();
            }
            else
            {
                userlist = sPONGE_Context.SPG_USERS.Where(x => x.USER_ID == userName[1] && x.ACTIVE_FLAG == "Y").ToList();
            }
            return Json(userlist);
        }

        [HttpPost]
        public JsonResult GetSearchData(int subFunctionId, int subjectAreaId, string assignToUser, DateTime dateFrom, DateTime dateTo, string active)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context sponge_context = new();
            List<SearchDataList> SearchConfgData = new();
            var role = sponge_context.SPG_USERS_FUNCTION.FirstOrDefault(x => x.USER_ID == userName[1] && (x.ROLE_ID == 5 || x.ROLE_ID == 3));
            var AdminRoleId = System.Configuration.ConfigurationManager.AppSettings["AdminRoleId"];

            // Check if the user has selected a date (other than the default date)
            bool isDateFromSelected = dateFrom != DateTime.MinValue;
            bool isDateToSelected = dateTo != DateTime.MinValue;
            bool isActiveSelected = active == "Y" ? true : false;

            int RoleID = 0;
            if (role != null)
            {
                RoleID = (int)role.ROLE_ID;
            }
            if ((RoleID == 5) || (RoleID == 3))
            {
                SearchConfgData = (from conf in sponge_context.SPG_CONFIGURATION
                                   join u in sponge_context.SPG_USERS on conf.USER_ID equals u.USER_ID
                                   join sa in sponge_context.SPG_SUBJECTAREA on conf.SUBJECTAREA_ID equals sa.SUBJECTAREA_ID
                                   join sf in sponge_context.SPG_SUBFUNCTION on sa.SUBFUNCTION_ID equals sf.SUBFUNCTION_ID
                                   where (sf.SUBFUNCTION_ID == subFunctionId)

                                    && (subjectAreaId == 0 || sa.SUBJECTAREA_ID == subjectAreaId)
                                     && (assignToUser == "0" || conf.USER_ID == assignToUser)
                 && (active == null || conf.ACTIVE_FLAG == active)

                                   select new SearchDataList
                                   {
                                       ConfigId = conf.CONFIG_ID,
                                       SubjectAreaId = sa.SUBJECTAREA_ID,
                                       SubjectAreaName = sa.SUBJECTAREA_NAME,
                                       AssignedUser = u.Name,
                                       Active = (conf.ACTIVE_FLAG == null) ? "" : (conf.ACTIVE_FLAG == "Y" ? "Yes" : "No"),
                                       EffectiveFrom = conf.EFFECTIVE_FROM,
                                       EffectiveTo = conf.EFFECTIVE_TO,
                                       ManualSendResendUrl = conf.ACTIVE_FLAG == null ? "In Progress" : conf.ACTIVE_FLAG == "N" ? "Inactive" : "Generate Template"
                                   }).Distinct().ToList();
                // To send data based on active status
                //if(active != null)
                //{
                //    SearchConfgData = SearchDataOnStatus(subjectAreaId,subFunctionId, active);
                //}

                if (isDateFromSelected && !isDateToSelected)
                {
                    // Only dateFrom is selected
                    SearchConfgData = SearchConfgData.Where(x => x.EffectiveFrom >= dateFrom).ToList();
                }
                else if (!isDateFromSelected && isDateToSelected)
                {
                    // Only dateTo is selected
                    SearchConfgData = SearchConfgData.Where(x => x.EffectiveTo <= dateTo).ToList();
                }
                else if (isDateFromSelected && isDateToSelected)
                {
                    // Both dateFrom and dateTo are selected
                    SearchConfgData = SearchConfgData.Where(x =>
                        (x.EffectiveFrom == null || x.EffectiveFrom >= dateFrom) &&
                        (x.EffectiveTo <= dateTo)).ToList();
                }


            }
            else
            {
                SearchConfgData = (from conf in sponge_context.SPG_CONFIGURATION
                                   join u in sponge_context.SPG_USERS on conf.USER_ID equals u.USER_ID
                                   join sa in sponge_context.SPG_SUBJECTAREA on conf.SUBJECTAREA_ID equals sa.SUBJECTAREA_ID
                                   join sf in sponge_context.SPG_SUBFUNCTION on sa.SUBFUNCTION_ID equals sf.SUBFUNCTION_ID
                                   where (sf.SUBFUNCTION_ID == subFunctionId)

                                    && (subjectAreaId == 0 || sa.SUBJECTAREA_ID == subjectAreaId)
                                     && ((conf.APPROVER_ID == userName[1] || (conf.USER_ID == userName[1])))
                 && (active == null || conf.ACTIVE_FLAG == active)
                                   select new SearchDataList
                                   {
                                       ConfigId = conf.CONFIG_ID,
                                       SubjectAreaId = sa.SUBJECTAREA_ID,
                                       SubjectAreaName = sa.SUBJECTAREA_NAME,
                                       AssignedUser = u.Name,
                                       Active = (conf.ACTIVE_FLAG == null) ? "" : (conf.ACTIVE_FLAG == "Y" ? "Yes" : "No"),
                                       EffectiveTo = conf.EFFECTIVE_TO,
                                       EffectiveFrom = conf.EFFECTIVE_FROM,
                                       ManualSendResendUrl = conf.ACTIVE_FLAG == null ? "In Progress" : conf.ACTIVE_FLAG == "N" ? "Inactive" : "Generate Template"
                                   }).Distinct().ToList();

                
                if (isDateFromSelected && !isDateToSelected)
                {
                    // Only dateFrom is selected
                    SearchConfgData = SearchConfgData.Where(x => x.EffectiveFrom >= dateFrom).ToList();
                }
                else if (!isDateFromSelected && isDateToSelected)
                {
                    // Only dateTo is selected
                    SearchConfgData = SearchConfgData.Where(x => x.EffectiveTo <= dateTo).ToList();
                }
                else if (isDateFromSelected && isDateToSelected)
                {
                    // Both dateFrom and dateTo are selected
                    SearchConfgData = SearchConfgData.Where(x =>
                        (x.EffectiveFrom == null || x.EffectiveFrom <= dateTo) &&
                        (x.EffectiveTo == null || x.EffectiveTo >= dateFrom)).ToList();
                }
            }
            return Json(SearchConfgData);
        }

        //public List<SearchDataList> SearchDataOnStatus(int subjectAreaId, int subFunctionId, string Status)
        //{
        //    SPONGE_Context sponge_context = new();
        //    try
        //    {
        //        var query = (from conf in sponge_context.SPG_CONFIGURATION
        //                           join u in sponge_context.SPG_USERS on conf.USER_ID equals u.USER_ID
        //                           join sa in sponge_context.SPG_SUBJECTAREA on conf.SUBJECTAREA_ID equals sa.SUBJECTAREA_ID
        //                           join sf in sponge_context.SPG_SUBFUNCTION on sa.SUBFUNCTION_ID equals sf.SUBFUNCTION_ID
        //                           where (sf.SUBFUNCTION_ID == subFunctionId)

        //                            && (subjectAreaId == 0 || sa.SUBJECTAREA_ID == subjectAreaId)
        //                            && (Status == null || conf.ACTIVE_FLAG == Status)
        //                            &&(Status == null || sa.ACTIVE_FLAG == Status)


        //                           select new SearchDataList
        //                           {
        //                               ConfigId = conf.CONFIG_ID,
        //                               SubjectAreaId = sa.SUBJECTAREA_ID,
        //                               SubjectAreaName = sa.SUBJECTAREA_NAME,
        //                               AssignedUser = u.Name,
        //                               Active = (conf.ACTIVE_FLAG == null) ? "" : (conf.ACTIVE_FLAG == "Y" ? "Yes" : "No"),
        //                               EffectiveFrom = conf.EFFECTIVE_FROM,
        //                               EffectiveTo = conf.EFFECTIVE_TO,
        //                               ManualSendResendUrl = conf.ACTIVE_FLAG == null ? "In Progress" : conf.ACTIVE_FLAG == "N" ? "Inactive" : "Generate Template"
        //                           }).Distinct().ToList();

        //        return query;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("An error occurred: " + ex.Message);

        //        // Returning an empty list or null, depending on the desired behavior:
        //        return new List<SearchDataList>(); // or return null;
        //    }
        //}
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}