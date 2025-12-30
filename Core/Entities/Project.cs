using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Projects")]
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [MaxLength(50)]
        public string? Technology { get; set; }
        public decimal Budget { get; set; }

        [MaxLength(100)]
        public string? ClientName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}