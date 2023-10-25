using Microsoft.AspNetCore.Mvc;
using Sponge.Models;
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
        public IActionResult Cancel()
    {
        return View("Views\\SubjectArea\\ManageSubjectArea.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }




}
}