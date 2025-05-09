namespace PeopleManagement.Controllers
{
    using Microsoft.AspNetCore.Authorization; 
    using Microsoft.AspNetCore.Mvc;

    using PeopleManagement.Models;
    using PeopleManagement.Services;

    [ApiController]
    [Route("api/[controller]")]
    public class PeopleController : ControllerBase
    {
        private readonly IRoleManagerClient _roleManager; 
        public PeopleController(IRoleManagerClient roleManager) => _roleManager = roleManager;

        [HttpGet("roles/{userId}")]
        [Authorize(Roles = "MANAGER,HR ADMIN")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var result = await _roleManager.GetRolesAsync(userId);
            return result.Success ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpPost("roles")]
        [Authorize(Roles = "MANAGER,HR ADMIN")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var result = await _roleManager.CreateRoleAsync(dto.RoleName, dto.Description);
            return result.Success ? Ok() : BadRequest(result.ErrorMessage);
        }

        [HttpDelete("roles/{roleId}")]
        [Authorize(Roles = "HR ADMIN")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var result = await _roleManager.DeleteRoleAsync(roleId);
            return result.Success ? Ok() : BadRequest(result.ErrorMessage);
        }

        [HttpPost("claims/{userId}/add")]
        [Authorize(Roles = "MANAGER,HR ADMIN")]
        public async Task<IActionResult> AddClaim(int userId, [FromBody] ClaimDTo claim)
        {
            var result = await _roleManager.AddClaimAsync(userId, claim);
            return result.Success ? Ok() : BadRequest(result.ErrorMessage);
        }

        [HttpPost("claims/{userId}/remove")]
        [Authorize(Roles = "HR ADMIN")]
        public async Task<IActionResult> RemoveClaim(int userId, [FromBody] ClaimDTo claim)  
        {
            var result = await _roleManager.RemoveClaimAsync(userId, claim);
            return result.Success ? Ok() : BadRequest(result.ErrorMessage);
        }

        [HttpGet("claims/{userId}")]
        [Authorize(Roles = "MANAGER,HR ADMIN")]
        public async Task<IActionResult> GetClaims(int userId)
        {
            var result = await _roleManager.GetClaimsAsync(userId);
            return result.Success ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }


}
