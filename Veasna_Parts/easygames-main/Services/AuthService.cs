// simple auth using BCrypt 
using System.Security.Claims;
using EasyGames.Models;
using EasyGames.Repositories;

namespace EasyGames.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        public AuthService(IUnitOfWork uow) { _uow = uow; }

        public async Task<User?> RegisterAsync(string name, string email, string password, string? address)
        {
            // normalize email to avoid duplicate-by-case and login confusion
            var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();

            var exists = (await _uow.Users.GetAllAsync(u => u.Email.ToLower() == normalized)).Any();
            if (exists) return null; // email in use see Notepad a2.a

            var user = new User
            {
                Name = name,
                Email = normalized,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Address = address,
                Role = Role.Customer
            };

            await _uow.Users.AddAsync(user);
            await _uow.Users.SaveAsync();
            return user;
        }

        public async Task<User?> ValidateAsync(string email, string password)
        {
            var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();

            var user = (await _uow.Users.GetAllAsync(u => u.Email.ToLower() == normalized)).FirstOrDefault();
            if (user == null) return null;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null; // see Notes.
        }

        public ClaimsPrincipal ToPrincipal(User user)
        {
            var claims = new List<Claim>{
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"));
        }

        // NEW: change password persisted to DB (verifies current first)
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            // Repository doesn’t expose GetById in the snippet, so query by predicate
            var user = (await _uow.Users.GetAllAsync(u => u.Id == userId)).FirstOrDefault();
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _uow.Users.SaveAsync();
            return true;
        }

        // NEW: convenience fetch by Id
        public async Task<User?> FindByIdAsync(int id)
        {
            return (await _uow.Users.GetAllAsync(u => u.Id == id)).FirstOrDefault();
        }
    }
}

