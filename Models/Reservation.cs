using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MZDNETWORK.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Room { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; } // Örn: "09:00"
        public string EndTime { get; set; }   // Örn: "12:00"
        public string Title { get; set; }
        public string Description { get; set; }
        public string Attendees { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectionReason { get; set; }
    }
}