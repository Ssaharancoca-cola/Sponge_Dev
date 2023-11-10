using Microsoft.AspNetCore.Mvc;
using Sponge.Common;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ApprovalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MyApproval()
        {
            return View();
        }
    }
}
