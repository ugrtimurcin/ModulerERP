using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.SystemCore.Application.Constants;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/system")]
[Authorize]
public class SystemController : ControllerBase
{
    [HttpGet("permissions")]
    [Authorize(Policy = Permissions.Roles.View)]
    public IActionResult GetPermissions()
    {
        var permissions = Permissions.GetAll();
        return Ok(permissions);
    }
}
