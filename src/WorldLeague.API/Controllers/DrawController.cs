using WorldLeague.API.DTOs;
using WorldLeague.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace WorldLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrawController(IDrawService drawService) : ControllerBase
{
    /// <summary>
    /// Performs a randomized group draw for the World League.
    /// GroupCount must be 4 or 8. DrawerFirstName and DrawerLastName are required.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DrawResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Draw([FromBody] DrawRequest request)
    {
        var result = await drawService.PerformDrawAsync(request);
        return Ok(result);
    }
}
