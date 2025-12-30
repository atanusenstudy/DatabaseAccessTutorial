using System;
using System.ComponentModel.DataAnnotations;

namespace Core.DTO
{
    public class TicketUpdate
    {
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(20)]
        public string Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public int? AssignedTo { get; set; }

        [StringLength(500)]
        public string Resolution { get; set; }

        public DateTime? ResolvedDate { get; set; }
    }
}