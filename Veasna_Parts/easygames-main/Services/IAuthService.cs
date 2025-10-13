// auth contract see Notepad a1
using System.Security.Claims;
using EasyGames.Models;

namespace EasyGames.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(string name, string email, string password, string? address);
        Task<User?> ValidateAsync(string email, string password);
        ClaimsPrincipal ToPrincipal(User user);

        // NEW: persist password changes in the database
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        // NEW: convenience fetch
        Task<User?> FindByIdAsync(int id);
    }
}
