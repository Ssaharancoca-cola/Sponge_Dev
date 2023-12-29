namespace Sponge.ViewModel
{
    public class ApproverModel
    {
        public decimal AppRoleId { get; set; }
        public string AppUserId { get; set; }
        public string AppMailid { get; set; }
        public string AppName { get; set; }
    }
    public class FileModel
    {
        public int TemplateID { get; set; }
        public string FileName { get; set; }
        public DateTime? PERIOD_FROM { get; set; }
        public DateTime? PERIOD_TO { get; set; }
        public DateTime? LockDate { get; set; }
        public string FileCode { get; set; }
        public string SUBJECTAREA_NAME { get; set; }
        public string DocumentID { get; set; }
        public string DocumentFileNAME { get; set; }
        public string ErrorMessage { get; set; }
        public int? DOCTEMPLATEID { get; set; }
        public List<FileModel> ListTemplateDetails;
        public int RoleID { get; set; }
        public int ConFigId { get; set; }
        public string UploderUserId { get; set; }
        public string UploderEmailId { get; set; }
        public string UserName { get; set; }
        public string OnTime { get; set; }
        public string ForTime { get; set; }
        public int? SubFunctionID { get; set; }

        public string ApproverID { get; set; }
        public String ApproverName { get; set; }
        public string ApproverEmailID { get; set; }
    }
    public class TemplateFile
    {
        public string FileName { get; set; }
        public int TemplateID { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }
        public List<TemplateFile> TeplateFileList;
    }
}
