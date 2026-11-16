using System.ComponentModel.DataAnnotations;
using JobTracker.Api.Models;

namespace JobTracker.Api;

// DTO = Data Transfer Object: the shape of the JSON that travels between React and the API.
// We keep these separate from the database models so we never send things like PasswordHash.

// ----- What the browser sends to the API -----

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = "";

    [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class JobRequest
{
    [Required(ErrorMessage = "Company is required."), MaxLength(150)]
    public string Company { get; set; } = "";

    [Required(ErrorMessage = "Role is required."), MaxLength(150)]
    public string Role { get; set; } = "";

    [MaxLength(150)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? JobUrl { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    public DateOnly AppliedOn { get; set; }
}

public class StatusRequest
{
    public ApplicationStatus Status { get; set; }
}

// ----- What the API sends back to the browser -----

public record AuthResponse(string Token, string Name, string Email);

public record JobResponse(
    int Id,
    string Company,
    string Role,
    string? Location,
    string? JobUrl,
    string? Notes,
    ApplicationStatus Status,
    DateOnly AppliedOn);
