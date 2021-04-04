using DattingApplication.Data;
using DattingApplication.DTOs;
using DattingApplication.Entities;
using DattingApplication.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DattingApplication.Controllers
{
    public class AccountController : BaseController
    {
        private readonly DataContext Context;

        public readonly ITokenService TokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            Context = context;
            TokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.UserName)) return BadRequest("UserName is taken");
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            return new UserDto
            {
                UserName = user.UserName,
                Token = TokenService.CreateToken(user)
            };
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<UserDto>> Login(DtoLogin loginDto)
        {
            var user = await Context.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.UserName);
            if (user == null) return Unauthorized("Invalid userName");
            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (user.PasswordHash[i] != computedHash[i]) return Unauthorized("Password incorrect");
            }

            return new UserDto
            {
                UserName = user.UserName,
                Token = TokenService.CreateToken(user)
            };
        }
        
        private async Task<bool> UserExist(string userName)
        {
            return await Context.Users.AnyAsync(u => u.UserName.Equals(userName.ToLower()));
        }
    }
}
