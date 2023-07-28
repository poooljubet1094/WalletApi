using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WalletApi.Models;
using WalletApi.Services.Abstract;
using WalletApi.Services.Concrete;
using WalletApi.ViewModels.Transactions;

namespace UnitTesting;

public class BalanceTransactionTest
{
    private readonly IConfiguration _config;
    private readonly BalanceTransactionService _balanceTransactionService;
    private readonly UserRegistrationService _userRegistrationService;

    private User _userFromTransfer;
    private User _userToTransfer;
    private User _userWith0Balance;

    private List<long> _accountNumberWithZeroBalance;
    private List<long> _accountNumberToTransfer;

    public BalanceTransactionTest()
	{
        var myConfiguration = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=WalletApi;Integrated Security=false;Persist Security Info=false;User ID=sa;Password=P@ssw0rd1;"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

        _userRegistrationService = new UserRegistrationService(_config);
        _balanceTransactionService = new BalanceTransactionService(_config, _userRegistrationService);

        _accountNumberWithZeroBalance = new List<long>()
        {
            202300000001043,
            202300000001044,
            202300000001045,
            202300000001046,
            202300000001047,
            202300000001048
        };

        _accountNumberToTransfer = new List<long>()
        {
            202300000001049,
            202300000001050,
            202300000001053,
            202300000001051,
            202300000001055,
            202300000001057
        };
    }

    [SetUp]
    public void Setup()
    {
        _userFromTransfer = _userRegistrationService.GetUser(202300000000028).Result;
        _userToTransfer = _userRegistrationService.GetUser(202300000000029).Result;
        _userWith0Balance = _userRegistrationService.GetUser(202300000000030).Result;
    }

    [Test, Order(1)]
    public async Task ValidateBalanceTransactionTest()
    {
        var transferBalanceViewModel = new TransferBalanceViewModel()
        {
            AccountNumber = _userFromTransfer.AccountNumber,
            toAccountNumber = _userToTransfer.AccountNumber,
            Amount = 2000
        };

        var errorList = await _balanceTransactionService.ValidateBalanceTrasaction(transferBalanceViewModel);

        Assert.That(errorList, Is.InstanceOf<IEnumerable<string>>());
        Assert.That(errorList.Count, Is.EqualTo(0));
    }

    [Test, Order(2)]
    public async Task ValidateBalanceTransaction_AccountDoesntExist()
    {
        var transferBalanceViewModel = new TransferBalanceViewModel()
        {
            AccountNumber = 123,
            toAccountNumber = 123,
            Amount = 2000
        };

        var errorList = await _balanceTransactionService.ValidateBalanceTrasaction(transferBalanceViewModel);

        Assert.That(errorList, Is.InstanceOf<IEnumerable<string>>());
        Assert.That(errorList.Count, Is.EqualTo(3));
        Assert.That(errorList.Any(x => x == "Account Number not found!"));
        Assert.That(errorList.Any(x => x == "To Account Number not found!"));
        Assert.That(errorList.Any(x => x == "You cannot transfer balance to the same account!"));
    }

    [Test, Order(3)]
    public async Task ProcessDepositTest()
    {
        var transferViewModel = new TransactionBaseViewModel()
        {
            AccountNumber = _userFromTransfer.AccountNumber,
            Amount = 6000
        };

        var transactionHistory = await _balanceTransactionService.ProcessDeposit(transferViewModel);

        Assert.That(transactionHistory, Is.InstanceOf<TransactionHistory>());
        Assert.That(transactionHistory.FromAccountNumber, Is.EqualTo(transferViewModel.AccountNumber));
        Assert.That(transactionHistory.Amount, Is.EqualTo(transferViewModel.Amount));
        Assert.That(transactionHistory.EndBalance, Is.EqualTo(_userFromTransfer.Balance + transferViewModel.Amount));
    }

    [Test, Order(4)]
    public async Task ProcessWithdrawTest()
    {
        var transferViewModel = new TransactionBaseViewModel()
        {
            AccountNumber = _userFromTransfer.AccountNumber,
            Amount = 3000
        };

        var transactionHistory = await _balanceTransactionService.ProcessWithdraw(transferViewModel);

        Assert.That(transactionHistory, Is.InstanceOf<TransactionHistory>());
        Assert.That(transactionHistory.FromAccountNumber, Is.EqualTo(transferViewModel.AccountNumber));
        Assert.That(transactionHistory.Amount, Is.EqualTo(transferViewModel.Amount));
        Assert.That(transactionHistory.EndBalance, Is.EqualTo(_userFromTransfer.Balance - transferViewModel.Amount));
    }

