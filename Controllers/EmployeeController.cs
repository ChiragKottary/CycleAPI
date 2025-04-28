using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IAuthRepository authRepository, ILogger<EmployeeController> logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        // GET: api/Employee
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployees()
        {
            try
            {
                _logger.LogInformation("Getting all employees");
                var employees = await _authRepository.GetAllEmployeesAsync();
                
                if (employees == null)
                {
                    _logger.LogWarning("GetAllEmployeesAsync returned null");
                    return StatusCode(500, "Employee data could not be retrieved");
                }
                
                _logger.LogInformation($"Found {employees.Count()} employees");
                
                var employeeDtos = new List<EmployeeDto>();
                foreach (var employee in employees)
                {
                    try 
                    {
                        employeeDtos.Add(MapToEmployeeDto(employee));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error mapping employee {employee.UserId}: {ex.Message}");
                        // Continue processing other employees
                    }
                }
                
                return Ok(employeeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving employees: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, "An error occurred while retrieving employees");
            }
        }

        // GET: api/Employee/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(Guid id)
        {
            try
            {
                _logger.LogInformation($"Getting employee with ID: {id}");
                var employee = await _authRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee with ID {id} not found");
                    return NotFound($"Employee with ID {id} not found");
                }

                return Ok(MapToEmployeeDto(employee));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving employee: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the employee");
            }
        }

        // POST: api/Employee
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RegisterResponseDto>> CreateEmployee([FromBody] RegisterRequestDto request)
        {
            try
            {
                _logger.LogInformation($"Creating new employee with email: {request.Email}");
                
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) ||
                    string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
                {
                    return BadRequest("Required fields are missing");
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };

                var response = await _authRepository.AddAsync(user);

                if (!response.Success)
                {
                    _logger.LogWarning($"Failed to create employee: {response.Message}");
                    return BadRequest(response.Message);
                }

                _logger.LogInformation($"Employee created successfully with ID: {response.UserId}");
                return CreatedAtAction(nameof(GetEmployee), new { id = response.UserId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating employee: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the employee");
            }
        }

        // PUT: api/Employee/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployeeDto>> UpdateEmployee(Guid id, [FromBody] EmployeeUpdateDto request)
        {
            try
            {
                _logger.LogInformation($"Updating employee with ID: {id}");
                
                var employee = await _authRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee with ID {id} not found");
                    return NotFound($"Employee with ID {id} not found");
                }

                // Update only the fields that are provided
                if (!string.IsNullOrEmpty(request.FirstName))
                    employee.FirstName = request.FirstName;

                if (!string.IsNullOrEmpty(request.LastName))
                    employee.LastName = request.LastName;

                if (!string.IsNullOrEmpty(request.PhoneNumber))
                    employee.PhoneNumber = request.PhoneNumber;
                    
                if (!string.IsNullOrEmpty(request.Address))
                    employee.Address = request.Address;

                if (!string.IsNullOrEmpty(request.Email))
                {
                    // Check if email is already in use by another employee
                    if (employee.Email != request.Email)
                    {
                        var existingEmployee = await _authRepository.GetByEmailAsync(request.Email);
                        if (existingEmployee != null && existingEmployee.UserId != id)
                        {
                            _logger.LogWarning($"Email {request.Email} is already in use");
                            return BadRequest($"Email {request.Email} is already in use");
                        }
                    }
                    employee.Email = request.Email;
                    employee.Username = request.Email.ToLower();
                }

                if (!string.IsNullOrEmpty(request.Password))
                {
                    employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                if (request.RoleId.HasValue)
                {
                    // We need to check if the role exists
                    var roleExists = await _authRepository.RoleExistsAsync(request.RoleId.Value);
                    if (!roleExists)
                    {
                        _logger.LogWarning($"Role with ID {request.RoleId} not found");
                        return BadRequest($"Role with ID {request.RoleId} not found");
                    }
                    employee.RoleId = request.RoleId.Value;
                }

                if (request.IsActive.HasValue)
                {
                    employee.IsActive = request.IsActive.Value;
                }

                employee.UpdatedAt = DateTime.UtcNow;

                var success = await _authRepository.UpdateAsync(employee);
                if (!success)
                {
                    _logger.LogWarning("Failed to update employee");
                    return StatusCode(500, "Failed to update employee");
                }

                _logger.LogInformation($"Employee with ID {id} updated successfully");
                return Ok(MapToEmployeeDto(employee));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating employee: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the employee");
            }
        }

        // DELETE: api/Employee/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            try
            {
                _logger.LogInformation($"Deleting employee with ID: {id}");
                
                if (!await _authRepository.ExistsAsync(id))
                {
                    _logger.LogWarning($"Employee with ID {id} not found");
                    return NotFound($"Employee with ID {id} not found");
                }

                var success = await _authRepository.DeleteAsync(id);
                if (!success)
                {
                    _logger.LogWarning($"Failed to delete employee with ID {id}");
                    return StatusCode(500, "Failed to delete employee");
                }

                _logger.LogInformation($"Employee with ID {id} deleted successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting employee: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the employee");
            }
        }

        // PATCH: api/Employee/{id}/activate
        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivateEmployee(Guid id)
        {
            try
            {
                _logger.LogInformation($"Activating employee with ID: {id}");
                
                if (!await _authRepository.ExistsAsync(id))
                {
                    _logger.LogWarning($"Employee with ID {id} not found");
                    return NotFound($"Employee with ID {id} not found");
                }

                var success = await _authRepository.ActivateEmployeeAsync(id);
                if (!success)
                {
                    _logger.LogWarning($"Failed to activate employee with ID {id}");
                    return StatusCode(500, "Failed to activate employee");
                }

                _logger.LogInformation($"Employee with ID {id} activated successfully");
                return Ok(new { Message = "Employee activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error activating employee: {ex.Message}");
                return StatusCode(500, "An error occurred while activating the employee");
            }
        }

        // PATCH: api/Employee/{id}/deactivate
        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateEmployee(Guid id)
        {
            try
            {
                _logger.LogInformation($"Deactivating employee with ID: {id}");
                
                if (!await _authRepository.ExistsAsync(id))
                {
                    _logger.LogWarning($"Employee with ID {id} not found");
                    return NotFound($"Employee with ID {id} not found");
                }

                var success = await _authRepository.DeactivateEmployeeAsync(id);
                if (!success)
                {
                    _logger.LogWarning($"Failed to deactivate employee with ID {id}");
                    return StatusCode(500, "Failed to deactivate employee");
                }

                _logger.LogInformation($"Employee with ID {id} deactivated successfully");
                return Ok(new { Message = "Employee deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deactivating employee: {ex.Message}");
                return StatusCode(500, "An error occurred while deactivating the employee");
            }
        }

        private EmployeeDto MapToEmployeeDto(User employee)
        {
            try
            {
                return new EmployeeDto
                {
                    UserId = employee.UserId,
                    Username = employee.Username ?? string.Empty,
                    Email = employee.Email ?? string.Empty,
                    FirstName = employee.FirstName ?? string.Empty,
                    LastName = employee.LastName ?? string.Empty,
                    PhoneNumber = employee.PhoneNumber ?? string.Empty,
                    Address = employee.Address ?? string.Empty,
                    Role = employee.Role?.RoleName ?? "Employee",
                    IsActive = employee.IsActive,
                    CreatedAt = employee.CreatedAt,
                    UpdatedAt = employee.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error mapping User to EmployeeDto: {ex.Message}");
                // Return a minimal valid DTO to prevent cascading failures
                return new EmployeeDto
                {
                    UserId = employee?.UserId ?? Guid.Empty,
                    Username = "Error",
                    Email = "Error",
                    FirstName = "Error",
                    LastName = "Error",
                    Role = "Employee",
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
        }
    }
}