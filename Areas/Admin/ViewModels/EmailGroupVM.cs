namespace EasyGames.Areas.Admin.ViewModels;
using EasyGames.Models;
using System.ComponentModel.DataAnnotations;

// Veasna File
public class EmailGroupVM
{
    [Required] public string Subject { get; set; } = "";
    [Required] public string Message { get; set; } = "";
    public bool SendToAll { get; set; }
    public UserTier? TargetTier { get; set; }
}