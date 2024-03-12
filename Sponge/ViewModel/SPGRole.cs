using DAL.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sponge.ViewModel
{
    public class SPGRole
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
        public int? RolePriority { get; set; }
    }
}
