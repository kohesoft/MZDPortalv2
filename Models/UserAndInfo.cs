using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class UserAndInfo
    {
        public UserInfo UserInfo { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}