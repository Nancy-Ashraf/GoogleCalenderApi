
using Microsoft.AspNetCore.Mvc;

namespace GoogleCalendarApi;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private IGoogleCalendarService _googleCalendarService;

    public UserController(IGoogleCalendarService googleCalendarService)
    {
        _googleCalendarService = googleCalendarService;
    }

    [HttpGet]
    [Route("auth/google")]
    public IActionResult GoogleAuth()
    {
        return Redirect(_googleCalendarService.GetAuthCode());
    }

    [HttpGet]
    [Route("auth/callback")]
    public async Task<IActionResult> Callback()
    {
        string code = HttpContext.Request.Query["code"];
        //string scope = HttpContext.Request.Query["scope"];

        // Call your service to get tokens
        var token = await _googleCalendarService.GetTokens(code);
        return Ok(token);
    }

}
