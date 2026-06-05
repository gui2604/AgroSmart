using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Services;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(UserRegisterDto dto);
    Task<AuthResponseDto> LoginAsync(UserLoginDto dto);
}

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _hasher = new();

    public UserService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _users.ExistsAsync(u => u.Email == email))
            throw new ValidationException("Já existe um usuário com este e-mail.");

        var user = new User
        {
            Email = email,
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Operator" : dto.Role,
            PasswordHash = string.Empty
        };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        return BuildToken(user);
    }

    public async Task<AuthResponseDto> LoginAsync(UserLoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email)
            ?? throw new ValidationException("Credenciais inválidas.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new ValidationException("Credenciais inválidas.");

        return BuildToken(user);
    }

    private AuthResponseDto BuildToken(User user)
    {
        var keyValue = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não configurada.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(4);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires,
            Email = user.Email,
            Role = user.Role
        };
    }
}
