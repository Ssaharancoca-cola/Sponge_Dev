using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sponge
{
    public class AppSettings
    {
        public string? SMTPHost { get; set; }
        public string? MailFrom { get; set; }
        public string? HiddenSheetName { get; set; }
        public string? ApplicationName { get; set; }

        public string? TemplateGenerationEmail { get; set; }
       
        public string? ErrorMailTemplate { get; set; }
        public string? UploadReminderMailExcelTemplate { get; set; }
        public string? EscalationMailExcelTemplate { get; set; }
        public string? DataApproverRoleId { get; set; }
        public string? AdminRoleId { get; set; }
        public string? RemoveRowsfromExcel { get; set; }
        public string? BatchJobErrorlocation { get; set; }
        public string? UploadedDocumentsFilePath { get; set; }
        public string? WarningdocumentFilePath { get; set; }
     
        public string? EmailTemplatePathForUploader { get; set; }
        public string? ApprovalmailToUploader { get; set; }
        public string? MailToApprover { get; set; }
        public string? TemplateApprovedExcelTemplate { get; set; }
        public string? TemplateRejectedExcelTemplate { get; set; }
        public string? MailIds { get; set; }
        public string? ExceedRecordMsg { get; set; }
        // Continue for all your properties...
    }


  
}
