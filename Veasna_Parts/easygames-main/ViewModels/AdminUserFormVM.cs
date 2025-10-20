using System.ComponentModel.DataAnnotations;
using EasyGames.Models; // Role enum

namespace EasyGames.ViewModels
{
    // keep it minimal for create user
    public class AdminUserFormVM
    {
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [Required]
        public Role Role { get; set; } = Role.Customer; // default new users to Customer
    }
}

