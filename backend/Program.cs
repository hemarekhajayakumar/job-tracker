using System.Text;
using System.Text.Json.Serialization;
using JobTracker.Api.Data;
using JobTracker.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. Register services (the things our app needs) ----------

// Controllers, and send enums as text ("Applied") instead of numbers (0).
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

// Database: SQLite keeps everything in one file (jobtracker.db). See appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<TokenService>();

// The secret key that signs login tokens. It is NOT in the code or on GitHub;
// it lives in "user secrets" on this computer. See README.md for how to set it.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing. See README.md, section 'First-time setup'.");

// Authentication: check the token that comes with each request.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TokenService.Issuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// CORS: browsers block a page on one address (React, port 5173) from calling
// another address (this API, port 5062) unless the API says it is allowed.
var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(frontendUrl).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---------- 2. Create the database file and tables if they do not exist yet ----------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ---------- 3. The request pipeline: every request passes through these, in order ----------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();   // Who are you? (reads the token)
app.UseAuthorization();    // Are you allowed? (checks [Authorize])

app.MapControllers();

app.Run();
