using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sponge.ViewModel;

namespace Sponge.Controllers
{
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

        public IActionResult CreateFunction(int? InvalidEntry)
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.COUNTRY_NAME }).Distinct();
            ViewBag.Country = new SelectList(lst.ToList(), "COUNTRY_NAME", "COUNTRY_NAME");
            ViewBag.ErrorMsg = InvalidEntry == 1 ? "Subfunction aleardy exist" : "";
            return View();
        }

        public IActionResult EditFunction(int id)
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            var lst = sPONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.COUNTRY_NAME }).Distinct();
            ViewBag.Country = new SelectList(lst.ToList(), "COUNTRY_NAME", "COUNTRY_NAME");
            SPG_SUBFUNCTION function = sPONGE_Context.SPG_SUBFUNCTION.Where(x => x.SUBFUNCTION_ID == id).FirstOrDefault();
            return View("~/Views/Function/CreateFunction.cshtml", function);
        }
        [HttpPost]
        public IActionResult SaveFunction(SPG_SUBFUNCTION data)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
           
            if (Request.Form["Command"]=="Save")
            {
                try
                {
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
                            string activeFlag;
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
                            //SPG_SUBFUNCTION spg = new SPG_SUBFUNCTION()
                            //{
                            //    COUNTRY_NAME = function["Country"],
                            //    FUNCTION_NAME = function["FunctionName"],
                            //    SUBFUNCTION_NAME = function["SubFunctionName"],
                            //    ACTIVE_FLAG = activeFlag,
                            //    CREATED_BY = userName[1].ToString(),                                
                            //    CREATED_DATE = DateTime.Now
                            //};                            
                            sPONGE_Context.SPG_SUBFUNCTION.Add(data);

                            sPONGE_Context.SaveChanges();
                            return RedirectToAction("Function");
                        }
                        else
                        {                                                     
                            return RedirectToAction("CreateFunction", new { InvalidEntry = 1 });
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
            else if (Request.Form["Command"] == "Update")
                {
               
                if (data.ACTIVE_FLAG == "on")
                {
                    data.ACTIVE_FLAG = "Y";
                }
                else
                {
                    data.ACTIVE_FLAG = "N";
                }
                using (SPONGE_Context sPONGE_Context = new SPONGE_Context())
                {
                    SPG_SUBFUNCTION function2 = sPONGE_Context.SPG_SUBFUNCTION.Where(x => x.SUBFUNCTION_ID == Convert.ToInt16(data.SUBFUNCTION_ID)).FirstOrDefault();
                    function2.MODIFIED_BY = userName[1].ToString();
                    function2.MODIFIED_DATE = DateTime.Now;
                    function2.ACTIVE_FLAG = data.ACTIVE_FLAG;
                    
                    sPONGE_Context.SaveChanges();
                }

            }
            return RedirectToAction("Function");
        }

        public JsonResult GetAllFunction(string namePrefix)
        {
            List<string>? list = null;
            using (SPONGE_Context sPONGE_Context = new SPONGE_Context())
            {
                list = sPONGE_Context.SPG_SUBFUNCTION.Where(s => s.FUNCTION_NAME.ToUpper().Contains(namePrefix.ToUpper())).Select(y => y.FUNCTION_NAME).Distinct().ToList();
            }
            return Json(list);
        }

        public JsonResult IsSubFunctionAlreadyExist(string functionName, string subFunctionName, int subFunctionId)
        {
            bool result = false;
            using (SPONGE_Context sPONGE_Context = new SPONGE_Context())
            {
                result = sPONGE_Context.SPG_SUBFUNCTION.Where(s => s.FUNCTION_NAME.ToUpper() == functionName.ToUpper() && s.SUBFUNCTION_NAME.ToUpper() == subFunctionName.ToUpper() && s.SUBFUNCTION_ID != subFunctionId).Any();
            }
            return Json(result);
        }

    }
}
