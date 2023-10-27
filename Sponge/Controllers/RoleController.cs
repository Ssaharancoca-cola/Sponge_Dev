using Microsoft.AspNetCore.Mvc;

namespace Sponge.Controllers
{
    public class RoleController : Controller
    {
        public IActionResult CreateRole()
        {
            return View();
        }
        public IActionResult ManageRole()
        {
            return View();
        }
    }
}
