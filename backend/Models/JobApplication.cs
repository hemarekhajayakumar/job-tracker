namespace JobTracker.Api.Models;

// The stages a job application can be in. These are the columns of the board.
public enum ApplicationStatus
{
    Applied,
    Interview,
    Offer,
    Rejected
}

// One row in the JobApplications table = one job you applied for.
public class JobApplication
{
    public int Id { get; set; }
    public string Company { get; set; } = "";
    public string Role { get; set; } = "";
    public string? Location { get; set; }
    public string? JobUrl { get; set; }
    public string? Notes { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public DateOnly AppliedOn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Which user owns this application (a "foreign key" to the Users table).
    public int UserId { get; set; }
    public User? User { get; set; }
}
