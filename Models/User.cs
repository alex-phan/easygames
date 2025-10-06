// user model + role see Notepad n1
using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public enum Role { Owner = 0, Customer = 1 } // two roles only see Notepad n1.a

    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(80)] public string Name { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string PasswordHash { get; set; } = ""; // hashed pass see Notepad n1.b
        public Role Role { get; set; } = Role.Customer;
        public string? Address { get; set; }
    }
}
