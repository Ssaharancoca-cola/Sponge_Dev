using DAL.Common;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sponge.Common;
using Sponge.Models;
using Sponge.ViewModel;
using System.Data;
using System.Diagnostics;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class SubjectAreaController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public SubjectAreaController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ManageSubjectArea()
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            var viewmodel = from sp in sPONGE_Context.SPG_SUBJECTAREA
                            join p in sPONGE_Context.SPG_SUBFUNCTION on sp.SUBFUNCTION_ID equals p.SUBFUNCTION_ID
                            select new SubjectArea { SpgSubjectArea = sp, SpgSubfunction = p };

            return View(viewmodel);
        }
        public IActionResult CreateSubjectArea(int? InvalidEntry)
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst1 = spONGE_Context.SPG_SUBFUNCTION.Select(o => new { o.SUBFUNCTION_NAME, o.SUBFUNCTION_ID }).Distinct();
            ViewBag.SubFunction = new SelectList(lst1.ToList(), "SUBFUNCTION_NAME", "SUBFUNCTION_ID");
            List<SelectListItem> timelvl = new List<SelectListItem>();
            SelectListItem item = new SelectListItem();
            ViewBag.Timelevel = timelvl;
            ViewBag.ErrorMsg = InvalidEntry == 1 ? "SubjectArea already exist" : "";
            SubjectArea spg = new SubjectArea
            {
                SpgSubfunction = new SPG_SUBFUNCTION(),
                SpgSubjectArea = new SPG_SUBJECTAREA()
            };

            return View(spg);
        }
        public IActionResult EditSubjectArea(int id)
        {
            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            var viewmodel = (from sp in sPONGE_Context.SPG_SUBJECTAREA
                             join p in sPONGE_Context.SPG_SUBFUNCTION on sp.SUBFUNCTION_ID equals p.SUBFUNCTION_ID
                             where sp.SUBJECTAREA_ID == id
                             select new SubjectArea { SpgSubjectArea = sp, SpgSubfunction = p }).FirstOrDefault();
            return View(viewmodel);
        }
        [HttpPost]
        public IActionResult UpdateSubjectArea(SPG_SUBJECTAREA spg)
        {
            if (spg == null || spg.SUBJECTAREA_ID == 0)
            {
                return Json(new { success = false, message = "Invalid Subject Area data." });
            }

            try
            {
                SPONGE_Context spONGE_Context = new SPONGE_Context();
                string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                var lst1 = spONGE_Context.SPG_SUBJECTAREA.FirstOrDefault(x => x.SUBJECTAREA_ID == spg.SUBJECTAREA_ID);

                if (lst1 != null)
                {
                    lst1.ACTIVE_FLAG = spg.ACTIVE_FLAG;
                    lst1.MODIFIED_BY = userName[1];
                    lst1.MODIFIED_DATE = DateTime.Now;
                    spONGE_Context.SaveChanges();
                    return Json(new { success = true, message = "Subject Area updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Subject Area not found." });
                }
            }
            catch (Exception ex)
            {
                ErrorLog lgerr = new ErrorLog();
                lgerr.LogErrorInTextFile(ex);
                return Json(new { success = false, message = "An error occurred while updating the Subject Area." });
            }
        }
        [HttpPost]
        public IActionResult SaveSubjectArea(SPG_SUBJECTAREA data)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

            try
            {
                SPONGE_Context sPONGE_Context = new SPONGE_Context();
                var SearchSubjectAreaData = (from func in sPONGE_Context.SPG_SUBJECTAREA
                                             select new SearchFunctionList
                                             {
                                                 SubjectAreaName = func.SUBJECTAREA_NAME
                                             }).ToList();
                SearchSubjectAreaData = SearchSubjectAreaData.Where(s => s.SubjectAreaName == data.SUBJECTAREA_NAME).ToList();

                if (SearchSubjectAreaData.Count <= 0)
                {
                    data.VERSION = data.VERSION == "true" ? "Y" : "N";
                    if (data.VERSION == "N")
                    {
                        data.ONTIMELEVEL = "0";
                    }
                    // string PERIOD = GetPeriod(data.FREQUENCY, data.TIME_LEVEL);
                    string PERIOD = sPONGE_Context.SPG_GET_PERIOD.Where(s => s.FREQUENCY == data.FREQUENCY && s.TIME_LEVEL == data.TIME_LEVEL).Select(s => s.PERIOD).FirstOrDefault().ToString();

                    data.SUBJECTAREA_TABLE = "SPG_" + data.SUBJECTAREA_NAME;
                    data.PERIOD = PERIOD;
                    data.CREATED_DATE = DateTime.Now;
                    data.CREATED_BY = userName[1].ToString();
                    sPONGE_Context.SPG_SUBJECTAREA.Add(data);
                    sPONGE_Context.SaveChanges();

                    return Json(new { success = true, message = "Data Saved Successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Subject area already exists." });
                }

            }
            catch (Exception ex)
            {
                ErrorLog lgerr = new ErrorLog();
                lgerr.LogErrorInTextFile(ex);
                return Json(new { success = false, message = "An error occurred while saving the data." });
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public JsonResult BindGranularity(string frequency)
        {
            List<SelectListItem> lstItems = new List<SelectListItem>();
            ViewBag.GranularityList = GetGranularityListTime(frequency);
            lstItems = GetGranularityListTime(frequency);
            return Json(lstItems);
        }
        public JsonResult BindTimeLevel(string frequency)
        {
            List<SelectListItem> lstItems = new List<SelectListItem>();
            ViewBag.Time = GetGranularityList(frequency);
            lstItems = GetGranularityList(frequency);
            return Json(lstItems);
        }
        private List<SelectListItem> GetGranularityList(string frequency)
        {
            List<SelectListItem> lstItems = new List<SelectListItem>();
            SelectListItem items;
            try
            {
                if (frequency.ToUpper().Trim() == "YEARLY")
                {
                    items = new SelectListItem();
                    items.Text = "Half Yearly";
                    items.Value = "HALF_YEARLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Quarterly";
                    items.Value = "QUARTERLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Monthly";
                    items.Value = "MONTHLY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "HALF_YEARLY")
                {
                    items = new SelectListItem();
                    items.Text = "Quarterly";
                    items.Value = "QUARTERLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Monthly";
                    items.Value = "MONTHLY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "QUARTERLY")
                {
                    items = new SelectListItem();
                    items.Text = "Monthly";
                    items.Value = "MONTHLY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "MONTHLY")
                {


                    items = new SelectListItem();
                    items.Text = "Weekly";
                    items.Value = "WEEKLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Daily";
                    items.Value = "DAILY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "WEEKLY")
                {


                    items = new SelectListItem();
                    items.Text = "Daily";
                    items.Value = "DAILY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "DAILY")
                {
                    items = new SelectListItem();
                    items.Text = "Daily";
                    items.Value = "DAILY";
                    lstItems.Add(items);
                }
            }
            catch (Exception ex) { }
            return lstItems;
        }
        private List<SelectListItem> GetGranularityListTime(string frequency)
        {
            List<SelectListItem> lstItems = new List<SelectListItem>();
            SelectListItem items;
            try
            {
                if (frequency.ToUpper().Trim() == "YEARLY")
                {
                    items = new SelectListItem();
                    items.Text = "Yearly";
                    items.Value = "YEARLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Half Yearly";
                    items.Value = "HALF_YEARLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Quarterly";
                    items.Value = "QUARTERLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Monthly";
                    items.Value = "MONTHLY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "HALF_YEARLY")
                {
                    items = new SelectListItem();
                    items.Text = "Half Yearly";
                    items.Value = "HALF_YEARLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Quarterly";
                    items.Value = "QUARTERLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Monthly";
                    items.Value = "MONTHLY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "QUARTERLY")
                {
                    items = new SelectListItem();
                    items.Text = "Quarterly";
                    items.Value = "QUARTERLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Monthly";
                    items.Value = "MONTHLY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "MONTHLY")
                {
                    items = new SelectListItem();
                    items.Text = "Monthly";
                    items.Value = "MONTHLY";
                    lstItems.Add(items);

                    items = new SelectListItem();
                    items.Text = "Weekly";
                    items.Value = "WEEKLY";
                    lstItems.Add(items);
                    items = new SelectListItem();
                    items.Text = "Daily";
                    items.Value = "DAILY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "WEEKLY")
                {

                    items = new SelectListItem();
                    items.Text = "Weekly";
                    items.Value = "WEEKLY";
                    lstItems.Add(items);

                    items = new SelectListItem();
                    items.Text = "Daily";
                    items.Value = "DAILY";
                    lstItems.Add(items);
                }
                else if (frequency.ToUpper().Trim() == "DAILY")
                {
                    items = new SelectListItem();
                    items.Text = "Daily";
                    items.Value = "DAILY";
                    lstItems.Add(items);
                }
            }
            catch (Exception ex) { }
            return lstItems;
        }
    }
}