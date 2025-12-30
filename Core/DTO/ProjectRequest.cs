using System;
using System.ComponentModel.DataAnnotations;

namespace Core.DTO
{

    public class ProjectRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [StringLength(50)]
        public string? Technology { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; }

        [StringLength(100)]
        public string? ClientName { get; set; }
    }
}