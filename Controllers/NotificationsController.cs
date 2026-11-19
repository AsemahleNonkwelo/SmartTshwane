using System.Security.Claims;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;using SmartTshwane.Data;
namespace SmartTshwane.Controllers;
[Authorize] public class NotificationsController(AppDb db):Controller{
public async Task<IActionResult> Index(){int uid=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);return View(await db.Notifications.Where(x=>x.UserId==uid).OrderByDescending(x=>x.CreatedUtc).ToListAsync());}
[HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Read(int id){int uid=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);var n=await db.Notifications.FirstOrDefaultAsync(x=>x.Id==id&&x.UserId==uid);if(n==null)return NotFound();n.IsRead=true;await db.SaveChangesAsync();return RedirectToAction("Index");}}
