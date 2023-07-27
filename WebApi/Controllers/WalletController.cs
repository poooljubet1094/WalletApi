using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using WalletApi.Models;
using WalletApi.Services.Abstract;
using WalletApi.ViewModels.Transactions;

namespace WalletApi.Controllers;

[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IBalanceTransactionService _balanceTransactionService;

    public WalletController(ILogger<UserController> logger, IBalanceTransactionService balanceTransactionService)
    {
        _logger = logger;
        _balanceTransactionService = balanceTransactionService;
    }

    [HttpPost]
    [Route("Deposit")]
    public async Task<ActionResult<TransactionHistory>> Deposit(TransactionBaseViewModel transaction)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validationError = await _balanceTransactionService.ValidateBalanceTrasaction(transaction, TransactionType.Deposit);

        if (validationError.Count() > 0)
        {
            return BadRequest(validationError);
        }

        try
        {
            var transactionHistory = await _balanceTransactionService.ProcessDeposit(transaction);
            return Ok(transactionHistory);
        }
        catch(SqlException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("Withdraw")]
    public async Task<ActionResult<TransactionHistory>> Withdraw(TransactionBaseViewModel transaction)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validationError = await _balanceTransactionService.ValidateBalanceTrasaction(transaction, TransactionType.Withdraw);

        if (validationError.Count() > 0)
        {
            return BadRequest(validationError);
        }
        try
        {
            var transactionHistory = await _balanceTransactionService.ProcessWithdraw(transaction);
            return Ok(transactionHistory);
        }
        catch(SqlException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("BalanceTransfer")]
    public async Task<ActionResult<TransactionHistory>> BalanceTransfer(TransferBalanceViewModel transaction)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validationError = await _balanceTransactionService.ValidateBalanceTrasaction(transaction);

        if (validationError.Count() > 0)
        {
            return BadRequest(validationError);
        }

        try
        {
            var transactionHistory = await _balanceTransactionService.ProcessTranferBalacne(transaction);
            return Ok(transactionHistory);
        }
        catch(SqlException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}