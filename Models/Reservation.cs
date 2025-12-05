using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Room { get; set; }
        public DateTime Date { get; set; }
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; } // Örn: 09:00
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }   // Örn: 12:00
        public string Title { get; set; }
        public string Description { get; set; }
        public string Attendees { get; set; } // Geçici olarak string, sonra kaldırılacak
        public ReservationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectionReason { get; set; }

        // Navigation Property - Katılımcılar
        public virtual ICollection<ReservationAttendee> ReservationAttendees { get; set; }

        // Navigation Property - Toplantı Kararları
        public virtual ICollection<MeetingDecision> MeetingDecisions { get; set; }

        public Reservation()
        {
            ReservationAttendees = new List<ReservationAttendee>();
            MeetingDecisions = new List<MeetingDecision>();
        }
    }
}