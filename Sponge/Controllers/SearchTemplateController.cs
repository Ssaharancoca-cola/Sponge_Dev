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
            var role = sPONGE_Context.SPG_USERS_FUNCTION.FirstOrDefault(x => x.USER_ID == userName[1] || x.ROLE_ID == 5);
            int RoleID = 0;
            if (role != null)
            {
                 RoleID = (int)role.ROLE_ID;
            }
            if(RoleID == 5)
            {
                 subfunctions = sPONGE_Context.SPG_SUBFUNCTION.ToList();
            }
            else
            {
                 subfunctions = (from subFunc in sPONGE_Context.SPG_SUBFUNCTION
                                   join userFunc in sPONGE_Context.SPG_USERS_FUNCTION on subFunc.SUBFUNCTION_ID equals userFunc.SUB_FUNCTION_ID
                                   where userFunc.USER_ID == userName[1] && userFunc.ACTIVE_FLAG == "Y"
                                   select subFunc).ToList();
            }
            return View(subfunctions);           
        }
        public JsonResult GetSubjectAreas(int subFunctionId)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            List<SPG_SUBJECTAREA> subjectArea = new();
            var role = sPONGE_Context.SPG_USERS_FUNCTION.FirstOrDefault(x => x.USER_ID == userName[1] || x.ROLE_ID == 5);
            int RoleID = 0;
            if (role != null)
            {
                RoleID = (int)role.ROLE_ID;
            }
            if (RoleID == 5)
            {
                subjectArea = sPONGE_Context.SPG_SUBJECTAREA.ToList();
            }
            else
            {
                subjectArea = (from SubjectArea in sPONGE_Context.SPG_SUBJECTAREA
                               join userconfig in sPONGE_Context.SPG_CONFIGURATION on SubjectArea.SUBJECTAREA_ID equals userconfig.SUBJECTAREA_ID
                               where SubjectArea.SUBFUNCTION_ID == subFunctionId && userconfig.USER_ID == userName[1]
                               select SubjectArea).ToList();
            }
            return Json(subjectArea);
        }

        public JsonResult GetAssignedUsers(int subjectAreaId)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            List<SPG_USERS> userlist = new();
            var role = sPONGE_Context.SPG_USERS_FUNCTION.FirstOrDefault(x => x.USER_ID == userName[1] || x.ROLE_ID == 5);
            int RoleID = 0;
            if (role != null)
            {
                RoleID = (int)role.ROLE_ID;
            }
            if (RoleID == 5)
            {
                userlist = sPONGE_Context.SPG_USERS.Where(x => x.ACTIVE_FLAG == "Y").ToList();
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
            var role = sponge_context.SPG_USERS_FUNCTION.FirstOrDefault(x => x.USER_ID == userName[1] || x.ROLE_ID == 5);
            int RoleID = 0;
            if (role != null)
            {
                RoleID = (int)role.ROLE_ID;
            }
            if (RoleID == 5)
            {
                SearchConfgData = (from conf in sponge_context.SPG_CONFIGURATION
                                   join u in sponge_context.SPG_USERS on conf.USER_ID equals u.USER_ID
                                   join sa in sponge_context.SPG_SUBJECTAREA on conf.SUBJECTAREA_ID equals sa.SUBJECTAREA_ID
                                   join sf in sponge_context.SPG_SUBFUNCTION on sa.SUBFUNCTION_ID equals sf.SUBFUNCTION_ID
                                   where sf.SUBFUNCTION_ID == subFunctionId && sa.SUBJECTAREA_ID == subjectAreaId
                                   && conf.ACTIVE_FLAG == active
                                   select new SearchDataList
                                   {
                                       ConfigId = conf.CONFIG_ID,
                                       SubjectAreaId = sa.SUBJECTAREA_ID,
                                       SubjectAreaName = sa.SUBJECTAREA_NAME,
                                       AssignedUser = u.Name,
                                       Active = conf.ACTIVE_FLAG == null ? "" : conf.ACTIVE_FLAG,
                                       EffectiveDate = conf.Created_On,
                                       ManualSendResendUrl = (conf.ACTIVE_FLAG == "null") ? "Configuration in Progress" : "Generate Template"
                                   }).Distinct().ToList();
            }
            else
            {
                SearchConfgData = (from conf in sponge_context.SPG_CONFIGURATION
                                   join u in sponge_context.SPG_USERS on conf.USER_ID equals u.USER_ID
                                   join sa in sponge_context.SPG_SUBJECTAREA on conf.SUBJECTAREA_ID equals sa.SUBJECTAREA_ID
                                   join sf in sponge_context.SPG_SUBFUNCTION on sa.SUBFUNCTION_ID equals sf.SUBFUNCTION_ID
                                   where sf.SUBFUNCTION_ID == subFunctionId && sa.SUBJECTAREA_ID == subjectAreaId && u.USER_ID == assignToUser
                                   && conf.ACTIVE_FLAG == active
                                   select new SearchDataList
                                   {
                                       ConfigId = conf.CONFIG_ID,
                                       SubjectAreaId = sa.SUBJECTAREA_ID,
                                       SubjectAreaName = sa.SUBJECTAREA_NAME,
                                       AssignedUser = u.Name,
                                       Active = conf.ACTIVE_FLAG == null ? "" : conf.ACTIVE_FLAG,
                                       EffectiveDate = conf.Created_On,
                                       ManualSendResendUrl = (conf.ACTIVE_FLAG == "null") ? "Configuration in Progress" : "Generate Template"
                                   }).Distinct().ToList();
            }
            return Json(SearchConfgData);
        }
        //public IActionResult ManualSend()
        //{
        //    return View("Views\\SearchTemplate\\ManualSendResend.cshtml");
        //}
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
       
    }
}