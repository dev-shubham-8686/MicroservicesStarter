using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Common;
using Shared.Contracts.DTOs;
using Shared.Contracts.Validators;
using Shared.Infrastructure.Controllers;
using Shared.Infrastructure.Exceptions;
using Shared.Infrastructure.Extensions;
using FluentValidation;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with token</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var response = await _authService.RegisterAsync(request);
        _logger.LogInformation("User registered successfully: {Email}", request.Email);

        return Success(response);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var response = await _authService.LoginAsync(request);
        _logger.LogInformation("User logged in successfully: {Email}", request.Email);

        return Success(response);
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <returns>Token validation result</returns>
    [HttpPost("validate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<object>> Validate()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Success<object>(new { 
            message = "Token is valid", 
            userId,
            email
        });
    }
}
