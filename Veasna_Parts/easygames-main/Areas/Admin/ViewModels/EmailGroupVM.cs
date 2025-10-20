namespace EasyGames.Areas.Admin.ViewModels;

using System.ComponentModel.DataAnnotations;

public enum CustomerTier { Bronze, Silver, Gold, Platinum }

public class EmailGroupVM
{
    [Required] public string Subject { get; set; } = "";
    [Required] public string Message { get; set; } = "";
    public bool SendToAll { get; set; }
    public CustomerTier? TargetTier { get; set; }
}
