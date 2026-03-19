using McpExample.API.Security;
using Microsoft.AspNetCore.Mvc;

namespace McpExample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(JwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Issues a JWT token for the provided credentials.
    /// For demonstration, any non-empty username/password pair is accepted.
    /// </summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Token([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        // In production, validate credentials against a user store.
        // Here we accept any non-empty credentials for demonstration purposes.
        var token = _jwtTokenService.GenerateToken(request.Username);
        return Ok(new TokenResponse(token));
    }
}

public record LoginRequest(string Username, string Password);
public record TokenResponse(string Token);
