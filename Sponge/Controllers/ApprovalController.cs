using DAL;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Sponge.Common;
using Sponge.ViewModel;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Net.Mail;

namespace Sponge.Controllers
{
    [AccessFilters]
    [SessionTimeOut]
    public class ApprovalController : Controller
    {
        private readonly IHttpContextAccessor _httpSession;

        //public ApprovalController(IHttpContextAccessor httpContextAccessor)
        //{
        //    _httpSession = httpContextAccessor;
        //}
        public IActionResult MyApproval()
        {
            string[] userid = User.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
                SPONGE_Context objFunction = new();
                var role = (from a in objFunction.SPG_USERS_FUNCTION.ToList()
                            from b in objFunction.SPG_ROLE
                            where a.ROLE_ID == b.ROLE_ID && a.USER_ID == userid[1] && a.ACTIVE_FLAG == "Y"
                            select new { RoleName = b.ROLE_NAME }).FirstOrDefault();
                if (role == null)
                {

                    if (role == null)
                    {
                        role = (from a in objFunction.SPG_CONFIGURATION.AsEnumerable()
                                where a.APPROVER_ID == userid[1]
                                select new { RoleName = a.APPROVER_ID }).FirstOrDefault();
                    }
                }
                var viewModel =
                    (from doc in objFunction.SPG_DOCUMENT
                     join temp in objFunction.SPG_TEMPLATE on doc.TEMPLATEID equals temp.TEMPLATE_ID
                     join status in objFunction.SPG_APPROVALSTATUS on doc.APPROVALSTATUSID equals status.ID
                     join conf in objFunction.SPG_CONFIGURATION on temp.CONFIG_ID equals conf.CONFIG_ID
                     join area in objFunction.SPG_SUBJECTAREA on conf.SUBJECTAREA_ID equals area.SUBJECTAREA_ID

                     where status.DESCRIPTION == "Pending"
                     select new ApprovalModel { Document = doc, Template = temp, ApprovalStatus = status, Configuration = conf, SubjectArea = area }).ToList();

            if (role.RoleName != "Admin")
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
                            //if (command.ToLower() == "approve")
                            //{
                            //    var approvalStatus = objModel.EP_APPROVALSTATUS.Where(a => a.DESCRIPTION.ToLower() == "approved").FirstOrDefault();
                            //    document.APPROVALSTATUSID = approvalStatus.ID;
                            //    document.LATEST_FLAG_FOR_DAY = 1;
                            //    string UserRole = Session["Description"].ToString();
                            //    try
                            //    {
                            //        NameValueCollection mailBodyplaceHolders = new NameValueCollection();
                            //        mailBodyplaceHolders.Add("<UserName>", Usr.NAME);
                            //        mailBodyplaceHolders.Add("<FileName>", document.FILE_NAME.Replace("txt", "xlsx"));
                            //        mailBodyplaceHolders.Add("<ForTime>", Convert.ToDateTime(template.PERIOD_FROM).ToString("dd/MMM/y"));
                            //        mailBodyplaceHolders.Add("<OnTime>", Convert.ToDateTime(template.PERIOD_TO).ToString("dd/MMM/y"));
                            //        mailBodyplaceHolders.Add("<LockDate>", Convert.ToDateTime(template.LOCK_DATE).ToString("dd/MMM/y"));
                            //        mailBodyplaceHolders.Add("<ApproverName>", Convert.ToString(Session["Name"]));

                            //        string DataCollectionSubject = "[iQlik Portal] - Template Status";
                            //        string mailbody = "";
                            //        string messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["TemplateApprovedExcelTemplate"].ToString());
                            //        mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                            //        if (UserRole.ToUpper() != "ADMIN" && UserRole.ToUpper() != "DATA APPROVER")
                            //        {
                            //            SendMail("", DataCollectionSubject, mailbody, Usr.EMAILID);
                            //        }
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        LogError logerror = new LogError();
                            //        logerror.LogErrorInTextFile(ex);
                            //    }
                            //}
                            //if (command.ToLower() == "reject")
                            //{
                            //    var approvalStatus = objModel.EP_APPROVALSTATUS.Where(a => a.DESCRIPTION.ToLower() == "rejected").FirstOrDefault();
                            //    document.APPROVALSTATUSID = approvalStatus.ID;
                            //    try
                            //    {
                            //        NameValueCollection mailBodyplaceHolders = new NameValueCollection();
                            //        mailBodyplaceHolders.Add("<UserName>", Usr.NAME);
                            //        mailBodyplaceHolders.Add("<FileName>", document.FILE_NAME.Replace("txt", "xlsx"));
                            //        mailBodyplaceHolders.Add("<ForTime>", Convert.ToDateTime(template.PERIOD_FROM).ToString("dd/MMM/y"));
                            //        mailBodyplaceHolders.Add("<OnTime>", Convert.ToDateTime(template.PERIOD_TO).ToString("dd/MMM/y"));
                            //        mailBodyplaceHolders.Add("<LockDate>", Convert.ToDateTime(template.LOCK_DATE).ToString("dd/MMM/y"));
                            //        mailBodyplaceHolders.Add("<ApproverName>", Convert.ToString(Session["Name"]));

                            //        string DataCollectionSubject = "[iQlik Portal] - Template Status";
                            //        string mailbody = "";
                            //        string messageTemplatePath = System.IO.File.ReadAllText(System.Configuration.ConfigurationManager.AppSettings["TemplateRejectedExcelTemplate"].ToString());
                            //        mailbody = GetMessageBody(messageTemplatePath, mailBodyplaceHolders);
                            //        SendMail("", DataCollectionSubject, mailbody, Usr.EMAILID);
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        LogError logerror = new LogError();
                            //        logerror.LogErrorInTextFile(ex);
                            //    }
                            //}
                            #endregion
                            if(command == "Approve")
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

        //public string GetMessageBody(string messageTemplate, NameValueCollection nvc)
        //{
        //    messageTemplate = ReplacePlaceHolders(messageTemplate, nvc);
        //    return messageTemplate;

        //}
        //private string ReplacePlaceHolders(string text, NameValueCollection valueCollection)
        //{
        //    if (valueCollection == null || valueCollection.Count <= 0)
        //    {
        //        throw new ArgumentException("Invalid NameValueCollection");
        //    }
        //    //string text=null;
        //    string result = text;
        //    string value;
        //    foreach (string key in valueCollection.AllKeys)
        //    {
        //        value = valueCollection[key];
        //        result = result.Replace(key, value);
        //    }
        //    return result;
        //}
        //public void SendMail(string filename, string subject, string mailbody, string MailID)
        //{
        //    SmtpClient smtpClient = new SmtpClient();
        //    smtpClient.Host = ConfigurationManager.AppSettings["SMTPHost"].ToString();

        //    MailMessage mailMessage = new MailMessage();
        //    mailMessage.Body = mailbody;
        //    mailMessage.Subject = subject;
        //    mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"].ToString());
        //    mailMessage.To.Add(new MailAddress(MailID));
        //    mailMessage.IsBodyHtml = true;
        //    if (!string.IsNullOrEmpty(filename))
        //    {
        //        mailMessage.Attachments.Add(new Attachment(filename));
        //    }

        //    smtpClient.Send(mailMessage);

        //}
        //}
    }
}
