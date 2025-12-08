using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MZDNETWORK.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Intercom { get; set; }
        public string PhoneNumber { get; set; }
        public string InternalEmail { get; set; }
        public string ExternalEmail { get; set; }
        public string Sicil { get; set; }
        // IList enables indexed access in views while still working with EF navigation properties
        public IList<UserInfo> UserInfo { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public bool IsPasswordChanged { get; set; }
        
        // Dinamik multiple roles için
        public virtual ICollection<UserRole> UserRoles { get; set; }
        
        // Helper property to get role names
        public List<string> RoleNames
        {
            get
            {
                if (UserRoles != null && UserRoles.Any())
                {
                    return UserRoles.Select(ur => ur.Role.Name).ToList();
                }
                return new List<string>();
            }
        }

        // Helper property for full name
        public string FullName => $"{Name} {Surname}".Trim();

        // Helper property for display name
        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Username;
    }
}
