using Core.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Core.DTO;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketsController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        // GET: api/tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            try
            {
                var tickets = await _ticketRepository.GetAllAsync();
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/tickets/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound($"Ticket with ID {id} not found");
                }
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/tickets/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByStatus(string status)
        {
            try
            {
                var tickets = await _ticketRepository.GetTicketsByStatusAsync(status);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/tickets/employee/{employeeId}
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByEmployee(int employeeId)
        {
            try
            {
                var tickets = await _ticketRepository.GetTicketsByEmployeeAsync(employeeId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/tickets/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByProject(int projectId)
        {
            try
            {
                var tickets = await _ticketRepository.GetTicketsByProjectAsync(projectId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> CreateTicket(TicketRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ticket = new Ticket
                {
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status,
                    Priority = request.Priority,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = request.DueDate,
                    AssignedTo = request.AssignedTo,
                    ProjectId = request.ProjectId,
                    Resolution = request.Resolution,
                    CreatedAt = DateTime.UtcNow
                };

                var id = await _ticketRepository.AddAsync(ticket);
                ticket.Id = id;

                return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/tickets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, TicketRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingTicket = await _ticketRepository.GetByIdAsync(id);
                if (existingTicket == null)
                {
                    return NotFound($"Ticket with ID {id} not found");
                }

                existingTicket.Title = request.Title;
                existingTicket.Description = request.Description;
                existingTicket.Status = request.Status;
                existingTicket.Priority = request.Priority;
                existingTicket.DueDate = request.DueDate;
                existingTicket.AssignedTo = request.AssignedTo;
                existingTicket.ProjectId = request.ProjectId;
                existingTicket.Resolution = request.Resolution;
                existingTicket.UpdatedAt = DateTime.UtcNow;

                var updated = await _ticketRepository.UpdateAsync(existingTicket);
                if (!updated)
                {
                    return StatusCode(500, "Failed to update ticket");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PATCH: api/tickets/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingTicket = await _ticketRepository.GetByIdAsync(id);
                if (existingTicket == null)
                {
                    return NotFound($"Ticket with ID {id} not found");
                }

                existingTicket.Status = request.Status;
                existingTicket.UpdatedAt = DateTime.UtcNow;

                var updated = await _ticketRepository.UpdateAsync(existingTicket);
                if (!updated)
                {
                    return StatusCode(500, "Failed to update ticket status");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PATCH: api/tickets/{id}/assign
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingTicket = await _ticketRepository.GetByIdAsync(id);
                if (existingTicket == null)
                {
                    return NotFound($"Ticket with ID {id} not found");
                }

                existingTicket.AssignedTo = request.EmployeeId;
                existingTicket.UpdatedAt = DateTime.UtcNow;

                var updated = await _ticketRepository.UpdateAsync(existingTicket);
                if (!updated)
                {
                    return StatusCode(500, "Failed to assign ticket");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/tickets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound($"Ticket with ID {id} not found");
                }

                var deleted = await _ticketRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, "Failed to delete ticket");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}