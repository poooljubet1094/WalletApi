using WalletApi.Services.Concrete;
using Microsoft.Extensions.Configuration;
using WalletApi.ViewModels.Users;
using System.Data.SqlClient;
using WalletApi.Models;

namespace UnitTesting;

public class UserRegistrationTest
{
    private readonly IConfiguration _config;
    private readonly UserRegistrationService _userRegistrationService;
    private readonly string _existingUsername;

    private User? _existingUser;

    public UserRegistrationTest()
    {
        var myConfiguration = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=WalletApi;Integrated Security=false;Persist Security Info=false;User ID=sa;Password=P@ssw0rd1;"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

        _userRegistrationService = new UserRegistrationService(_config);
        _existingUsername = Faker.Internet.UserName();
    }

    [SetUp]
    public void Setup()
    {
        
    }

    [Test, Order(1)]
    public async Task AddUserTest()
    {
        var registerViewModel = new RegisterViewModel()
        {
            LoginName = _existingUsername,
            Password = Faker.Lorem.GetFirstWord()
        };

        var user = await _userRegistrationService.AddUser(registerViewModel);

        _existingUser = user;

        var currentYear = (long)DateTime.UtcNow.Year;
        var accountNumber = (currentYear * 100000000000) + user.UserId;

        Assert.That(user.UserId, Is.GreaterThan(0));
        Assert.That(user.LoginName, Is.EqualTo(registerViewModel.LoginName));
        Assert.That(user.Balance, Is.EqualTo(0));
        Assert.That(user.AccountNumber, Is.EqualTo(accountNumber));
        Assert.That(user.RegisterDate.Year, Is.EqualTo(DateTime.UtcNow.Year));
        Assert.That(user.RegisterDate.Month, Is.EqualTo(DateTime.UtcNow.Month));
        Assert.That(user.RegisterDate.Day, Is.EqualTo(DateTime.UtcNow.Day));
    }

    [Test, Order(2)]
    public void AddDuplicateUserTest()
    {
        var registerViewModel = new RegisterViewModel()
        {
            LoginName = _existingUsername,
            Password = Faker.Lorem.GetFirstWord()
        };

        var ex = Assert.ThrowsAsync<SqlException>(async () =>
        {
            var user = await _userRegistrationService.AddUser(registerViewModel);
        });

        Assert.That(ex.Number, Is.EqualTo(2627));
    }

    [Test, Order(3)]
    public async Task GetUserTest()
    {
        if (_existingUser == null)
        {
            Assert.Fail();
        }

        var user = await _userRegistrationService.GetUser(_existingUser.AccountNumber);

        Assert.That(user, Is.InstanceOf<User>());
        Assert.That(user.UserId, Is.EqualTo(_existingUser.UserId));
        Assert.That(user.AccountNumber, Is.EqualTo(_existingUser.AccountNumber));
        Assert.That(user.LoginName, Is.EqualTo(_existingUser.LoginName));
        Assert.That(user.Balance, Is.EqualTo(_existingUser.Balance));
        Assert.That(user.RegisterDate, Is.EqualTo(_existingUser.RegisterDate));
    }
}