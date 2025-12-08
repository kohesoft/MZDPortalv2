using System.Collections.Generic;

namespace MZDNETWORK.Models
{
    public class AccessChangeUserListItem
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string InternalEmail { get; set; }
        public string ExternalEmail { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string Intercom { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
