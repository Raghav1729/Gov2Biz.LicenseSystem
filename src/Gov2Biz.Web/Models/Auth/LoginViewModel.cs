using System.ComponentModel.DataAnnotations;

namespace Gov2Biz.Web.Models.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    [Required(ErrorMessage = "Tenant domain is required")]
    [Display(Name = "Tenant Domain")]
    public string TenantDomain { get; set; } = "default";

    public string? ReturnUrl { get; set; }
}
