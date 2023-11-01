using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            var viewmodel = from sp in sPONGE_Context.SPG_SUBJECTAREA
                            join p in sPONGE_Context.SPG_SUBFUNCTION on sp.SUBFUNCTION_ID equals p.SUBFUNCTION_ID
                            select new SubjectArea { SpgSubjectArea = sp, SpgSubfunction = p };
            return View(viewmodel);           
        }
        public IActionResult CreateSubjectArea(int? InvalidEntry)
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_SUBJECTAREA.Select(o => new { o.SUBJECTAREA_NAME }).Distinct();
            ViewBag.SubjectArea = new SelectList(lst.ToList(), "SUBJECTAREA_NAME", "SUBJECTAREA_ID");
            var lst1 = spONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.FUNCTION_NAME }).Distinct();
            ViewBag.SubjectArea = new SelectList(lst.ToList(), "FUNCTION_NAME", "FUNCTION_ID");
            ViewBag.ErrorMsg = InvalidEntry == 1 ? "SubJectArea aleardy exist" : "";
            return View();            
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