namespace Sponge.ViewModel
{
    public class SearchDataList
    {
        public int ConfigId { get; set; }
        public int SubjectAreaId { get; set; }
        public string SubjectAreaName { get; set; }
        public DateTime? EffectiveDate { get; set;}
        public string AssignedUser { get; set;}
        public string Active { get; set;}
        public string ManualSendResendUrl { get; set;}
    }
}
