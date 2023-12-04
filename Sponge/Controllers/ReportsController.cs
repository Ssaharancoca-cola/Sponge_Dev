using Microsoft.AspNetCore.Mvc;
using Sponge.Common;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult UploadedTemplateReport()
        {
            return View("Views\\Reports\\UploadedTemplateReports.cshtml");
        }
    }
}
