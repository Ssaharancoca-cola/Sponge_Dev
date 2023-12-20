using DAL.Models;

namespace Sponge.ViewModel
{
    public class ApprovalModel : SPG_DOCUMENT
    {
        public SPG_DOCUMENT Document { get; set; }
        public SPG_TEMPLATE Template { get; set; }
        public SPG_USERS Users { get; set; }
        public string ApproverEmailID { get; set; }
        public SPG_APPROVALSTATUS ApprovalStatus { get; set; }

        public SPG_CONFIGURATION Configuration { get; set; }
        public SPG_SUBJECTAREA SubjectArea { get; set; }
    }
}
