using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.DTOs;
using Shared.Contracts.Responses;
using Shared.Contracts.Validation;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        // Validate request at action level
        var validationErrors = RequestValidator.Validate(request);
        if (validationErrors != null)
        {
            _logger.LogWarning("Validation failed for registration request");
            return BadRequest(ApiResponse<AuthResponse>.ValidationErrorResponse(validationErrors));
        }

        try
        {
            var response = await _authService.RegisterAsync(request);
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "User registered successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message, "REGISTRATION_FAILED"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, ApiResponse<AuthResponse>.ErrorResponse("An error occurred during registration", "INTERNAL_ERROR"));
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        // Validate request at action level
        var validationErrors = RequestValidator.Validate(request);
        if (validationErrors != null)
        {
            _logger.LogWarning("Validation failed for login request");
            return BadRequest(ApiResponse<AuthResponse>.ValidationErrorResponse(validationErrors));
        }

        try
        {
            var response = await _authService.LoginAsync(request);
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse(ex.Message, "UNAUTHORIZED"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse<AuthResponse>.ErrorResponse("An error occurred during login", "INTERNAL_ERROR"));
        }
    }

    [HttpPost("validate")]
    [Authorize]
    public ActionResult<ApiResponse<object>> Validate()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var response = new { message = "Token is valid", userId };
        return Ok(ApiResponse<object>.SuccessResponse(response, "Token is valid"));
    }
}

