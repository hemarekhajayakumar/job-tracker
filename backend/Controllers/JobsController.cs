using System.Security.Claims;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Controllers;

// Handles job applications: list, create, update, change status, delete.
// [Authorize] means every URL here needs a valid login token.
[ApiController]
[Authorize]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _db;

    public JobsController(AppDbContext db)
    {
        _db = db;
    }

    // The id of the logged-in user, read from their token.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/jobs  ->  all applications of the logged-in user
    [HttpGet]
    public async Task<List<JobResponse>> GetAll()
    {
        var jobs = await _db.JobApplications
            .Where(job => job.UserId == CurrentUserId)
            .OrderByDescending(job => job.AppliedOn)
            .ThenByDescending(job => job.Id)
            .ToListAsync();

        return jobs.Select(ToResponse).ToList();
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _db.JobApplications
            .Where(job => job.UserId == CurrentUserId)
            .GroupBy(job => job.Status)
            .Select(g => new {Status = g.Key , Count = g.Count() })
            .ToListAsync();
        
        return Ok(stats);

    }

    // POST /api/jobs  ->  add a new application
    [HttpPost]
    public async Task<ActionResult<JobResponse>> Create(JobRequest request)
    {
        var job = new JobApplication { UserId = CurrentUserId };
        CopyFields(request, job);

        _db.JobApplications.Add(job);
        await _db.SaveChangesAsync();

        return Created($"/api/jobs/{job.Id}", ToResponse(job));
    }

    // PUT /api/jobs/5  ->  replace all fields of application 5
    [HttpPut("{id}")]
    public async Task<ActionResult<JobResponse>> Update(int id, JobRequest request)
    {
        var job = await FindOwnJob(id);
        if (job is null) return NotFound();

        CopyFields(request, job);
        await _db.SaveChangesAsync();

        return ToResponse(job);
    }

    // PATCH /api/jobs/5/status  ->  change only the status (used when a card moves on the board)
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<JobResponse>> UpdateStatus(int id, StatusRequest request)
    {
        var job = await FindOwnJob(id);
        if (job is null) return NotFound();

        job.Status = request.Status;
        await _db.SaveChangesAsync();

        return ToResponse(job);
    }

    // DELETE /api/jobs/5  ->  remove application 5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var job = await FindOwnJob(id);
        if (job is null) return NotFound();

        _db.JobApplications.Remove(job);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Finds a job only if it belongs to the logged-in user.
    // This check is what stops one user from touching another user's data.
    private Task<JobApplication?> FindOwnJob(int id)
    {
        return _db.JobApplications
            .FirstOrDefaultAsync(job => job.Id == id && job.UserId == CurrentUserId);
    }

    private static void CopyFields(JobRequest request, JobApplication job)
    {
        job.Company = request.Company.Trim();
        job.Role = request.Role.Trim();
        job.Location = request.Location?.Trim();
        job.JobUrl = request.JobUrl?.Trim();
        job.Notes = request.Notes?.Trim();
        job.Status = request.Status;
        job.AppliedOn = request.AppliedOn;
    }

    private static JobResponse ToResponse(JobApplication job)
    {
        return new JobResponse(
            job.Id, job.Company, job.Role, job.Location,
            job.JobUrl, job.Notes, job.Status, job.AppliedOn);
    }
}
