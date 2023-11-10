using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Protocol.Plugins;
using Sponge.ViewModel;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Policy;
using static System.Reflection.Metadata.BlobBuilder;

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
            ViewBag.SubFunction = new SelectList(subFnList.ToList(), "SUBFUNCTION_ID", "SUBFUNCTION_NAME");
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
        [HttpPost]
        public IActionResult UpdateUser(IFormCollection data)
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
            }
            catch (Exception ex) { }

            return View();
        }

        public IActionResult ManageUser() {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();

            using var cmd = sPONGE_Context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "[dbo].[SP_GET_USERS_DETAILS]";

            //common
            cmd.CommandType = CommandType.StoredProcedure;
            if (cmd.Connection.State != System.Data.ConnectionState.Open) cmd.Connection.Open();
           
            List<GetUserinfo> GUi = new List<GetUserinfo>();
           
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    GetUserinfo ui = new GetUserinfo();

                    ui.UserId = (string)reader["User_Id"];
                    ui.UserName = (string)reader["NAME"];
                    ui.Email = (string)reader["EMAIL_ID"];
                    ui.Role = (string)reader["ROLE_NAME"];
                    ui.SubFunction = (string)reader["SUBFUNCTION"];
                    ui.Status = (string)reader["ACTIVE_FLAG"];
                    GUi.Add(ui);
                }
            }
            return View(GUi);
        }

        public IActionResult EditUser(string id)
        {
            GetUserinfo userInfo = new GetUserinfo();
            using (SPONGE_Context sPONGE_Context = new SPONGE_Context())
            {
                var subFnList = (from f in sPONGE_Context.SPG_SUBFUNCTION
                                 from c in sPONGE_Context.SPG_USERS_FUNCTION.Where(user => user.SUB_FUNCTION_ID == f.SUBFUNCTION_ID).DefaultIfEmpty()

                                 select new  SPGSubfuncion
                                 {
                                     SubfunctionName = f.SUBFUNCTION_NAME,
                                     SubFunctionId =f.SUBFUNCTION_ID,
                                     Selected = c.SUB_FUNCTION_ID.HasValue,
                                 });

                var RoleList = (from f in sPONGE_Context.SPG_ROLE
                                 from c in sPONGE_Context.SPG_USERS_FUNCTION.Where(user => user.ROLE_ID == f.ROLE_ID).DefaultIfEmpty()

                                 select new SPGRole
                                 {
                                     RoleName = f.ROLE_NAME,
                                     RoleId = f.ROLE_ID,
                                     Selected = c.ROLE_ID.HasValue,
                                    }).Distinct();


                userInfo = (from u in sPONGE_Context.SPG_USERS
                                join f in sPONGE_Context.SPG_USERS_FUNCTION on u.USER_ID equals f.USER_ID
                                where u.USER_ID == id
                                select new GetUserinfo
                                {
                                    UserId = u.USER_ID,
                                    UserName = u.Name,
                                    Email = u.EMAIL_ID,
                                    Role =  f.ROLE_ID.ToString(),
                                    Status = f.ACTIVE_FLAG
                                }).FirstOrDefault();
                userInfo.SubfunctionList = subFnList.ToList();
                userInfo.RoleList = RoleList.ToList();

            }
            return View("~/Views/User/EditUser.cshtml", userInfo);

        }
    }
}
