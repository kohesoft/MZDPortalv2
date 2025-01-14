using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using MZDNETWORK.Models;

namespace MZDNETWORK.Models
{
public class User
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Department { get; set; }
    public string Position { get; set; }
    public string Intercom { get; set; }
    public string PhoneNumber { get; set; }
    public string InternalEmail { get; set; }
    public string ExternalEmail { get; set; }
    public string Sicil { get; set; }
    public ICollection<UserInfo> UserInfo { get; set; }
    public bool IsPasswordChanged { get; set; }

    }
}