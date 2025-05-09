namespace IdentityServer.Controllers
{
    using IdentityServer.Application.Results;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult AsActionResult<T>(IdentityResult<T> result)
        {
            if (result == null)
                return NotFound("The requested result was not found.");

            if (result.IsSuccess)
                return Ok(result);

            if (!string.IsNullOrWhiteSpace(result.Error))
            { 
                if (result.Error.Contains("unexpected", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(StatusCodes.Status500InternalServerError, result.Error);

                return BadRequest(result.Error);
            }

            return BadRequest("An unknown error occurred.");
        }

    }
}
