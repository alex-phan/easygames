using System.ComponentModel.DataAnnotations;

namespace EasyGames.ViewModels
{
    // just what the form needs 
    public class LoginVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; } // bounce back after login.
    }
}

