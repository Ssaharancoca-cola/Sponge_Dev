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

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
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

        public async Task<IActionResult> CreateFunction(int? InvalidEntry)
        {
            ViewBag.Countries = await PopulateCountries();
            ViewBag.ErrorMsg = InvalidEntry == 1 ? "Subfunction aleardy exist" : "";
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
            
            //var lst = spONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.COUNTRY_NAME }).Distinct();
            //ViewBag.Country = new SelectList(lst.ToList(), "COUNTRY_NAME", "COUNTRY_NAME");
            

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
                                                      FunctionName = func.FUNCTION_NAME,
                                                      SubFunctionName = func.SUBFUNCTION_NAME
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
                            return RedirectToAction("CreateFunction", new { InvalidEntry = 1 });
                        }
                    }
                    else { return View("CreateFunction",data); }

                }
                catch (Exception ex)
                {

                }
            }
            //else if (Request.Form["Command"] == "Update")
            //    {

            //    if (data.ACTIVE_FLAG == "on")
            //    {
            //        data.ACTIVE_FLAG = "Y";
            //    }
            //    else
            //    {
            //        data.ACTIVE_FLAG = "N";
            //    }
            //    using (SPONGE_Context sPONGE_Context = new SPONGE_Context())
            //    {
            //        SPG_SUBFUNCTION function2 = sPONGE_Context.SPG_SUBFUNCTION.Where(x => x.SUBFUNCTION_ID == Convert.ToInt16(data.SUBFUNCTION_ID)).FirstOrDefault();
            //        function2.MODIFIED_BY = userName[1].ToString();
            //        function2.MODIFIED_DATE = DateTime.Now;
            //        function2.ACTIVE_FLAG = data.ACTIVE_FLAG;

            //        sPONGE_Context.SaveChanges();
            //    }

            //}
            return RedirectToAction("Function");
        }

    }
}
