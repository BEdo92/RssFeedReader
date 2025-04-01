using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RssFeedReader.DTOs;
using RssFeedReader.Interfaces;
using RssFeedReader.Models;

namespace RssFeedReader.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> RegisterAsync(RegisterDto registerDto)
    {
        if (await UserExistsAsync(registerDto.Username))
        {
            return BadRequest(new { message = "Username is taken" });
        }

        var user = mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.Username.ToLower();

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var userDto = new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };

        return userDto;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x =>
        x.NormalizedUserName == loginDto.Username.ToUpper());

        if (user is null || user.UserName is null)
        {
            // NOTE: We don't want to help hackers by telling them if the username or password is incorrect.
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
        {
            // NOTE: We don't want to help hackers by telling them if the username or password is incorrect.
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExistsAsync(string username)
    {
        return await userManager.Users.AnyAsync(x =>
            x.NormalizedUserName == username.ToUpper());
    }
}
