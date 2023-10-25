using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            ViewBag.ErrorMsg = InvalidEntry == 1 ? "This entry aleardy exist, use edit or try with another value." : "";
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
        public IActionResult SaveFunction(IFormCollection function, string Command)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

            if (Command == "Save")
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
                        SearchFunctionData = SearchFunctionData.Where(s => s.CountryName == function["Country"]).ToList();
                        SearchFunctionData = SearchFunctionData.Where(s => s.FunctionName == function["FunctionName"]).ToList();
                        SearchFunctionData = SearchFunctionData.Where(s => s.SubFunctionName == function["SubFunctionName"]).ToList();
                        if (SearchFunctionData.Count <= 0)
                        {
                            SPG_SUBFUNCTION spg = new SPG_SUBFUNCTION()
                            {
                                COUNTRY_NAME = function["Country"],
                                FUNCTION_NAME = function["FunctionName"],
                                SUBFUNCTION_NAME = function["SubFunctionName"],
                                ACTIVE_FLAG = function["ActiveFlag"],
                                CREATED_BY = userName[1].ToString(),
                                CREATED_DATE = DateTime.Now
                            };                            
                            sPONGE_Context.SPG_SUBFUNCTION.Add(spg);
                            sPONGE_Context.SaveChanges();
                            return RedirectToAction("Function");
                        }
                        else
                        {
                            return RedirectToAction("SaveFunction", new { InvalidEntry = 1 });
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
            else if (Command == "Update")
            {
                using (SPONGE_Context sPONGE_Context = new SPONGE_Context())
                {
                    SPG_SUBFUNCTION function2 = sPONGE_Context.SPG_SUBFUNCTION.Where(x => x.SUBFUNCTION_ID == Convert.ToInt16(function["SUBFUNCTION_ID"])).FirstOrDefault();
                    function2.MODIFIED_BY = userName[1].ToString();
                    function2.MODIFIED_DATE = DateTime.Now;
                    function2.FUNCTION_NAME = function["FunctionName"];
                    function2.COUNTRY_NAME = function["Country"];
                    function2.SUBFUNCTION_NAME = function["SubFunctionName"];

                    function2.ACTIVE_FLAG = function["ActiveFlag"];
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
