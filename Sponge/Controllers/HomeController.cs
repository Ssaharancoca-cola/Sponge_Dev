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
            return View();
        }

        public IActionResult ManageFunction()
        {
            return View("Views\\Landing\\ManageFunction.cshtml");
        }
        public IActionResult ManageRole()
        {
            return View("Views\\Landing\\ManageRole.cshtml");
        }
        public IActionResult CreateRole()
        {
            return View("Views\\Landing\\CreateRole.cshtml");
        }
        public IActionResult ManageSubjectArea()
        {
            return View("Views\\SubjectArea\\ManageSubjectArea.cshtml");
        }
        public IActionResult CreateSubjectArea()
        {
            return View("Views\\SubjectArea\\CreateSubjectArea.cshtml");
        }
        public IActionResult CreateFunction()
        {
            return View("Views\\Landing\\CreateFunction.cshtml");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult ManageUser()
        {
            return View("Views\\Landing\\ManageUser.cshtml");
        }
        public IActionResult CreateUser()
        {
            return View("Views\\Landing\\CreateUser.cshtml");
        }

        //public async Task<IActionResult> ManageUser()
        //{

        //    return await Task.Run(() => ViewComponent("ManageUser",
        //            new
        //            {

        //            }));
        //}
    }
}