namespace WalletApi.Models;

public class TransactionHistory
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public Int64 FromAccountNumber { get; set; }
    public Int64 ToAccountNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal EndBalance { get; set; }
    public TransactionType TransactionType { get; set; }
}