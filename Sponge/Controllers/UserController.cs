using DAL.Common;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Sponge.Common;
using Sponge.ViewModel;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;

namespace Sponge.Controllers
{
    //[AccessFilters]
    [SessionTimeOut]
    public class UserController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly PrincipalContext _context;
        public UserController(IMemoryCache cache)
        {
            _cache = cache;
            _context = new PrincipalContext(ContextType.Domain);
        }
        public IActionResult CreateUser()
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_ROLE.Select(o => new { o.ROLE_NAME, o.ROLE_ID }).Distinct();
            var subFnList = spONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.SUBFUNCTION_NAME, o.SUBFUNCTION_ID }).Distinct();
            ViewBag.Role = new SelectList(lst.ToList(), "ROLE_ID", "ROLE_NAME");
            ViewBag.SubFunction = new SelectList(subFnList.ToList(), "SUBFUNCTION_ID", "SUBFUNCTION_NAME");
            //FetchEmailsAndCache();
            return View();
        }

        // Fetching emails method
        public IActionResult FetchEmailsAndCache()
        {
            List<string> emailList = new List<string>();

            // Search for all UserPrincipal
            using (var searcher = new PrincipalSearcher(new UserPrincipal(_context)))
            {
                foreach (var result in searcher.FindAll())
                {
                    DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                    if (de.Properties["mail"].Value != null)
                    {
                        emailList.Add(de.Properties["mail"].Value.ToString());
                    }
                }
            }

            // Cache it for 1 hour
            _cache.Set("Emails", emailList, TimeSpan.FromHours(1));

            return Ok("Emails fetched and cached successfully.");
        }

        public ActionResult GetEmailSuggestions(string email)
        {
            List<string> matchingEmails = new List<string>();

            using (var context = new PrincipalContext(ContextType.Domain, "USAWS1ESI56.apac.ko.com"))
            {
                UserPrincipal user = new UserPrincipal(context);
                user.EmailAddress = email + "*";

                using (var searcher = new PrincipalSearcher(user))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        UserPrincipal foundUser = result as UserPrincipal;
                        if (foundUser != null)
                        {
                            matchingEmails.Add(foundUser.EmailAddress);
                        }
                    }
                }
            }

            return Json(matchingEmails);

        }
        // To get the user name suggestions
        public ActionResult GetUserNameSuggestions(string UserName)
        {
            List<string> matchingNames = new List<string>();

            using (var context = new PrincipalContext(ContextType.Domain, "USAWS1ESI56.apac.ko.com"))
            {
                UserPrincipal user = new UserPrincipal(context);
                user.DisplayName = UserName + "*"; //change this based on your AD schema

                using (var searcher = new PrincipalSearcher(user))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        UserPrincipal foundUser = result as UserPrincipal;
                        if (foundUser != null)
                        {
                            matchingNames.Add(foundUser.DisplayName);
                        }
                    }
                }
            }

            return Json(matchingNames);
        }

        [ActionName("GetUserInfo")]
        public UserInfo GetADUserInfo(string userEmailId)
        {
            UserInfo userInfo = new UserInfo();
            string pattern = @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex regex = new Regex(pattern);
            if (!regex.IsMatch(userEmailId))
            {
                userInfo.UserEmail = userEmailId.ToString();
                userInfo.UserName = "";
                userInfo.UserId = "";
                userInfo.ErrorMsg = "Please enter a valid email address";
                return userInfo;
            }
            else
            {

                SPONGE_Context spONGE_Context = new SPONGE_Context();
                int userid = spONGE_Context.SPG_USERS.Where(o => o.EMAIL_ID == userEmailId).Count();
                if (userid > 0)
                {
                    userInfo.UserEmail = userEmailId.ToString();
                    userInfo.UserName = "";
                    userInfo.UserId = "";
                    userInfo.ErrorMsg = "User already exists";

                }
                else
                {
                    try
                    {

                        using (var context = new PrincipalContext(ContextType.Domain, "USAWS1ESI56.apac.ko.com"))
                        {
                            UserPrincipal userPrincipal = new UserPrincipal(context);
                            userPrincipal.EmailAddress = userEmailId;

                            PrincipalSearcher search = new PrincipalSearcher(userPrincipal);

                            var user = (UserPrincipal)search.FindOne();

                            if (user != null)
                            {

                                userInfo.UserId = user.SamAccountName.ToString();
                                userInfo.UserName = user.DisplayName.ToString();
                                userInfo.UserEmail = user.EmailAddress.ToString();
                                userInfo.ErrorMsg = "";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog lgerr = new ErrorLog();
                        lgerr.LogErrorInTextFile(ex);
                    }
                }
                return userInfo;
            }

        }
        public UserInfo GetUserInfoByName(string userName)
        {
            UserInfo userInfo = new UserInfo();
            if (string.IsNullOrWhiteSpace(userName))
            {
                userInfo.ErrorMsg = "Enter valid user name.";
                return userInfo;
            }
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            int userid = spONGE_Context.SPG_USERS.Where(o => o.Name == userName).Count();
            if (userid > 0)
            {
                userInfo.UserEmail = "";
                userInfo.UserName = userName.ToString();
                userInfo.UserId = "";
                userInfo.ErrorMsg = "User already exists";

            }
            else
            {
                try
                {

                    using (var context = new PrincipalContext(ContextType.Domain, "USAWS1ESI56.apac.ko.com"))
                    {
                        UserPrincipal userPrincipal = new UserPrincipal(context);
                        userPrincipal.DisplayName = userName;

                        PrincipalSearcher search = new PrincipalSearcher(userPrincipal);

                        var user = (UserPrincipal)search.FindOne();

                        if (user != null)
                        {

                            userInfo.UserId = user.SamAccountName.ToString();
                            userInfo.UserName = user.DisplayName.ToString();
                            userInfo.UserEmail = user.EmailAddress.ToString();
                            userInfo.ErrorMsg = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog lgerr = new ErrorLog();
                    lgerr.LogErrorInTextFile(ex);
                }
            }
            return userInfo;

        }
        [HttpPost]
        public JsonResult SaveUser(IFormCollection data)
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

                string Role = data["Role"];
                string[] RoleArr = Role.Split(',');

                foreach (string role in RoleArr)
                {
                    foreach (string subFunction in subFunctionArr)
                    {
                        int subFunctionId = 0;
                        Int32.TryParse(subFunction, out subFunctionId);
                        int roleid = 0;
                        Int32.TryParse(role, out roleid);
                        int userid = sPONGE_Context.SPG_USERS_FUNCTION.Where(o => o.USER_ID == data["userId"].ToString() && o.SUB_FUNCTION_ID == subFunctionId).Count();
                        if (userid == 0)
                        {
                            SPG_USERS_FUNCTION sPG_1 = new SPG_USERS_FUNCTION();
                            {
                                sPG_1.ACTIVE_FLAG = data["status"];
                                sPG_1.USER_ID = data["userId"];

                                sPG_1.SUB_FUNCTION_ID = subFunctionId;

                                sPG_1.ROLE_ID = roleid;
                            }
                            sPONGE_Context.SPG_USERS_FUNCTION.Add(sPG_1);
                        }
                        else
                        {
                            return Json("User id is already exist");
                        }
                    }
                }
                sPONGE_Context.SaveChanges();
                return Json("User added successfully");
            }
            catch (Exception ex) {
                ErrorLog lgerr = new ErrorLog();
                lgerr.LogErrorInTextFile(ex);
            }
            return Json("Some error occured");
        }
        [HttpPost]
       //[Authorize(Policy = "RequireAdminRole")]
        public IActionResult UpdateUser(IFormCollection data)
        {
            string userId = data["userId"].ToString();
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

            try
            {
                using (SPONGE_Context sPONGE_Context = new SPONGE_Context())
                {
                    using (var dbContextTransaction = sPONGE_Context.Database.BeginTransaction())
                    {
                        try
                        {
                            var matchingRows = sPONGE_Context.SPG_USERS_FUNCTION.Where(u => u.USER_ID == userId).ToList();

                            foreach (var row in matchingRows)
                            {
                                sPONGE_Context.SPG_USERS_FUNCTION.Remove(row);
                            }

                            sPONGE_Context.SaveChanges();

                            string subFunctions = data["subFunction"];
                            string[] subFunctionArr = subFunctions.Split(',');

                            string Roles = data["Role"];
                            string[] RolesArr = Roles.Split(',');

                            foreach (var role in RolesArr)
                            {
                                foreach (var subFunction in subFunctionArr)
                                {
                                    SPG_USERS_FUNCTION newUserFunction = new SPG_USERS_FUNCTION
                                    {
                                        USER_ID = userId,
                                        ROLE_ID = int.Parse(role),
                                        SUB_FUNCTION_ID = int.Parse(subFunction),
                                        ACTIVE_FLAG = data["status"]
                                    };
                                    sPONGE_Context.SPG_USERS_FUNCTION.Add(newUserFunction);
                                }
                            }

                            sPONGE_Context.SaveChanges();

                            var user = sPONGE_Context.SPG_USERS.FirstOrDefault(u => u.USER_ID == userId);
                            if (user != null)
                            {
                                user.ACTIVE_FLAG = data["status"];
                                sPONGE_Context.SaveChanges();
                            }

                            dbContextTransaction.Commit();

                            return Json("User updated successfully");
                        }
                        catch (Exception ex)
                        {
                            dbContextTransaction.Rollback();
                            ErrorLog lgerr = new ErrorLog();
                            lgerr.LogErrorInTextFile(ex);
                            return Json("Error Occurred: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog lgerr = new ErrorLog();
                lgerr.LogErrorInTextFile(ex);
                return Json("Error Occurred: " + ex.Message);
            }
        }

        public IActionResult ManageUser()
        {
            SPONGE_Context context = new SPONGE_Context();
            List<GetUserinfo> UserInfo = new List<GetUserinfo>();
            var query = from U in context.SPG_USERS
                        join UF in context.SPG_USERS_FUNCTION on U.USER_ID equals UF.USER_ID into U_UF
                        from UF in U_UF.DefaultIfEmpty()
                        join SF in context.SPG_SUBFUNCTION on UF.SUB_FUNCTION_ID equals SF.SUBFUNCTION_ID into UF_SF
                        from SF in UF_SF.DefaultIfEmpty()
                        join R in context.SPG_ROLE on UF.ROLE_ID equals R.ROLE_ID into UF_R
                        from R in UF_R.DefaultIfEmpty()
                        select new
                        {
                            U,
                            UF,
                            SF,
                            R
                        } into x
                        group x by new
                        {
                            x.U.USER_ID,
                            x.U.EMAIL_ID,
                            x.U.Name,
                            x.U.ACTIVE_FLAG
                        }
                        into g
                        select new GetUserinfo
                        {
                            UserId = g.Key.USER_ID,
                            Email = g.Key.EMAIL_ID,
                            UserName = g.Key.Name,
                            Status = g.Key.ACTIVE_FLAG,
                            SubFunction = string.Join(", ", g.Select(x => x.SF.SUBFUNCTION_NAME).Distinct()),
                            Role = string.Join(", ", g.Select(x => x.R.ROLE_NAME).Distinct())
                        };

            UserInfo = query.ToList();


            return View(UserInfo);

        }

        public IActionResult EditUser(string id)
        {
            GetUserinfo userInfo = new GetUserinfo();
            using (SPONGE_Context sPONGE_Context = new SPONGE_Context())
            {
                var subFnList = (from f in sPONGE_Context.SPG_SUBFUNCTION
                                 from c in sPONGE_Context.SPG_USERS_FUNCTION.Where(user => user.SUB_FUNCTION_ID == f.SUBFUNCTION_ID && user.USER_ID == id).DefaultIfEmpty()

                                 select new SPGSubfuncion
                                 {
                                     SubfunctionName = f.SUBFUNCTION_NAME,
                                     SubFunctionId = f.SUBFUNCTION_ID,
                                     Selected = c.SUB_FUNCTION_ID.HasValue,
                                 }).Distinct();

                var RoleList = (from f in sPONGE_Context.SPG_ROLE
                                from c in sPONGE_Context.SPG_USERS_FUNCTION.Where(user => user.ROLE_ID == f.ROLE_ID && user.USER_ID == id).DefaultIfEmpty()

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

                                Status = u.ACTIVE_FLAG
                            }).FirstOrDefault();
                userInfo.SubfunctionList = subFnList.ToList();
                userInfo.RoleList = RoleList.ToList();

            }
            return View("~/Views/User/EditUser.cshtml", userInfo);

        }
    }
}
