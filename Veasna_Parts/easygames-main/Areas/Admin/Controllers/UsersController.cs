using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EasyGames.Filters;         // AuthorizeOwner
using EasyGames.Data;            // ApplicationDbContext
using EasyGames.Models;          // User and Role
using EasyGames.ViewModels;      // AdminUserFormVM, AdminUserEditVM
using BCryptor = BCrypt.Net.BCrypt;

namespace EasyGames.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeOwner] // owner only, keep it this way got it trouble here at notepad at Ga
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UsersController(ApplicationDbContext db) => _db = db;

        // GET: /Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _db.Users
                .AsNoTracking()
                .OrderByDescending(u => u.Role) // Owner first
                .ThenBy(u => u.Name)
                .ToListAsync();

            // Convention: /Areas/Admin/Views/Users/Index.cshtml
            return View(users);
        }

        // GET: /Admin/Users/Create
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new AdminUserFormVM { Role = Role.Customer }; // sane default
            // Convention: /Areas/Admin/Views/Users/Create.cshtml
            return View(vm);
        }

        // POST: /Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserFormVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // unique email simple check
            var exists = await _db.Users.AnyAsync(u => u.Email == vm.Email);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Email), "Email already exists");
                return View(vm);
            }

            var user = new User
            {
                Name = vm.Name,
                Email = vm.Email,
                PasswordHash = BCryptor.HashPassword(vm.Password), // hash, no plain text
                Address = vm.Address ?? string.Empty,
                Role = vm.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "User created."; // mini feedback
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Users/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();

            var vm = new AdminUserEditVM
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Address = u.Address,
                Role = u.Role
            };

            // Convention: /Areas/Admin/Views/Users/Edit.cshtml
            return View(vm);
        }

        // POST: /Admin/Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminUserEditVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (u == null) return NotFound();

            // unique email except self
            var emailClash = await _db.Users.AnyAsync(x => x.Email == vm.Email && x.Id != vm.Id);
            if (emailClash)
            {
                ModelState.AddModelError(nameof(vm.Email), "Email already exists");
                return View(vm);
            }

            u.Name = vm.Name;
            u.Email = vm.Email;
            u.Address = vm.Address ?? string.Empty;
            u.Role = vm.Role;

            // change password only when provided
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
                u.PasswordHash = BCryptor.HashPassword(vm.NewPassword);

            await _db.SaveChangesAsync();
            TempData["Msg"] = "User updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (u == null)
            {
                TempData["Err"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // can't delete self, can't delete last Owner asked GPT for help bwcause i could not make  int.TryParse(currentIdString, out var currentId) work out 
            var currentIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(currentIdString, out var currentId);

            if (u.Id == currentId)
            {
                TempData["Err"] = "Cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            if (u.Role == Role.Owner)
            {
                var ownerCount = await _db.Users.CountAsync(x => x.Role == Role.Owner);
                if (ownerCount <= 1)
                {
                    TempData["Err"] = "Cannot delete the last Owner.";
                    return RedirectToAction(nameof(Index));
                }
            }

            _db.Users.Remove(u);
            await _db.SaveChangesAsync();
            TempData["Msg"] = "User deleted.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/ToggleRole/5  (Owner <-> Customer)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRole(int id)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (u == null)
            {
                TempData["Err"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            if (u.Role == Role.Owner)
            {
                // about to demote an Owner → ensure there is another Owner
                var ownerCount = await _db.Users.CountAsync(x => x.Role == Role.Owner);
                if (ownerCount <= 1)
                {
                    TempData["Err"] = "Cannot demote the last Owner.";
                    return RedirectToAction(nameof(Index));
                }
                u.Role = Role.Customer;
            }
            else
            {
                u.Role = Role.Owner;
            }

            await _db.SaveChangesAsync();
            TempData["Msg"] = "Role switched.";
            return RedirectToAction(nameof(Index));
        }
    }
}


