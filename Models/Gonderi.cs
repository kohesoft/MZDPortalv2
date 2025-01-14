using System;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{ 
public class Gonderi
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; internal set; }
}
}