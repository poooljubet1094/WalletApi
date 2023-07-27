using System.ComponentModel.DataAnnotations;
using WalletApi.ViewModels.ValidationAttributes;

namespace WalletApi.ViewModels.Users;

public class RegisterViewModel
{
    [Required]
    public required string LoginName { get; set; }
    [Required]
    public required string Password { get; set; }
}