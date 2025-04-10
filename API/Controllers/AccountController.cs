using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
{

    [HttpPost("register")] //account/register
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
    {
        if(await UserExists(registerDTO.Username)) return BadRequest("Username is taken");

        var user = mapper.Map<AppUser>(registerDTO);

        user.UserName = registerDTO.Username.ToLower();

        var result = await userManager.CreateAsync(user, registerDTO.Password);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return new UserDTO
        {
            Username = user.UserName,
            Token = await tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };

    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        var user = await userManager.Users
            .Include(p => p.Photos)
             .FirstOrDefaultAsync(x => 
                  x.NormalizedUserName == loginDTO.Username.ToUpper());

        if(user == null || user.UserName == null) return Unauthorized("Invalid username");

        var result = await userManager.CheckPasswordAsync(user, loginDTO.Password);

        if(!result) return Unauthorized();

  
        return new UserDTO
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Token = await tokenService.CreateToken(user),
            Gender = user.Gender,
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };

    }


    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
    }

}
