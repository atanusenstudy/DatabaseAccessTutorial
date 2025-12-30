using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Medium";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public int? AssignedTo { get; set; }

        [Required]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        [MaxLength(500)]
        public string? Resolution { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Employee? Employee { get; set; }
        public virtual Project? Project { get; set; }
    }
}