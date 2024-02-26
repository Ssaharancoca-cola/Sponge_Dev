using DAL.Common;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sponge.Common;
using Sponge.ViewModel;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    [Authorize(Roles = "Admin")]

    public class RoleController : Controller
    {
        public IActionResult CreateRole()
        {            
            return View();
        }
        public IActionResult ManageRole()
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            var roleList = sPONGE_Context.SPG_ROLE.Where(x => x.ACTIVE_FLAG == "Y").ToList();
            return View(roleList);            
        }
        public IActionResult SaveRole(SPG_ROLE data)
        {
            if (string.IsNullOrWhiteSpace(data.ROLE_NAME))
            {
                TempData["ErrorMsg"] = "Role Name is null or empty.";
                return View("CreateRole");
            }
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            try
            {
                    SPONGE_Context sPONGE_Context = new SPONGE_Context();
                    var SearchRoleData = (from func in sPONGE_Context.SPG_ROLE
                                          select new SearchFunctionList
                                          {
                                              CountryName = func.ROLE_NAME,
                                          }).ToList();
                    SearchRoleData = SearchRoleData.Where(s => s.CountryName == data.ROLE_NAME).ToList();

                    if (SearchRoleData.Count > 0)
                    {
                        TempData["ErrorMsg"] = "Role Name already exists.";
                        return View("CreateRole");
                    }

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
            catch (Exception ex)
            {
                ErrorLog lgerr = new ErrorLog();
                lgerr.LogErrorInTextFile(ex);
            }
            return View("CreateRole"); // return back to the same view with the model if not valid
        }

    }
}
