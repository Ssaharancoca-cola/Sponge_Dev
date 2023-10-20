using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Sponge.ViewComponents
{
    public class ManageUser : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()

        {
            return View("~/Views/Landing/ManageUser.cshtml");
        }
    }
}