using DAL.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sponge.ViewModel
{
    public class SubjectArea
    {
        public SPG_SUBFUNCTION SpgSubfunction { get; set; }
        public SPG_SUBJECTAREA SpgSubjectArea { get; set; }
        public List<SelectListItem> selectListItems { get; set; }
    }
}
