using System.Data.SqlClient;
using WalletApi.Models;
using WalletApi.Services.Abstract;
using WalletApi.ViewModels.Users;

namespace WalletApi.Services.Concrete;

public class UserRegistrationService : IUserRegistrationService
{
    private readonly IConfiguration _config;

    private readonly string _connString;

    public UserRegistrationService(IConfiguration config)
    {
        _config = config;
        _connString = _config["ConnectionStrings:DefaultConnection"] ?? "";
    }

    public async Task<User> GetUser(long accountNumber)
    {
        using (var conn = new SqlConnection(_connString))
        {
            SqlCommand cmd = new SqlCommand("dbo.GetUserByAccountNumber", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@AccountNumber", accountNumber));
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    return new User()
                    {
                        UserId = (int)reader["UserId"],
                        LoginName = reader["LoginName"].ToString(),
                        Balance = (decimal)reader["Balance"],
                        RegisterDate = (DateTime)reader["RegisterDate"],
                        AccountNumber = (Int64)reader["AccountNumber"]
                    };
                }
            }
        }

        throw new KeyNotFoundException("The account number doesn't exists.");
    }

    public async Task<User> AddUser(RegisterViewModel register)
    {
        var user = new User();

        using (var conn = new SqlConnection(_connString))
        {
            SqlCommand cmd = new SqlCommand("dbo.RegisterUser", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@LoginName", register.LoginName));
            cmd.Parameters.Add(new SqlParameter("@Password", register.Password));
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    user.UserId = (int)reader["UserId"];
                    user.LoginName = reader["LoginName"].ToString();
                    user.Balance = (decimal)reader["Balance"];
                    user.RegisterDate = (DateTime)reader["RegisterDate"];
                    user.AccountNumber = (Int64)reader["AccountNumber"];
                }
            }
        }

        return user;
    }
}