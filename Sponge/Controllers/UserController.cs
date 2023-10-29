using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sponge.ViewModel;
using System.DirectoryServices.AccountManagement;

namespace Sponge.Controllers
{
    public class UserController : Controller
    {
        public IActionResult CreateUser()
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_ROLE.Select(o => new { o.ROLE_NAME, o.ROLE_ID }).Distinct();
            var subFnList = spONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.SUBFUNCTION_NAME, o.SUBFUNCTION_ID }).Distinct();
            ViewBag.Role = new SelectList(lst.ToList(), "ROLE_NAME", "ROLE_ID");
            ViewBag.SubFunction = new SelectList(subFnList.ToList(), "SUBFUNCTION_NAME", "SUBFUNCTION_ID");
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
        [HttpPost]
        public IActionResult SaveUser(IFormCollection data)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            try
            {
                SPONGE_Context sPONGE_Context = new SPONGE_Context();

                SPG_USERS sPG_ = new SPG_USERS();
                {
                    sPG_.USER_ID = data["userId"];
                    sPG_.Name = data["userName"];
                    sPG_.EMAIL_ID = data["email"];
                    sPG_.ACTIVE_FLAG = data["status"];
                    sPG_.CREATED_DATE = DateTime.Now;
                    sPG_.CREATED_BY = userName[1].ToString();
                }
                sPONGE_Context.SPG_USERS.Add(sPG_);                

                string subFunctions = data["subFunction"];
                string[] subFunctionArr = subFunctions.Split(',');

                int roleId = 0;
                Int32.TryParse(data["role"], out roleId);

                foreach (string subFunction in subFunctionArr)
                {
                    SPG_USERS_FUNCTION sPG_1 = new SPG_USERS_FUNCTION();
                    {
                        sPG_1.ACTIVE_FLAG = data["status"];
                        sPG_1.USER_ID = data["userId"];
                        int subFunctionId = 0;
                        Int32.TryParse(subFunction, out subFunctionId);
                        sPG_1.SUB_FUNCTION_ID = subFunctionId;
                        sPG_1.ROLE_ID = roleId;
                    }
                    sPONGE_Context.SPG_USERS_FUNCTION.Add(sPG_1);
                }

                sPONGE_Context.SaveChanges();
            } catch (Exception ex) { }

            return View();
        }

        public IActionResult ManageUser() {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();

            var userInfo = (from u in sPONGE_Context.SPG_USERS
                            join f in sPONGE_Context.SPG_USERS_FUNCTION on u.USER_ID equals f.USER_ID                            
                            select new GetUserinfo
                            {                                
                                UserId = u.USER_ID,
                                UserName = u.Name,
                                Email = u.EMAIL_ID,
                                Role = f.ROLE_ID.ToString(),
                                SubFunction = f.SUB_FUNCTION_ID.ToString(),
                                Status = f.ACTIVE_FLAG
                            }).ToList();

            return View(userInfo);
        }

        public IActionResult EditUser(string id)
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();

            var userInfo = (from u in sPONGE_Context.SPG_USERS
                            join f in sPONGE_Context.SPG_USERS_FUNCTION on u.USER_ID equals f.USER_ID
                            where u.USER_ID == id
                            select new GetUserinfo
                            {
                                UserId = u.USER_ID,
                                UserName = u.Name,
                                Email = u.EMAIL_ID,
                                Role = f.ROLE_ID.ToString(),
                                SubFunction = f.SUB_FUNCTION_ID.ToString(),
                                Status = f.ACTIVE_FLAG
                            }).ToList();
            return View("~/Views/User/CreateUser.cshtml", userInfo);

        }
    }
}
