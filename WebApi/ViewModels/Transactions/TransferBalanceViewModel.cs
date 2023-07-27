using System.ComponentModel.DataAnnotations;
using WalletApi.Models;
using WalletApi.ViewModels.ValidationAttributes;

namespace WalletApi.ViewModels.Transactions;

public class TransferBalanceViewModel : TransactionBaseViewModel
{
    [Required]
    public Int64 toAccountNumber { get; set; }
}