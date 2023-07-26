using WalletApi.Models;
using WalletApi.ViewModels.Users;

namespace WalletApi.Services.Abstract;

public interface IUserRegistrationService
{
    Task<User> GetUser(Int64 accountNumber);
    Task<User> AddUser(RegisterViewModel user);
    Task<bool> UserExist(RegisterViewModel user);
}