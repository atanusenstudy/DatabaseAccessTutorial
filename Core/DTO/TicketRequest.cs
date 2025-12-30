using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class TicketRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open";

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        public DateTime? DueDate { get; set; }

        public int? AssignedTo { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        [StringLength(500)]
        public string? Resolution { get; set; }
    }
}
