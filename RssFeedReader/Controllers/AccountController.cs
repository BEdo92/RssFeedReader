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
        if (await UserExistsByUsernameAsync(registerDto.Username))
        {
            return BadRequest(new { message = "Username is taken" });
        }

        if (await UserExistsByEmailAsync(registerDto.Email))
        {
            return BadRequest(new { message = "Email is already registered" });
        }

        var user = mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.Username.ToLower();
        user.Email = registerDto.Email.ToLower();

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
            x.NormalizedEmail == loginDto.Email.ToUpper());

        if (user is null || user.Email is null)
        {
            // NOTE: We don't want to help hackers by telling them if the email or password is incorrect.
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
        {
            // NOTE: We don't want to help hackers by telling them if the email or password is incorrect.
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExistsByUsernameAsync(string username)
    {
        return await userManager.Users.AnyAsync(x =>
            x.NormalizedUserName == username.ToUpper());
    }

    private async Task<bool> UserExistsByEmailAsync(string email)
    {
        return await userManager.Users.AnyAsync(x =>
            x.NormalizedEmail == email.ToUpper());
    }
}
