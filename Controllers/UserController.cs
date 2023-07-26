using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using WalletApi.Models;
using WalletApi.Services.Abstract;
using WalletApi.Services.Concrete;
using WalletApi.ViewModels.Users;

namespace WalletApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserRegistrationService _userRegistrationService;
    public UserController(ILogger<UserController> logger, IUserRegistrationService userRegistrationService)
    {
        _logger = logger;
        _userRegistrationService = userRegistrationService;
    }

    [HttpPost(Name = "Register")]
    public async Task<ActionResult<User>> Register(RegisterViewModel register)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var user = await _userRegistrationService.AddUser(register);
            return Ok(user);
        }
        catch(SqlException ex)
        {
            if (ex.Number == 2627)
            {
                return BadRequest("User already exists.");
            }
        }

        return StatusCode(500);
    }
}