    [Test, Order(5)]
    public void ProcessWithdrawTest_WithZeroBalance()
    {
        var transferViewModel = new TransactionBaseViewModel()
        {
            AccountNumber = _userWith0Balance.AccountNumber,
            Amount = 3000
        };

        var ex = Assert.ThrowsAsync<SqlException>(async () =>
        {
            var transactionHistory = await _balanceTransactionService.ProcessWithdraw(transferViewModel);
        });

        Assert.That(ex.Message, Is.EqualTo("You dont have balance to proceed in this request."));
    }

    [Test, Order(6)]
    public async Task ProcessTransferBalanceTest()
    {
        var transferViewModel = new TransferBalanceViewModel()
        {
            AccountNumber = _userFromTransfer.AccountNumber,
            toAccountNumber = _userToTransfer.AccountNumber,
            Amount = 3000,
        };

        var transactionHistory = await _balanceTransactionService.ProcessTranferBalacne(transferViewModel);

        Assert.That(transactionHistory, Is.InstanceOf<TransactionHistory>());
        Assert.That(transactionHistory.FromAccountNumber, Is.EqualTo(transferViewModel.AccountNumber));
        Assert.That(transactionHistory.ToAccountNumber, Is.EqualTo(transferViewModel.toAccountNumber));
        Assert.That(transactionHistory.Amount, Is.EqualTo(transferViewModel.Amount));
        Assert.That(transactionHistory.EndBalance, Is.EqualTo(_userFromTransfer.Balance - transferViewModel.Amount));

        var previousToUserBalance = _userToTransfer.Balance;

        _userToTransfer = _userRegistrationService.GetUser(_userToTransfer.AccountNumber).Result;

        Assert.That(_userToTransfer.Balance, Is.EqualTo(previousToUserBalance + transferViewModel.Amount));
    }

    [Test, Order(7)]
    public void ProcessTransferBalanceTest_WithInsuficientBalance()
    {
        var transferViewModel = new TransferBalanceViewModel()
        {
            AccountNumber = _userWith0Balance.AccountNumber,
            toAccountNumber = _userToTransfer.AccountNumber,
            Amount = 3000,
        };

        var ex = Assert.ThrowsAsync<SqlException>(async () =>
        {
            var transactionHistory = await _balanceTransactionService.ProcessTranferBalacne(transferViewModel);
        });

        Assert.That(ex.Message, Is.EqualTo("You dont have balance to proceed in this request."));
    }

    [Test, Order(8)]
    public void ProcessDepositTest_Concurrency()
    {
        var transferViewModels = new List<TransactionBaseViewModel>();

        foreach (var accountNumber in _accountNumberWithZeroBalance)
        {
            transferViewModels.Add(new TransactionBaseViewModel()
            {
                AccountNumber = accountNumber,
                Amount = 6000
            });
        }

        Parallel.ForEach(transferViewModels, (model) =>
        {
            var user = _userRegistrationService.GetUser(model.AccountNumber).Result;

            var transactionHistory = _balanceTransactionService.ProcessDeposit(model).Result;
            Assert.That(transactionHistory, Is.InstanceOf<TransactionHistory>());
            Assert.That(transactionHistory.FromAccountNumber, Is.EqualTo(model.AccountNumber));
            Assert.That(transactionHistory.Amount, Is.EqualTo(model.Amount));
            Assert.That(transactionHistory.EndBalance, Is.EqualTo(user.Balance + model.Amount));
        });
    }

    [Test, Order(9)]
    public void ProcessTransferBalanceTest_Concurrency()
    {
        var transferViewModels = new List<TransferBalanceViewModel>();

        for (var i = 0; i < _accountNumberWithZeroBalance.Count(); i++)
        {
            transferViewModels.Add(new TransferBalanceViewModel()
            {
                AccountNumber = _accountNumberWithZeroBalance[i],
                toAccountNumber = _accountNumberToTransfer[i],
                Amount = 5000
            });
        }

        Parallel.ForEach(transferViewModels, (model) =>
        {
            var fromUser = _userRegistrationService.GetUser(model.AccountNumber).Result;
            var toUser = _userRegistrationService.GetUser(model.toAccountNumber).Result;

            var transactionHistory = _balanceTransactionService.ProcessTranferBalacne(model).Result;
            Assert.That(transactionHistory, Is.InstanceOf<TransactionHistory>());
            Assert.That(transactionHistory.FromAccountNumber, Is.EqualTo(model.AccountNumber));
            Assert.That(transactionHistory.ToAccountNumber, Is.EqualTo(model.toAccountNumber));
            Assert.That(transactionHistory.Amount, Is.EqualTo(model.Amount));
            Assert.That(transactionHistory.EndBalance, Is.EqualTo(fromUser.Balance - model.Amount));

            var previousToUserBalance = toUser.Balance;
            toUser = _userRegistrationService.GetUser(model.toAccountNumber).Result;

            Assert.That(toUser.Balance, Is.EqualTo(previousToUserBalance + model.Amount));
        });
    }
}

