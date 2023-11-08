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
    public class ConfigureTemplateController : Controller
{
    private readonly ILogger<HomeController> _logger;

        public ConfigureTemplateController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult ConfigureTemplate()
        {
            SPONGE_Context spONGE_Context = new SPONGE_Context();
            var lst = spONGE_Context.SPG_SUBJECTAREA.Select(o => new { o.SUBJECTAREA_NAME, o.SUBJECTAREA_ID }).Distinct();
            ViewBag.SubjectArea = new SelectList(lst.ToList(), "SUBJECTAREA_NAME", "SUBJECTAREA_ID");

            using var cmd = spONGE_Context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "[dbo].[SP_GET_USERS_DETAILS]";

            //common
            cmd.CommandType = CommandType.StoredProcedure;
            if (cmd.Connection.State != System.Data.ConnectionState.Open) cmd.Connection.Open();

            List<GetUserinfo> GUi = new List<GetUserinfo>();

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    GetUserinfo ui = new GetUserinfo();

                    ui.UserName = (string)reader["NAME"];
                   
                    GUi.Add(ui);
                }
            }
            return View(GUi);
        }
    
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }




}
}