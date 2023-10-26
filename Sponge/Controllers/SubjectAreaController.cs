using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Sponge.Models;
using Sponge.ViewModel;
using System.Diagnostics;

namespace Sponge.Controllers
{ 
    public class SubjectAreaController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public SubjectAreaController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

        public IActionResult ManageSubjectArea()
        {
            return View("Views\\SubjectArea\\ManageSubjectArea.cshtml");
        }
        public IActionResult CreateSubjectArea()
        {
            return View("Views\\SubjectArea\\CreateSubjectArea.cshtml");
        }
        public IActionResult ConfigureSubjectArea()
        {
            return View("Views\\SubjectArea\\ConfigureSubjectArea.cshtml");
        }
        public IActionResult SaveMastersGroup()
        {
            return View("Views\\SubjectArea\\ConfigureMasters.cshtml");
        }
        public IActionResult SaveMasters()
        {
            return View("Views\\SubjectArea\\ConfigureDataCollection.cshtml");
        }
        public IActionResult SaveDataCollection()
        {
            return View("Views\\SubjectArea\\AssignUsers.cshtml");
        }
        public IActionResult SaveUsers()
        {
            return View("Views\\Home\\Index.cshtml");
        }
        [HttpPost]
        public IActionResult ConfigureMasterGroup(IFormCollection function, string Command)
        {
          
            return RedirectToAction("Function");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }




}
}