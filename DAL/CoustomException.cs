﻿using System.Net;
using DAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace DAL
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception here
            System.Diagnostics.Debug.WriteLine(exception);

            //context.Response.ContentType = "application/json";
            //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            //var response = new { error = exception.Message };
            ////var payload = JsonConvert.SerializeObject(response);
            //var payload = "error occurred please connect with Sponge Admin team";
            //return context.Response.WriteAsync(payload);
            // Log the exception here
            System.Diagnostics.Debug.WriteLine(exception);
            ErrorLog srsEx = new ErrorLog();
            srsEx.LogErrorInTextFile(exception);
            context.Response.Redirect("/ErrorPage");

            return Task.CompletedTask;
        }
    }
}