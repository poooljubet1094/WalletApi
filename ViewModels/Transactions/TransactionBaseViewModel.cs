using System.ComponentModel.DataAnnotations;
using WalletApi.ViewModels.ValidationAttributes;
using WalletApi.Models;

namespace WalletApi.ViewModels.Transactions;

public class TransactionBaseViewModel
{
    [Required]
    [MinValue(0.01, ErrorMessage = "The amount must be greater than 0.01.")]
    public decimal Amount { get; set; }
    [Required]
    public Int64 AccountNumber { get; set; }
}