using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;

namespace Sponge.Common
{
    public class AccessFilters : ActionFilterAttribute
    {          
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userName = context.HttpContext.User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context context1 = new SPONGE_Context();
            var roleDetails = (from user in context1.SPG_USERS
                               join userFunc in context1.SPG_USERS_FUNCTION on user.USER_ID equals userFunc.USER_ID
                               join role in context1.SPG_ROLE on userFunc.ROLE_ID equals role.ROLE_ID
                               where user.USER_ID == userName[1]
                               orderby role.ROLE_ID descending
                               select new
                               {
                                   NAME = user.Name,
                                   ROLE = role.ROLE_NAME
                               }).FirstOrDefault();
            if (roleDetails == null)
            {
                context.Result = new RedirectResult("~/Home/AccessDenied");
            }
            base.OnActionExecuting(context);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var userName = context.HttpContext.User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context context1 = new SPONGE_Context();
            var roleDetails = (from user in context1.SPG_USERS
                               join userFunc in context1.SPG_USERS_FUNCTION on user.USER_ID equals userFunc.USER_ID
                               join role in context1.SPG_ROLE on userFunc.ROLE_ID equals role.ROLE_ID
                               where user.USER_ID == userName[1]
                               orderby role.ROLE_ID descending
                               select new
                               {
                                   NAME = user.Name,
                                   ROLE = role.ROLE_NAME
                               }).FirstOrDefault();

            if (roleDetails == null)
            {
                context.Result = new RedirectResult("~/Home/AccessDenied");
            }
            
            base.OnActionExecuted(context);
        }      
    }
}
