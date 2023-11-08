using Microsoft.AspNetCore.Mvc;
using Sponge.Common;
using Sponge.Models;
using System.Diagnostics;

namespace Sponge.Controllers
{
    [AccessFilters]
    public class UploadController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public UploadController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upload()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
       
    }
}