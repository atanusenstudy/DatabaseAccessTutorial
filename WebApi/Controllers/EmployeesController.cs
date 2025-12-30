using Core.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Core.DTO;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeesController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // GET: api/employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            try
            {
                var employees = await _employeeRepository.GetAllAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/employees/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {id} not found");
                }
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/employees/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult<Employee>> GetEmployeeByEmail(string email)
        {
            try
            {
                var employee = await _employeeRepository.GetByEmailAsync(email);
                if (employee == null)
                {
                    return NotFound($"Employee with email {email} not found");
                }
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/employees/department/{department}
        [HttpGet("department/{department}")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByDepartment(string department)
        {
            try
            {
                var employees = await _employeeRepository.GetByDepartmentAsync(department);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/employees/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetActiveEmployees()
        {
            try
            {
                var employees = await _employeeRepository.GetActiveEmployeesAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/employees
        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(EmployeeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var employee = new Employee
                {
                    Name = request.Name,
                    Department = request.Department,
                    Email = request.Email,
                    HireDate = request.HireDate,
                    Phone = request.Phone,
                    Address = request.Address,
                    Salary = request.Salary,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var id = await _employeeRepository.AddAsync(employee);
                employee.Id = id;

                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/employees/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingEmployee = await _employeeRepository.GetByIdAsync(id);
                if (existingEmployee == null)
                {
                    return NotFound($"Employee with ID {id} not found");
                }

                existingEmployee.Name = request.Name;
                existingEmployee.Department = request.Department;
                existingEmployee.Email = request.Email;
                existingEmployee.HireDate = request.HireDate;
                existingEmployee.Phone = request.Phone;
                existingEmployee.Address = request.Address;
                existingEmployee.Salary = request.Salary;
                existingEmployee.IsActive = request.IsActive;
                existingEmployee.UpdatedAt = DateTime.UtcNow;

                var updated = await _employeeRepository.UpdateAsync(existingEmployee);
                if (!updated)
                {
                    return StatusCode(500, "Failed to update employee");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/employees/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound($"Employee with ID {id} not found");
                }

                var deleted = await _employeeRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, "Failed to delete employee");
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