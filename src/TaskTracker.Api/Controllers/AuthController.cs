using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Api.DTOs;
using TaskTracker.Application.Commands;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers;

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

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        // TODO: Add rate limiting for login attempts
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _authService.LoginAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var response = new LoginResponse
        {
            Token = result.Token,
            ExpiresAt = result.ExpiresAt,
            User = new UserResponse
            {
                Id = result.User!.Id,
                Email = result.User.Email,
                DisplayName = result.User.DisplayName,
                CreatedAt = result.User.CreatedAt
            }
        };

        _logger.LogInformation("Successful login for user: {UserId}", result.User.Id);
        return Ok(response);
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var command = new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _authService.RegisterAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed registration attempt for email: {Email}. Error: {Error}", request.Email, result.ErrorMessage);
            return BadRequest(new { message = result.ErrorMessage });
        }

        var response = new LoginResponse
        {
            Token = result.Token,
            ExpiresAt = result.ExpiresAt,
            User = new UserResponse
            {
                Id = result.User!.Id,
                Email = result.User.Email,
                DisplayName = result.User.DisplayName,
                CreatedAt = result.User.CreatedAt
            }
        };

        _logger.LogInformation("Successful registration for user: {UserId}", result.User.Id);
        return Ok(response);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        
        _logger.LogInformation("Password change request for user: {UserId}", currentUserId);

        var command = new ChangePasswordCommand
        {
            UserId = currentUserId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var success = await _authService.ChangePasswordAsync(command, ct);

        if (!success)
        {
            _logger.LogWarning("Failed password change for user: {UserId}", currentUserId);
            return BadRequest(new { message = "Current password is incorrect" });
        }

        _logger.LogInformation("Successful password change for user: {UserId}", currentUserId);
        return NoContent();
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        
        var user = await _authService.GetUserByIdAsync(currentUserId, ct);
        
        if (user == null)
        {
            return NotFound();
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };

        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }
}