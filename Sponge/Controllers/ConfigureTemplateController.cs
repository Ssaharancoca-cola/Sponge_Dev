using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Sponge.Common;
using Sponge.Models;
using Sponge.ViewModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Globalization;

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
               var  result = context.SPG_CONFIGURATION
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
                            configUser.CONFIGURATION.Created_On

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
                configRecord.SCHEDULED = data.SCHEDULED;
                configRecord.LOCK_DATE = data.LOCK_DATE;
                configRecord.PATTERN = data.PATTERN;
                configRecord.DATA_COLLECTION = "OFFLINE";
                configRecord.PATTERN_MONTH = data.PATTERN_MONTH;
                configRecord.REMMINDER_DATE = data.REMMINDER_DATE;
                configRecord.ESCALATION_DATE = data.ESCALATION_DATE;
                configRecord.APPROVER_NAME = data.APPROVER_NAME;
                configRecord.APPROVER_EMAILD =data.APPROVER_EMAILID;
                configRecord.APPROVER_ID = data.APPROVER_ID;
                configRecord.MODIFIED_BY = userName[1];
                configRecord.MODIFIED_DATE = DateTime.Now;
                if (configRecord.EFFECTIVE_TO == null)
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
                            group U by new { SPG.CONFIG_ID, U.Name, U.USER_ID, SPG.ACTIVE_FLAG} into g
                            select new
                            {
                                configID = g.Key.CONFIG_ID,
                                username = g.Key.Name,
                                activeflag  = g.Key.ACTIVE_FLAG
                            };
         

            // UserInfo = query.ToList();
            return Json(usernames);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




}
}