using System.ComponentModel.DataAnnotations;

namespace WalletApi.Models;

public class User
{
    public int UserId { get; set; }
    public string? LoginName { get; set; }
    public Int64 AccountNumber { get; set; }
    public Decimal Balance { get; set; }
    public DateTime RegisterDate { get; set; } 

    
}