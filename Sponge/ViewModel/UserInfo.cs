using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sponge.ViewModel
{
    public class UserInfo
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string UserEmail { get; set; }
        [Required]
        public string UserName { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class GetUserinfo
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        public List<SPGRole> RoleList { get; set; }
        public string Role { get; set; }
        public string SubFunction { get; set; }
        public List<SPGSubfuncion>SubfunctionList{ get; set; }
        public string Status { get; set; }

    }
}
