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
        SPONGE_Context spONGE_Context = new();
        var lst = spONGE_Context.SPG_SUBJECTAREA.Select(o => new { o.SUBJECTAREA_NAME , o.SUBJECTAREA_ID}).Distinct();
        ViewBag.SubjectArea = new SelectList(lst.ToList(), "SUBJECTAREA_NAME", "SUBJECTAREA_ID");
            var lstDimensions = spONGE_Context.SPG_MPP_MASTER.Select(o => new { o.MPP_DIMENSION_NAME, o.DIMENSION_TABLE }).Distinct();
            ViewBag.lstDimensions = new SelectList(lstDimensions.ToList(), "DIMENSION_TABLE", "MPP_DIMENSION_NAME");
            return View();
    }
    [HttpGet]
    public IActionResult SelectedDimension(int? subjectAreaId)
    {
        SPONGE_Context spONGE_Ctx = new();
            var dimensionlist = (from user in spONGE_Ctx.SPG_SUBJECT_DIMENSION                              
                               where user.SUBJECTAREA_ID == subjectAreaId
                                 select new
                               {
                                   DIMENSION_NAME = user.MPP_DIMENSION_NAME,
                                   DIMENSIONTABLENAME = user.DIMENSION_TABLE
                               }).Distinct().ToList();
            if (dimensionlist.Count <= 0)
            {
                var lstDimensions = spONGE_Ctx.SPG_MPP_MASTER.Select(o => new { o.MPP_DIMENSION_NAME, o.DIMENSION_TABLE }).Distinct();
                ViewBag.lstDimensions = new SelectList(lstDimensions.ToList(), "DIMENSION_TABLE", "MPP_DIMENSION_NAME");
                //return View();
            }

            return Json(dimensionlist);
    }
    public IActionResult SaveMastersGroup(List<Dimension> dimensions, int? selectedSubjectArea)
    {
        string[] userName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);

        SPONGE_Context sPONGE_Context = new();

            foreach (var Dimension in dimensions)
            {
                if (!Dimension.IsSelected)
                {
                    SPG_SUBJECT_DIMENSION sPG_1 = new();
                    {
                        sPG_1.MPP_DIMENSION_NAME = Dimension.Value;
                        sPG_1.DIMENSION_TABLE = Dimension.Key;
                        sPG_1.SUBJECTAREA_ID = selectedSubjectArea;
                        sPG_1.ACTIVE_FLAG = "Y";
                        sPG_1.CREATED_DATE = DateTime.Now;
                        sPG_1.CREATED_BY = userName[1].ToString();
                    }
                    sPONGE_Context.SPG_SUBJECT_DIMENSION.Add(sPG_1);
                    sPONGE_Context.SaveChanges();
                    var spg_Master = sPONGE_Context.SPG_MPP_MASTER
                        .Where(o => o.MPP_DIMENSION_NAME == Dimension.Value && o.IS_KEY != "Y")
                        .Select(o => new { o.MASTER_NAME, o.MASTER_DISPLAY_NAME })
                        .Distinct(); 
                    ViewBag.SPG_MASTER = new SelectList(spg_Master.ToList(), "MASTER_NAME", "MASTER_DISPLAY_NAME");
                }            
                else
                {
                    var spg_Master = sPONGE_Context.SPG_MPP_MASTER
                                            .Where(o => o.MPP_DIMENSION_NAME == Dimension.Value && o.IS_KEY != "Y")
                                            .Select(o => new { o.MASTER_NAME, o.MASTER_DISPLAY_NAME })
                                            .Distinct(); 
                    ViewBag.SPG_MASTER = new SelectList(spg_Master.ToList(), "MASTER_NAME", "MASTER_DISPLAY_NAME");
                }
                
            }
            return View("Views\\ConfigureSubjectArea\\ConfigureMasters.cshtml");

        }        
    public IActionResult GetFieldName(string? masterName)
    {
        SPONGE_Context sPONGE_Context = new();
        var fieldName = sPONGE_Context.SPG_MPP_MASTER.Where(o => o.COLUMN_NAME == masterName)
            .Select(o => new { o.COLUMN_DISPLAY_NAME }).Distinct();
        ViewBag.FieldName = new SelectList(fieldName.ToList(), "FILED NAME", "FILED_DISPLAY_NAME");
        return Json(fieldName);
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