using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class AssignTicketRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int EmployeeId { get; set; }
    }
}
