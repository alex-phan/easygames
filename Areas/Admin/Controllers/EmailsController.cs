using System;
using System.Linq;
using System.Threading.Tasks;
using EasyGames.Areas.Admin.ViewModels;
using EasyGames.Data;
using EasyGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Veasna File

namespace EasyGames.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Owner")]
    public class EmailsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;

        // classic constructor
        public EmailsController(ApplicationDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        [HttpGet]
        public IActionResult Compose() => View(new EmailGroupVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compose(EmailGroupVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Base query (adjust DbSet if needed)
            var q = _db.Users.AsNoTracking().AsQueryable();

            // Optional: filter by tier only if NOT sending to all and a tier is specified.
            if (!vm.SendToAll && vm.TargetTier.HasValue)
            {
                // If a User has a real Tier property, replace EF.Property with u => u.Tier
                q = q.Where(u => EF.Property<int?>(u, "Tier") == (int)vm.TargetTier.Value);
            }

            var recipients = await q
                .Select(u => u.Email)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToListAsync();

            if (recipients.Count == 0)
            {
                ModelState.AddModelError("", "No recipients found for that selection.");
                return View(vm);
            }

            // Try to render a template; if missing, fall back to simple HTML
            string html;
            try
            {
                html = await _email.RenderViewToStringAsync(
                    "/Views/Shared/EmailTemplates/BulkMessage.cshtml",
                    new { Title = vm.Subject, Body = vm.Message });
            }
            catch
            {
                html = $"<h3>{System.Net.WebUtility.HtmlEncode(vm.Subject)}</h3><div>{System.Net.WebUtility.HtmlEncode(vm.Message)}</div>";
            }

            try
            {
                await _email.SendBulkAsync(recipients, vm.Subject, html);
                TempData["Toast.Success"] = $"Sent to {recipients.Count} recipient(s).";
                return RedirectToAction(nameof(Sent), new { count = recipients.Count, simulated = false });
            }
            // If SMTP isn't configured, treat as simulated success (assignment/demo mode)
            catch (InvalidOperationException ex) when (
                ex.Message.StartsWith("SMTP not configured", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Toast.Info"] = $"Simulated send (no SMTP configured). {recipients.Count} recipient(s).";
                return RedirectToAction(nameof(Sent), new { count = recipients.Count, simulated = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Email send failed: {ex.Message}");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult Sent(int count, bool simulated = false)
        {
            ViewBag.SentCount = count;
            ViewBag.Simulated = simulated;
            return View();
        }
    }
}