using WalletApi.ViewModels.Transactions;
using WalletApi.Models;

namespace WalletApi.Services.Abstract;

public interface IBalanceTransactionService
{
    Task<IEnumerable<string>> ValidateBalanceTrasaction(TransferBalanceViewModel transaction);
    Task<IEnumerable<string>> ValidateBalanceTrasaction(TransactionBaseViewModel transaction, TransactionType type);
    Task<TransactionHistory> ProcessDeposit(TransactionBaseViewModel transaction);
    Task<TransactionHistory> ProcessWithdraw(TransactionBaseViewModel transaction);
    Task<TransactionHistory> ProcessTranferBalacne(TransferBalanceViewModel transaction);
}