using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sponge.Common;
using Sponge.Models;
using Sponge.ViewModel;
using System.Data;
using System.Diagnostics;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ConfigureSubjectAreaController : Controller
{
    private readonly ILogger<HomeController> _logger;

        public ConfigureSubjectAreaController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult ConfigureSubjectArea()
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_SUBJECTAREA.Select(o => new { o.SUBJECTAREA_NAME , o.SUBJECTAREA_ID}).Distinct();
            ViewBag.SubjectArea = new SelectList(lst.ToList(), "SUBJECTAREA_NAME", "SUBJECTAREA_ID");
            var lstDimensions= spONGE_Context.SPG_MPP_MASTER.Select(o => new { o.MPP_DIMENSION_NAME, o.DIMENSION_TABLE }).Distinct();
            ViewBag.lstDimensions = new SelectList(lstDimensions.ToList(), "DIMENSION_TABLE", "MPP_DIMENSION_NAME");

            return View();
        }
        [HttpPost]
        public IActionResult SaveMastersGroup(IFormCollection data)
        {
            string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

            SPONGE_Context sPONGE_Context = new SPONGE_Context();
            string Dimensions = data["Dimension"];
            string[] DimensionsArr = Dimensions.Split(',');
            foreach (string Dimension in DimensionsArr)
            {
                SPG_SUBJECT_DIMENSION sPG_1 = new SPG_SUBJECT_DIMENSION();
                {
                    sPG_1.MPP_DIMENSION_NAME = Dimension;
                    sPG_1.SUBJECTAREA_ID = Convert.ToInt32(data["SubjectArea"]);
                    sPG_1.ACTIVE_FLAG = "Y";
                    sPG_1.CREATED_DATE = DateTime.Now;
                    sPG_1.CREATED_BY = userName[1].ToString();
                }
                sPONGE_Context.SPG_SUBJECT_DIMENSION.Add(sPG_1);
            }

            sPONGE_Context.SaveChanges();
            
            var SPG_MASTER = sPONGE_Context.SPG_MPP_MASTER.Select(o => new { o.MASTER_NAME, o.MASTER_DISPLAY_NAME }).Distinct();
            ViewBag.SPG_MASTER = new SelectList(SPG_MASTER.ToList(), "MASTER_NAME", "MASTER_DISPLAY_NAME");

            return View("Views\\ConfigureSubjectArea\\ConfigureMasters.cshtml");
        }        
        public IActionResult SaveMasters()
        {
           
            return View("Views\\ConfigureSubjectArea\\ConfigureDataCollection.cshtml");
        }
        public IActionResult SaveDataCollection()
        {
            return View("Views\\ConfigureSubjectArea\\AssignUsers.cshtml");
        }
        public IActionResult SaveUsers()
        {
            return View("Views\\Home\\Index.cshtml");
        }
        [HttpPost]
        public IActionResult ConfigureMasterGroup(IFormCollection function, string Command)
        {
          
            return RedirectToAction("Function");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }




}
}