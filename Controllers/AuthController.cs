using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging; // Add logging

namespace AspDotNetGoogleAuthExample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        try
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse), "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login initiation.");
            return StatusCode(500, "Internal server error."); // Return 500
        }
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            _logger.LogInformation($"Authentication succeeded: {authenticateResult.Succeeded}");
            _logger.LogInformation($"Authentication properties: {authenticateResult.Properties}");

            if (!authenticateResult.Succeeded)
            {
                _logger.LogError("Google authentication failed.");
                return BadRequest("Google authentication failed.");
            }

            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (email == null)
            {
                _logger.LogError("Email claim not found from google response");
                return BadRequest("Email not found");
            }
            return Ok(new { Email = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google response processing.");
            return StatusCode(500, "Internal server error.");
        }
    }
}