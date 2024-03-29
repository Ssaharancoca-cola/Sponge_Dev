﻿using DAL.Models;
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
                               orderby role.ROLE_PRIORITY ascending
                               select new
                               {
                                   NAME = user.Name,
                                   ROLE = role.ROLE_NAME,
                                   PRIORITY = role.ROLE_PRIORITY
                               }).FirstOrDefault();
            var activeFlag = (from user in context1.SPG_USERS
                              where user.USER_ID == userName[1]
                              select new
                              {
                                  ACTIVE_FLAG = user.ACTIVE_FLAG
                              }).FirstOrDefault();

            if (roleDetails == null || activeFlag.ACTIVE_FLAG == "N")
            {
                context.Result = new RedirectResult("~/Home/AccessDenied");
            }
            context.HttpContext.Session.SetString("ROLE", roleDetails.ROLE);
            context.HttpContext.Session.SetString("NAME", roleDetails.NAME);

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
                               orderby role.ROLE_PRIORITY ascending
                               select new
                               {
                                   NAME = user.Name,
                                   ROLE = role.ROLE_NAME,
                                   PRIORITY = role.ROLE_PRIORITY
                               }).FirstOrDefault();

            if (roleDetails == null)
            {
                context.Result = new RedirectResult("~/Home/AccessDenied");
            }
            context.HttpContext.Session.SetString("ROLE", roleDetails.ROLE);
            context.HttpContext.Session.SetString("NAME", roleDetails.NAME);
            base.OnActionExecuted(context);
        }      
    }
}
