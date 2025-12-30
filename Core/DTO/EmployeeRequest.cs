using System;
using System.ComponentModel.DataAnnotations;

namespace Core.DTO
{
    public class EmployeeRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime HireDate { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }

        public bool IsActive { get; set; } = true;
    }
}