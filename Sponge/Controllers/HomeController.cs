using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Sponge.Models;
using System.Diagnostics;

namespace Sponge.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context context = new SPONGE_Context();
            var userDetails = (from user in context.SPG_USERS
                               join userFunc in context.SPG_USERS_FUNCTION on user.USER_ID equals userFunc.USER_ID
                               join role in context.SPG_ROLE on userFunc.ROLE_ID equals role.ROLE_ID
                               where user.USER_ID == userName[1]
                               orderby role.ROLE_ID descending
                               select new
                               {
                                   NAME = user.Name,
                                   ROLE = role.ROLE_NAME,
                               }).FirstOrDefault();

            if (userDetails == null)
            {
                return View("~/Views/Shared/AccessDenied.cshtml");
            }
            HttpContext.Session.SetString("ROLE", userDetails.ROLE);
            HttpContext.Session.SetString("NAME", userDetails.NAME);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ViewResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }
       
    }
}