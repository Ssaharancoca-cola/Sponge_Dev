using DAL.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sponge.ViewModel
{
    public class SPGSubfuncion
    {
        public int SubFunctionId { get; set; }
        public string SubfunctionName { get; set; }
        public bool Selected { get; set; }
    }
}
