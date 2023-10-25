using DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace Sponge.Controllers
{
    public class SubjectAreaController : Controller
    {
        public IActionResult CreateSubjectArea()
        {
            return View();
        }
        public IActionResult ManageSubjectArea()
        {
            
            return View();
        }
    }
}
