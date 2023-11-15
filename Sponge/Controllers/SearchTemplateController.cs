using Microsoft.AspNetCore.Mvc;
using Sponge.Common;
using Sponge.Models;
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
            return View();
        }

        public IActionResult ManualSend()
        {
            return View("Views\\SearchTemplate\\ManualSendResend.cshtml");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
       
    }
}