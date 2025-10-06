using System.ComponentModel.DataAnnotations;
using EasyGames.Models; // Role enum

namespace EasyGames.ViewModels
{
    // edit form keeps simple, lets owner optionally change password
    public class AdminUserEditVM
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [Required]
        public Role Role { get; set; } = Role.Customer;

        [DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; } // leave blank to keep current // see Notepad 
    }
}
