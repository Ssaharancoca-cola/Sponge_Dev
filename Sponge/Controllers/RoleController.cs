using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Sponge.Common;
using Sponge.ViewModel;

namespace Sponge.Controllers
{
    [AccessFilters]
    public class RoleController : Controller
    {
        public IActionResult CreateRole(int? InvalidEntry)
        {            
            ViewBag.ErrorMsg = InvalidEntry == 1 ? "Role aleardy exist" : "";
            return View();
        }
        public IActionResult ManageRole()
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            return View(sPONGE_Context.SPG_ROLE.ToList());            
        }
        public IActionResult SaveRole(SPG_ROLE data)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            try
            {
                if (ModelState.IsValid)
                {
                    SPONGE_Context sPONGE_Context = new SPONGE_Context();
                    var SearchRoleData = (from func in sPONGE_Context.SPG_ROLE
                                              select new SearchFunctionList
                                              {
                                                  CountryName = func.ROLE_NAME,                                                 
                                              }).ToList();
                    SearchRoleData = SearchRoleData.Where(s => s.CountryName == data.ROLE_NAME).ToList();
                    
                    if (SearchRoleData.Count <= 0)
                    {
                        string activeFlag;
                        if (data.ACTIVE_FLAG == "on")
                        {
                            data.ACTIVE_FLAG = "Y";
                        }
                        else
                        {
                            data.ACTIVE_FLAG = "N";
                        }
                        data.CREATED_ON = DateTime.Now;
                        data.CREATED_BY = userName[1].ToString();

                        sPONGE_Context.SPG_ROLE.Add(data);

                        sPONGE_Context.SaveChanges();
                        return RedirectToAction("ManageRole");
                    }
                    else
                    {
                        return RedirectToAction("CreateRole", new { InvalidEntry = 1 });
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("ManageRole");
        }


    }
}
