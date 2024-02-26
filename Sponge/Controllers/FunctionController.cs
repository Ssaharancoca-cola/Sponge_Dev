using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sponge.Common;
using Sponge.ViewModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using System.Diagnostics.Metrics;
using System.Dynamic;
using System.Collections.Generic;
using DAL.Common;
using Microsoft.AspNetCore.Authorization;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    [Authorize(Roles = "Admin")]

    public class FunctionController : Controller
    {
        private readonly IHttpContextAccessor _httpSession;

        public FunctionController(IHttpContextAccessor httpContextAccessor)
        {
            _httpSession = httpContextAccessor;
        }

        public IActionResult Function()
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            return View(sPONGE_Context.SPG_SUBFUNCTION.ToList());
        }

        public async Task<IActionResult> CreateFunction()
        {
            ViewBag.Countries = await PopulateCountries();
            return View();
        }
        public async Task<List<string>> PopulateCountries()
        {
            var countries = new List<string>();
            SPONGE_Context spONGE_Context = new SPONGE_Context(); 

            using (var command = spONGE_Context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "dbo.SP_COUNTRY_LIST"; 
                command.CommandType = CommandType.StoredProcedure;

                spONGE_Context.Database.OpenConnection();

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        countries.Add(result.GetString(result.GetOrdinal("ENT_COUNTRY_SHORT_NAME")));
                    }
                }
            }

            spONGE_Context.Database.CloseConnection();

            return countries;
        }


        public async Task<IActionResult> EditFunction(int id)
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            var lst = sPONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.COUNTRY_NAME }).Distinct();
            SPG_SUBFUNCTION function = sPONGE_Context.SPG_SUBFUNCTION.Where(x => x.SUBFUNCTION_ID == id).FirstOrDefault();
            return View(function);
        }
        [HttpPost]
        public IActionResult EditFunction(SPG_SUBFUNCTION data)
        {
            if (ModelState.IsValid)
            {
                SPONGE_Context sPONGE_Context = new SPONGE_Context();
                if (data.ACTIVE_FLAG == "on")
                {
                    data.ACTIVE_FLAG = "Y";
                }
                else
                {
                    data.ACTIVE_FLAG = "N";
                }
                sPONGE_Context.Entry(data).State = EntityState.Modified;
                sPONGE_Context.SaveChanges();
                return RedirectToAction("Function");
            }
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult >SaveFunction( SPG_SUBFUNCTION data)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
           
            if (Request.Form["Command"]=="Save")
            {
                try
                {
                    ViewBag.Countries = await PopulateCountries();
                    if (ModelState.IsValid)
                    {
                        SPONGE_Context sPONGE_Context = new SPONGE_Context();
                        var SearchFunctionData = (from func in sPONGE_Context.SPG_SUBFUNCTION
                                                  select new SearchFunctionList
                                                  {
                                                      CountryName = func.COUNTRY_NAME,
                                                      FunctionName = func.FUNCTION_NAME.Trim(),
                                                      SubFunctionName = func.SUBFUNCTION_NAME.Trim()
                                                  }).ToList();
                        SearchFunctionData = SearchFunctionData.Where(s => s.CountryName == data.COUNTRY_NAME).ToList();
                        SearchFunctionData = SearchFunctionData.Where(s => s.FunctionName == data.FUNCTION_NAME).ToList();
                        SearchFunctionData = SearchFunctionData.Where(s => s.SubFunctionName == data.SUBFUNCTION_NAME).ToList();
                        if (SearchFunctionData.Count <= 0)
                        {                            
                            if (data.ACTIVE_FLAG == "on")
                            {
                                data.ACTIVE_FLAG = "Y";
                            }
                            else
                            {
                                data.ACTIVE_FLAG = "N";
                            }
                            data.CREATED_DATE = DateTime.Now;
                            data.CREATED_BY = userName[1].ToString();                                                      
                            sPONGE_Context.SPG_SUBFUNCTION.Add(data);

                            sPONGE_Context.SaveChanges();
                            return RedirectToAction("Function");
                        }
                        else
                        {
                            TempData["ErrorMsg"] = "Sub function already exist.";
                            return View("CreateFunction");
                        }
                    }
                    else { return View("CreateFunction",data); }

                }
                catch (Exception ex)
                {
                    ErrorLog lgerr = new ErrorLog();
                    lgerr.LogErrorInTextFile(ex);
                }
            }
            
            return RedirectToAction("Function");
        }

    }
}
