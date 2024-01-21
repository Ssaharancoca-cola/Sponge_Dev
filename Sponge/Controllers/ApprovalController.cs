using DAL.Common;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sponge.Common;
using Sponge.ViewModel;
using System.Collections.Specialized;
using System.Data;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ApprovalController : Controller
    {
        private readonly IOptions<AppSettings> _settings;
        private readonly Email _email;

        public ApprovalController(IOptions<AppSettings> settings, Email email)
        {
            _settings = settings;
            this._email = email;
        }
        public IActionResult MyApproval()
        {
            string[] userid = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                SPONGE_Context objFunction = new();
                var role = (from a in objFunction.SPG_USERS_FUNCTION.ToList()
                            from b in objFunction.SPG_ROLE
                            where a.ROLE_ID == b.ROLE_ID && a.USER_ID == userid[1] && a.ACTIVE_FLAG == "Y"
                            select new { RoleName = b.ROLE_NAME }).FirstOrDefault();
                
                var viewModel =
                    (from doc in objFunction.SPG_DOCUMENT
                     join temp in objFunction.SPG_TEMPLATE on doc.TEMPLATEID equals temp.TEMPLATE_ID
                     join status in objFunction.SPG_APPROVALSTATUS on doc.APPROVALSTATUSID equals status.ID
                     join conf in objFunction.SPG_CONFIGURATION on temp.CONFIG_ID equals conf.CONFIG_ID
                     join area in objFunction.SPG_SUBJECTAREA on conf.SUBJECTAREA_ID equals area.SUBJECTAREA_ID
                     join usr in objFunction.SPG_USERS on doc.UPLOADEDBY equals usr.USER_ID

                     where status.DESCRIPTION == "Pending"
                     select new ApprovalModel {Users=usr, Document = doc, Template = temp, ApprovalStatus = status, Configuration = conf, SubjectArea = area }).ToList();

            if (!(role.RoleName == "Admin") && !(role.RoleName == "Data Configure"))
           
               {
                viewModel = (from vd in viewModel
                             where vd.Document.APPROVERID == userid[1]
                             select vd).ToList();
            }           
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult save(IFormCollection form, ApprovalModel model, string command, string[] SelectedChkBox, string comment)
        {
            string[] UserName = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None );
                SPONGE_Context objModel = new();
                if (SelectedChkBox != null && SelectedChkBox.Length > 0)
                {
                    foreach (string str in SelectedChkBox)
                    {
                        var document = objModel.SPG_DOCUMENT.Where(m => m.ID == str).FirstOrDefault();
                        var Usr = objModel.SPG_USERS.Where(s => s.USER_ID.ToUpper() == document.UPLOADEDBY.ToUpper()).FirstOrDefault();
                        var template = objModel.SPG_TEMPLATE.Where(m => m.TEMPLATE_ID == document.TEMPLATEID).FirstOrDefault();
                        if (document != null)
                        {
                            using (var dbcontext = new SPONGE_Context())
                            {
                                int ApprovedStatusId = (int)Helper.ApprovalStatusEnum.Approved;
                                var documentModel = dbcontext.SPG_DOCUMENT.Where(s => s.TEMPLATEID == template.TEMPLATE_ID && (s.APPROVALSTATUSID == ApprovedStatusId)).ToList();

                                if (documentModel.Count > 0)
                                {
                                    foreach (var item in documentModel)
                                    {
                                        item.LATEST_FLAG_FOR_DAY = 0;
                                        dbcontext.Entry(item);
                                    }
                                }
                                dbcontext.SaveChanges();
                            }
                        #region email
                        if (command.ToLower() == "approve")
                        {
                            var approvalStatus = objModel.SPG_APPROVALSTATUS.Where(a => a.DESCRIPTION.ToLower() == "approved").FirstOrDefault();
                            document.APPROVALSTATUSID = approvalStatus.ID;
                            document.LATEST_FLAG_FOR_DAY = 1;
                            string UserRole = HttpContext.Session.GetString("ROLE");
                            try
                            {
                                NameValueCollection mailBodyplaceHolders = new NameValueCollection
                                {
                                    { "<UserName>", Usr.Name },
                                    { "<FileName>", document.FILE_NAME.Replace("txt", "xlsx") },
                                    { "<ForTime>", template.PERIOD_FROM.ToString() },
                                    { "<OnTime>", template.PERIOD_TO.ToString() },
                                    { "<LockDate>", template.LOCK_DATE.ToString() },
                                    { "<ApproverName>", HttpContext.Session.GetString("NAME") }
                                };

                                string DataCollectionSubject = "[Sponge] - Template Status";
                                string mailbody = "";
                                string messageTemplatePath = _settings.Value.TemplateApprovedExcelTemplate;
                                {
                                    mailbody = _email.GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                                }
                                    _email.SendMail("", DataCollectionSubject, mailbody, Usr.EMAIL_ID);
                                
                            }
                            catch (Exception ex)
                            {
                                ErrorLog logerror = new ();
                                logerror.LogErrorInTextFile(ex);
                            }
                        }
                        if (command.ToLower() == "reject")
                        {
                            var approvalStatus = objModel.SPG_APPROVALSTATUS.Where(a => a.DESCRIPTION.ToLower() == "rejected").FirstOrDefault();
                            document.APPROVALSTATUSID = approvalStatus.ID;
                            try
                            {
                                NameValueCollection mailBodyplaceHolders = new NameValueCollection
                                {
                                    { "<UserName>", Usr.Name },
                                    { "<FileName>", document.FILE_NAME.Replace("txt", "xlsx") },
                                    { "<ForTime>", Convert.ToDateTime(template.PERIOD_FROM).ToString("dd/MMM/y") },
                                    { "<OnTime>", Convert.ToDateTime(template.PERIOD_TO).ToString("dd/MMM/y") },
                                    { "<LockDate>", Convert.ToDateTime(template.LOCK_DATE).ToString("dd/MMM/y") },
                                    { "<ApproverName>", HttpContext.Session.GetString("NAME") }
                                };

                                string DataCollectionSubject = "[Sponge] - Template Status";
                                string mailbody = "";
                                string messageTemplatePath = _settings.Value.TemplateRejectedExcelTemplate;
                                mailbody = _email.GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                                _email.SendMail("", DataCollectionSubject, mailbody, Usr.EMAIL_ID);
                            }
                            catch (Exception ex)
                            {
                                ErrorLog logerror = new();
                                logerror.LogErrorInTextFile(ex);
                            }
                        }
                        #endregion
                        if (command == "Approve")
                        {
                            document.APPROVALSTATUSID = (int)Helper.ApprovalStatusEnum.Approved;
                        }
                        else
                        {
                            document.APPROVALSTATUSID = (int)Helper.ApprovalStatusEnum.Rejected;
                        }
                            document.APPROVEDON = DateTime.Now;
                            document.APPROVERID = UserName[1];
                            document.COMMENTS = comment;
                            objModel.SaveChanges();
                        }
                    }
                }
            return RedirectToAction("MyApproval");
        }

        public IActionResult Download(string documentId, int configId)
        {
            SPONGE_Context objFunction = new();
            ApprovalModel model = new ApprovalModel();

            string fileName = documentId + ".xlsx";
            var fileURI =
                (from doc in objFunction.SPG_DOCUMENT
                 join temp in objFunction.SPG_TEMPLATE on doc.TEMPLATEID equals temp.TEMPLATE_ID
                 join conf in objFunction.SPG_CONFIGURATION on temp.CONFIG_ID equals conf.CONFIG_ID
                 where conf.CONFIG_ID == configId && doc.ID == documentId
                 select new { doc.FILE_PATH, doc.ID, temp.FILE_NAME }).First();
            string filepath = Path.Combine(fileURI.FILE_PATH, fileName);

            var memory = new MemoryStream();
            using (var stream = new FileStream(filepath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileURI.FILE_NAME);
        }       
    }
}
