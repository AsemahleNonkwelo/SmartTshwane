using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTshwane.Data;
using SmartTshwane.Models;

namespace SmartTshwane.Controllers;

// API shared by Windows Forms and Android. The database password stays on the server.
[ApiController]
[Route("api/v1")]
public class MobileApiController(AppDb db, IConfiguration configuration) : ControllerBase
{
    private record TokenPayload(int UserId, long Expires);
    public record RegisterDto(string FullName, string PhoneNumber, string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record ReportDto(string Title, string Description, string Location, int CategoryId, double? Latitude, double? Longitude);
    public record UpdateDto(string Status, string? Note);

    private byte[] Key()
    {
        var key = configuration["MobileApi:SigningKey"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Set MobileApi__SigningKey to a secret of at least 32 UTF-8 bytes before using the mobile API.");
        return Encoding.UTF8.GetBytes(key);
    }
    private static string Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] Unurl(string value) => Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/') + new string('=', (4 - value.Length % 4) % 4));
    private string Issue(int id)
    {
        var payload = Url(JsonSerializer.SerializeToUtf8Bytes(new TokenPayload(id, DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds())));
        var signature = Url(HMACSHA256.HashData(Key(), Encoding.UTF8.GetBytes(payload)));
        return payload + "." + signature;
    }
    private async Task<User?> Auth()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;
        var parts = header[7..].Trim().Split('.');
        if (parts.Length != 2) return null;
        try
        {
            var expected = HMACSHA256.HashData(Key(), Encoding.UTF8.GetBytes(parts[0]));
            if (!CryptographicOperations.FixedTimeEquals(expected, Unurl(parts[1]))) return null;
            var payload = JsonSerializer.Deserialize<TokenPayload>(Unurl(parts[0]));
            if (payload == null || payload.Expires <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;
            return await db.Users.FindAsync(payload.UserId);
        }
        catch (FormatException) { return null; }
        catch (JsonException) { return null; }
    }
    private static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
    private static bool Verify(string password, string stored)
    {
        try
        {
            var parts = stored.Split(':');
            var expected = Convert.FromBase64String(parts[1]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(parts[0]), 210000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch { return false; }
    }
    private static object UserView(User u) => new { u.Id, u.FullName, u.Email, u.PhoneNumber, u.Role };
    private object Session(User u) => new { token = Issue(u.Id), user = UserView(u) };

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", app = "SmartTshwane", utc = DateTime.UtcNow });

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName) || string.IsNullOrWhiteSpace(input.PhoneNumber) ||
            string.IsNullOrWhiteSpace(input.Email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(input.Email) ||
            string.IsNullOrWhiteSpace(input.Password) || input.Password.Length < 8)
            return BadRequest(new { error = "Provide name, phone, valid email and password of at least 8 characters." });
        var email = input.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email)) return Conflict(new { error = "Email already registered." });
        var user = new User { FullName = input.FullName.Trim(), PhoneNumber = input.PhoneNumber.Trim(), Email = email, PasswordHash = Hash(input.Password), Role = "Citizen" };
        db.Users.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { error = "Account could not be created. Check whether the email is already registered." }); }
        return Ok(Session(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto input)
    {
        var email = (input.Email ?? "").Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null || !Verify(input.Password ?? "", user.PasswordHash)) return Unauthorized(new { error = "Invalid email or password." });
        return Ok(Session(user));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me() { var u = await Auth(); return u == null ? Unauthorized() : Ok(UserView(u)); }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories() => Ok(await db.Categories.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name, x.DepartmentId }).ToListAsync());

    [HttpGet("departments")]
    public async Task<IActionResult> Departments() => Ok(await db.Departments.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync());

    [HttpGet("reports")]
    public async Task<IActionResult> Reports()
    {
        var u = await Auth(); if (u == null) return Unauthorized();
        var q = db.Requests.Include(r => r.Category).AsQueryable();
        if (u.Role != "Admin") q = q.Where(r => r.UserId == u.Id);
        return Ok(await q.OrderByDescending(r => r.CreatedUtc).Select(r => new { r.Id, r.Title, r.Description, r.Location, r.Latitude, r.Longitude, r.Status, r.CreatedUtc, r.AdminNote, r.CategoryId, Category = r.Category == null ? "" : r.Category.Name, r.UserId }).ToListAsync());
    }

    [HttpPost("reports")]
    public async Task<IActionResult> CreateReport(ReportDto input)
    {
        var u = await Auth(); if (u == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Length > 120 || string.IsNullOrWhiteSpace(input.Description) || input.Description.Length > 2000 || string.IsNullOrWhiteSpace(input.Location) || input.Location.Length > 200)
            return BadRequest(new { error = "Title, description and location are required and must fit the field limits." });
        if (input.Latitude.HasValue != input.Longitude.HasValue || input.Latitude is < -90 or > 90 || input.Longitude is < -180 or > 180)
            return BadRequest(new { error = "Coordinates must be a valid latitude/longitude pair." });
        if (!await db.Categories.AnyAsync(c => c.Id == input.CategoryId)) return BadRequest(new { error = "Choose a valid category." });
        var report = new ServiceRequest { Title = input.Title.Trim(), Description = input.Description.Trim(), Location = input.Location.Trim(), CategoryId = input.CategoryId, Latitude = input.Latitude, Longitude = input.Longitude, UserId = u.Id, Status = "Pending", CreatedUtc = DateTime.UtcNow };
        db.Requests.Add(report); await db.SaveChangesAsync();
        return Ok(new { report.Id, reference = $"TSH-{report.Id}", report.Status });
    }

    [HttpPut("reports/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateDto input)
    {
        var u = await Auth(); if (u == null) return Unauthorized();
        if (u.Role != "Admin") return Forbid();
        if (!new[] { "Pending", "In Progress", "Resolved", "Rejected" }.Contains(input.Status)) return BadRequest(new { error = "Invalid status." });
        var report = await db.Requests.FindAsync(id); if (report == null) return NotFound();
        report.Status = input.Status; report.AdminNote = input.Note?.Trim();
        db.Notifications.Add(new Notification { UserId = report.UserId, Message = $"Report #TSH-{report.Id} status: {input.Status}. {report.AdminNote}" });
        await db.SaveChangesAsync(); return Ok(new { report.Id, report.Status });
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications()
    {
        var u = await Auth(); if (u == null) return Unauthorized();
        return Ok(await db.Notifications.Where(n => n.UserId == u.Id).OrderByDescending(n => n.CreatedUtc).Select(n => new { n.Id, n.Message, n.IsRead, n.CreatedUtc }).ToListAsync());
    }
}
