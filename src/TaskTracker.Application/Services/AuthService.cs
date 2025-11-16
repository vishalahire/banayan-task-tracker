using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskTracker.Application.Commands;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Repositories;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IUserRepository _userRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IUserRepository userRepository,
        IAuditRepository auditRepository,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _auditRepository = auditRepository;
        _configuration = configuration;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginCommand command, CancellationToken ct = default)
    {
        var identityUser = await _userManager.FindByEmailAsync(command.Email);
        if (identityUser == null)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid email or password"
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(identityUser, command.Password, false);
        if (!result.Succeeded)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid email or password"
            };
        }

        // Get or create domain user
        var domainUser = await GetOrCreateDomainUser(identityUser, ct);

        // Generate JWT token
        var token = GenerateJwtToken(identityUser);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(GetJwtExpiryMinutes());

        // Record login audit event
        var auditEvent = new AuditEvent(Domain.Enums.AuditAction.UserLogin, domainUser.Id, null, nameof(User), "User logged in");
        await _auditRepository.AddAsync(auditEvent, ct);

        return new AuthenticationResult
        {
            IsSuccess = true,
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = domainUser.Id,
                Email = domainUser.Email,
                DisplayName = domainUser.DisplayName,
                CreatedAt = domainUser.CreatedAt
            }
        };
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterCommand command, CancellationToken ct = default)
    {
        // Check if user already exists
        var existingIdentityUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingIdentityUser != null)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "A user with this email address already exists"
            };
        }

        // Create Identity user
        var identityUser = new IdentityUser
        {
            UserName = command.Email,
            Email = command.Email,
            EmailConfirmed = true // For simplicity, auto-confirm emails
        };

        var identityResult = await _userManager.CreateAsync(identityUser, command.Password);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = $"Registration failed: {errors}"
            };
        }

        // Create domain user with the same ID as Identity user
        var displayName = $"{command.FirstName} {command.LastName}".Trim();
        var domainUser = new User(command.Email, displayName)
        {
            Id = Guid.Parse(identityUser.Id)
        };
        
        var domainUserId = await _userRepository.AddAsync(domainUser, ct);

        // Generate JWT token for immediate login
        var token = GenerateJwtToken(identityUser);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(GetJwtExpiryMinutes());

        // Record registration audit event
        var auditEvent = new AuditEvent(Domain.Enums.AuditAction.UserCreated, domainUserId, null, nameof(User), "User registered");
        await _auditRepository.AddAsync(auditEvent, ct);

        return new AuthenticationResult
        {
            IsSuccess = true,
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = domainUserId,
                Email = domainUser.Email,
                DisplayName = domainUser.DisplayName,
                CreatedAt = domainUser.CreatedAt
            }
        };
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken ct = default)
    {
        var domainUser = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (domainUser == null)
        {
            return false;
        }

        var identityUser = await _userManager.FindByEmailAsync(domainUser.Email);
        if (identityUser == null)
        {
            return false;
        }

        var result = await _userManager.ChangePasswordAsync(identityUser, command.CurrentPassword, command.NewPassword);
        return result.Succeeded;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<User> GetOrCreateDomainUser(IdentityUser identityUser, CancellationToken ct)
    {
        if (!Guid.TryParse(identityUser.Id, out var userId))
        {
            throw new InvalidOperationException("Invalid user ID format");
        }

        // First try to find by email (in case user already exists from registration)
        var domainUser = await _userRepository.GetByEmailAsync(identityUser.Email!, ct);
        if (domainUser == null)
        {
            // If not found by email, try by ID
            domainUser = await _userRepository.GetByIdAsync(userId, ct);
        }

        if (domainUser == null)
        {
            // Create domain user if it doesn't exist
            domainUser = new User(identityUser.Email!, identityUser.UserName ?? identityUser.Email!)
            {
                Id = userId
            };
            await _userRepository.AddAsync(domainUser, ct);
        }

        return domainUser;
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetJwtExpiryMinutes()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        return int.TryParse(jwtSettings["ExpiryMinutes"], out var minutes) ? minutes : 60;
    }
}