namespace JobTracker.Api.Models;

// One row in the Users table = one person who can log in.
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";

    // We never store the real password, only a scrambled (hashed) version of it.
    public string PasswordHash { get; set; } = "";

    // One user has many job applications.
    public List<JobApplication> JobApplications { get; set; } = new();
}
