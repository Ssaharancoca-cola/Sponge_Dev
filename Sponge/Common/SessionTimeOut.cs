using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sponge.Common
{
    public class SessionTimeOut : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var name = context.HttpContext.Session.GetString("NAME");
            var role = context.HttpContext.Session.GetString("ROLE");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectResult("~/Home/SessionTimeOut");
                return;
            }
            base.OnActionExecuting(context);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var name = context.HttpContext.Session.GetString("NAME");
            var role = context.HttpContext.Session.GetString("ROLE");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectResult("~/Home/SessionTimeOut");
                return;
            }
            base.OnActionExecuted(context);
        }
    }
}
