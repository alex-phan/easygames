using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using EasyGames.Models;
using EasyGames.Services;

namespace EasyGames.Controllers
{
    // Auth backed by DB via IAuthService (UnitOfWork + BCrypt) see readme
    // - Register: writes User row (Customer) and signs in
    // - Login: validates against stored hash, supports ReturnUrl & RememberMe
    // - Change Password: verifies current, updates stored hash (persists)
    // - Logout: POST -> redirects to Login page
    public class AccountController : Controller
    {
        private readonly IAuthService _auth;

        public AccountController(IAuthService auth)
        {
            _auth = auth;
        }

        // ---------- Register ----------
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new EasyGames.ViewModels.RegisterVM());
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register(EasyGames.ViewModels.RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = (model.Email ?? "").Trim().ToLowerInvariant();

            var created = await _auth.RegisterAsync(
                name: string.IsNullOrWhiteSpace(model.FullName) ? email.Split('@')[0] : model.FullName.Trim(),
                email: email,
                password: model.Password,
                address: null // RegisterVM has no Address; persist as null
            );

            if (created == null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            // Sign in newly registered user
            var principal = _auth.ToPrincipal(created);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            return RedirectToAction("Index", "Home");
        }

        // ---------- Login ----------
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        // edit update path reviewed with AI assist; safer flow = load entity -> map allowed fields -> save
        // repo pattern notes recorded in Notepad repo1 (avoid overposting, no AsNoTracking for update)

        // Keeps existing login form field names: email/password/rememberMe/returnUrl got ChatGpt help with this see README 
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(string? email, string? password, bool rememberMe = false, string? returnUrl = null)
        {
            var e = (email ?? string.Empty).Trim().ToLowerInvariant();
            var p = (password ?? string.Empty).Trim();

            var user = await _auth.ValidateAsync(e, p);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var principal = _auth.ToPrincipal(user);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = rememberMe });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ---------- Change Password (DB-persisted) ----------
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new EasyGames.ViewModels.ChangePasswordVM());
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(EasyGames.ViewModels.ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // NameIdentifier should be user Id when logged in via this controller
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId))
            {
                // If user was logged in with an old cookie (email as NameIdentifier), force re-login
                ModelState.AddModelError(string.Empty, "Please log out and log in again, then retry changing your password.");
                return View(model);
            }

            var ok = await _auth.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
            if (!ok)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
                return View(model);
            }

            TempData["Status"] = "Password changed successfully. You'll use the new password next time.";
            return RedirectToAction("ChangePassword");
        }

        // ---------- Logout -> always go to Login ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}





