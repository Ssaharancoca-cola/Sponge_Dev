using System.Data;

namespace Sponge.ViewModel
{
    public class ManualSendResendExcelModel
    {
        public string UserName { get; set; }
        public string SubjectAreaName { get; set; }
        public int ConfigId { get; set; }
        public int SubjectAreaId { get; set; }
        public string Frequency { get; set; }
        public string GranularTime { get; set; }
        public string Version { get; set; }
        public string OnTime { get; set; }
        public string IsPopulated { get; set; }
        public string DataCollection { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public DateTime? UploadReminderDate { get; set; }
        public DateTime? EscalationAlertDate { get; set; }
        public DateTime? LockDate { get; set; }


    }
    public class SendResendExcel
    {
        public int ID { get; set;}
        public DateTime? LockDate { get; set;}
        public DateTime? UploadReminderDate { get; set;}
        public DateTime? EsclationDate { get; set; }
        public string IsPopluated { get; set; }
        public int Error { get; set; }

    }
}
