using System.Data.SqlClient;
using WalletApi.Models;
using WalletApi.Services.Abstract;
using WalletApi.ViewModels.Transactions;

namespace WalletApi.Services.Concrete;

public class BalanceTransactionService : IBalanceTransactionService
{
    private readonly IConfiguration _config;
    private readonly string _connString;
    private readonly IUserRegistrationService _userRegistrationService;

    public BalanceTransactionService(IConfiguration config, IUserRegistrationService userRegistrationService)
    {
        _config = config;
        _connString = _config["ConnectionStrings:DefaultConnection"] ?? "";
        _userRegistrationService = userRegistrationService;
    }

    public async Task<IEnumerable<string>> ValidateBalanceTrasaction(TransferBalanceViewModel transaction)
    {
        var result = new List<string>();

        if (!await IsAccountExist(transaction.AccountNumber))
        {
            result.Add("Account Number not found!");
        }

        if (!await IsAccountExist(transaction.toAccountNumber))
        {
            result.Add("To Account Number not found!");
        }

        if (transaction.AccountNumber == transaction.toAccountNumber)
        {
            result.Add("You cannot transfer balance to the same account!");
        }

        var endBalance = await ComputeEndBalance(transaction.AccountNumber, transaction.Amount, TransactionType.BalanceTransfer);;

        if (endBalance < 0)
        {
            result.Add("You do not have enough balance to process this transaction.");
        }

        return result;
    }

    public async Task<IEnumerable<string>> ValidateBalanceTrasaction(TransactionBaseViewModel transaction, TransactionType type)
    {
        var result = new List<string>();

        if (!await IsAccountExist(transaction.AccountNumber))
        {
            result.Add("Account Number not found!");
        }

        var endBalance = await ComputeEndBalance(transaction.AccountNumber, transaction.Amount, type);

        if (endBalance < 0)
        {
            result.Add("You do not have enough balance to process this transaction.");
        }

        return result;
    }

    private async Task<bool> IsAccountExist(Int64 accountNumber)
    {
        using (var conn = new SqlConnection(_connString))
        {
            SqlCommand cmd = new SqlCommand("dbo.AccountNumberExist", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@AccountNumber", accountNumber));
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    return true;
                }
            }

        }

        return false;
    }

    public async Task<TransactionHistory> ProcessDeposit(TransactionBaseViewModel transaction)
    {
        var transactionViewModel = new TransactionViewModel()
        {
            Amount = transaction.Amount,
            AccountNumber = transaction.AccountNumber,
            toAccountNumber = transaction.AccountNumber,
            TransactionDate = DateTime.Now,
            EndBalance = await ComputeEndBalance(transaction.AccountNumber, transaction.Amount, TransactionType.Deposit),
            TransactionType = TransactionType.Deposit
        };

        return await ProcessWalletTransaction(transactionViewModel);
    }

    public async Task<TransactionHistory> ProcessWithdraw(TransactionBaseViewModel transaction)
    {
        var transactionViewModel = new TransactionViewModel()
        {
            Amount = transaction.Amount,
            AccountNumber = transaction.AccountNumber,
            toAccountNumber = transaction.AccountNumber,
            TransactionDate = DateTime.Now,
            EndBalance = await ComputeEndBalance(transaction.AccountNumber, transaction.Amount, TransactionType.Withdraw),
            TransactionType = TransactionType.Withdraw
        };

        return await ProcessWalletTransaction(transactionViewModel);
    }

    public async Task<TransactionHistory> ProcessTranferBalacne(TransferBalanceViewModel transaction)
    {
        var transactionViewModel = new TransactionViewModel()
        {
            Amount = transaction.Amount,
            AccountNumber = transaction.AccountNumber,
            toAccountNumber = transaction.toAccountNumber,
            TransactionDate = DateTime.Now,
            EndBalance = await ComputeEndBalance(transaction.AccountNumber, transaction.Amount, TransactionType.BalanceTransfer),
            TransactionType = TransactionType.BalanceTransfer
        };

        return await ProcessWalletTransaction(transactionViewModel);
    }

    private async Task<decimal> ComputeEndBalance(Int64 accountNumber, decimal amount, TransactionType type)
    {
        var account = await _userRegistrationService.GetUser(accountNumber);

        var transactionAmount = (type == TransactionType.Deposit) ? amount : -amount;
        return account.Balance + transactionAmount;
    }

    private async Task<TransactionHistory> ProcessWalletTransaction(TransactionViewModel transaction)
    {
        var transactionHistory = new TransactionHistory();

        using (var conn = new SqlConnection(_connString))
        {
            SqlCommand cmd = new SqlCommand("dbo.ProcessWalletTransaction", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Amount", transaction.Amount));
            cmd.Parameters.Add(new SqlParameter("@FromAccountNumber", transaction.AccountNumber));
            cmd.Parameters.Add(new SqlParameter("@ToAccountNumber", transaction.toAccountNumber));
            cmd.Parameters.Add(new SqlParameter("@TransactionDate", transaction.TransactionDate));
            cmd.Parameters.Add(new SqlParameter("@EndBalance", transaction.EndBalance));
            cmd.Parameters.Add(new SqlParameter("@TransactionType", transaction.TransactionType));
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    transactionHistory.TransactionId = (int)reader["TransactionId"];
                    transactionHistory.Amount = (decimal)reader["Amount"];
                    transactionHistory.FromAccountNumber = (Int64)reader["FromAccountNumber"];
                    transactionHistory.ToAccountNumber = (Int64)reader["ToAccountNumber"];
                    transactionHistory.TransactionDate = (DateTime)reader["TransactionDate"];
                    transactionHistory.EndBalance = (decimal)reader["EndBalance"];
                    transactionHistory.TransactionType = (TransactionType)reader["TransactionType"];
                }
            }
        }

        return transactionHistory;
    }
}