﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sponge.ViewModel
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class GetUserinfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<SPGRole> RoleList { get; set; }
        public string Role { get; set; }
        public string SubFunction { get; set; }
        public List<SPGSubfuncion>SubfunctionList{ get; set; }
        public string Status { get; set; }

    }
}