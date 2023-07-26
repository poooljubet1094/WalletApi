using System.ComponentModel.DataAnnotations;
using WalletApi.Models;

namespace WalletApi.ViewModels.Transactions;

public class TransactionViewModel : TransactionBaseViewModel
{
    [Required]
    public Int64 toAccountNumber { get; set; }
    [Required]
    public DateTime TransactionDate { get; set; }
    public decimal EndBalance { get; set; }
    public TransactionType TransactionType { get; set; }
}