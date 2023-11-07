using Microsoft.AspNetCore.Mvc;

namespace Sponge.Controllers
{
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
