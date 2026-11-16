using JobTracker.Api.Data;
using JobTracker.Api.Models;
using JobTracker.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Controllers;

// Handles creating an account and logging in.
// URLs: POST /api/auth/register and POST /api/auth/login
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly PasswordHasher<User> _hasher = new();

    // .NET passes these in for us. This is "dependency injection".
    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var emailTaken = await _db.Users.AnyAsync(user => user.Email == email);
        if (emailTaken)
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var user = new User { Name = request.Name.Trim(), Email = email };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new AuthResponse(_tokens.CreateToken(user), user.Name, user.Email);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same message for "no such email" and "wrong password",
        // so nobody can use this to find out which emails are registered.
        if (user is null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Email or password is incorrect." });
        }

        return new AuthResponse(_tokens.CreateToken(user), user.Name, user.Email);
    }
}
