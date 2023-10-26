using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sponge.ViewModel;
using System.DirectoryServices.AccountManagement;

namespace Sponge.Controllers
{
    public class UserController : Controller
    {
        public IActionResult CreateUser()
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_ROLE.Select(o => new { o.ROLE_NAME }).Distinct();
            var subFnList = spONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.SUBFUNCTION_NAME }).Distinct();
            ViewBag.Role = new SelectList(lst.ToList(), "ROLE_NAME", "ROLE_NAME");
            ViewBag.SubFunction = new SelectList(subFnList.ToList(), "SUBFUNCTION_NAME", "SUBFUNCTION_NAME");
            return View();
        }
        public IActionResult ManageUser()
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            return View();

        }
        [ActionName("GetUserInfo")]
            public UserInfo GetADUserInfo(string userId)
            {
                UserInfo userInfo = new UserInfo();
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            int userid = spONGE_Context.SPG_USERS.Where(o =>  o.USER_ID==userId ).Count();
            if (userid>0)
            {
                userInfo.UserId = userId.ToString();
                userInfo.UserName = "";
                userInfo.UserEmail = "";
                userInfo.ErrorMsg = "User already exists";

            }
            else
            {
                try
                {
                    using (var context = new PrincipalContext(ContextType.Domain, "USAWS1ESI56.apac.ko.com"))
                    {
                        var user = UserPrincipal.FindByIdentity(context, userId);

                        if (user != null)
                        {
                          
                            userInfo.UserId = user.Name.ToString();
                            userInfo.UserName = user.DisplayName.ToString();
                            userInfo.UserEmail = user.EmailAddress.ToString();
                            userInfo.ErrorMsg = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
                return userInfo;
            }
        



    }
}
