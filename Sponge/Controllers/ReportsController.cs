using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sponge.Common;
using Sponge.ViewModel;
using System.Data;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> UploadedTemplate()
        {
            string[] userId = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context _context = new();
            var report = await _context.Procedures.SP_UPLOADEDTEMPLATEAsync(userId[1]);

            List<UploadedTemplateReport> results = new();

            foreach (var item in report)
            {
                UploadedTemplateReport result = new();
                result.FILE_PATH = item.FILE_PATH;
                result.PERIOD_TO = item.PERIOD_TO;
                result.FILE_NAME = item.FILE_NAME;
                result.SUBFUNCTION_NAME = item.SUBFUNCTION_NAME;
                result.DatA_COLLECTION = item.DatA_COLLECTION;
                result.CONFIG_ID = item.CONFIG_ID;
                result.DOCUMENT_ID = item.DOCUMENT_ID;
                result.UPLOADDATE = item.UPLOADDATE;
                results.Add(result);
            }

            return View("Views\\Reports\\UploadedTemplate.cshtml", results);
        }

        public async Task<IActionResult> PendingTemplate()
        {
            string[] userId = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context _context = new();
            var report = await _context.Procedures.SP_GETUPLOADPENDINGAsync(userId[1]);

            List<PendingTemplateReport> results = new();

            foreach (var item in report)
            {
                PendingTemplateReport result = new();
                result.LOCK_DATE = item.LOCK_DATE;
                result.FILE_NAME = item.FILE_NAME;
                result.SUBFUNCTION_NAME = item.SUBFUNCTION_NAME;          
                results.Add(result);
            }

            return View("Views\\Reports\\PendingTemplate.cshtml", results);
        }

        public async Task<IActionResult> PendingApproval()
        {
            string[] userId = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
            SPONGE_Context _context = new();
            var report = await _context.Procedures.SP_PENDINGFORAPPROVALAsync(userId[1]);

            List<PendingApprovalReport> results = new();

            foreach (var item in report)
            {
                PendingApprovalReport result = new();
                result.FILE_PATH = item.FILE_PATH;
                result.PERIOD_TO = item.PERIOD_TO;
                result.FILE_NAME = item.FILE_NAME;
                result.SUBFUNCTION_NAME = item.SUBFUNCTION_NAME;
                result.DatA_COLLECTION = item.DatA_COLLECTION;
                result.CONFIG_ID = item.CONFIG_ID;
                result.DOCUMENT_ID = item.DOCUMENT_ID;
                result.APPROVER_NAME = item.APPROVER_NAME;
                result.LOCK_DATE = item.LOCK_DATE;
                results.Add(result);
            }
            return View("Views\\Reports\\PendingApproval.cshtml", results);
        }
    }
}
