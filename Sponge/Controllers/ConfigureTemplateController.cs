using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sponge.Common;
using Sponge.Models;
using Sponge.ViewModel;
using System.Data;
using System.Diagnostics;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ConfigureTemplateController : Controller
{
    private readonly ILogger<HomeController> _logger;

        public ConfigureTemplateController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult ConfigureTemplate()
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_SUBJECTAREA.Select(o => new { o.SUBJECTAREA_NAME, o.SUBJECTAREA_ID }).Distinct();
            ViewBag.SubjectArea = new SelectList(lst.ToList(), "SUBJECTAREA_NAME", "SUBJECTAREA_ID");

            
            return View();
        }
        public IActionResult SetUp(string id)
        {

            return View("Views\\ConfigureTemplate\\Setup.cshtml");
        }
       [HttpGet]
       public IActionResult GetUserList(int subjectAreaId)
        {
           
            SPONGE_Context context = new();

            
            var usernames = from U in context.SPG_USERS
                            join SPG in context.SPG_CONFIGURATION on U.USER_ID equals SPG.USER_ID
                            where SPG.SUBJECTAREA_ID == subjectAreaId
                            group U by new { U.USER_ID, U.EMAIL_ID, U.Name, U.ACTIVE_FLAG } into g
                            select new
                            {
                                userid = g.Key.USER_ID,
                                username = g.Key.Name,
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