using System;
using WalletApi.Models;
using WalletApi.Services.Abstract;
using WalletApi.ViewModels.Users;

namespace UnitTesting.Fakers
{
	public class UserRegistrationServiceFaker : IUserRegistrationService
	{
        public List<User> Users;

		public UserRegistrationServiceFaker()
		{
            Users = new List<User>()
            {
                new User()
                {
                    UserId = 1,
                    LoginName = "TEST001",
                    AccountNumber = 202300000001,
                    Balance = 10000,
                    RegisterDate = DateTime.Now
                },
                new User()
                {
                    UserId = 2,
                    LoginName = "TEST002",
                    AccountNumber = 202300000002,
                    Balance = 10000,
                    RegisterDate = DateTime.Now
                },
                new User()
                {
                    UserId = 3,
                    LoginName = "TEST001",
                    AccountNumber = 202300000003,
                    Balance = 10000,
                    RegisterDate = DateTime.Now
                }
            };
		}

        public Task<User> AddUser(RegisterViewModel user)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUser(long accountNumber)
        {
            throw new NotImplementedException();
        }
    }
}

