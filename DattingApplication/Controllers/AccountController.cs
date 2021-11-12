using AutoMapper;
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
        public readonly IUserRepository UserRepository;
        public readonly IMapper _mapper;

        public AccountController(DataContext context, ITokenService tokenService, IUserRepository userRepository, IMapper mapper)
        {
            Context = context;
            TokenService = tokenService;
            UserRepository = userRepository;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.UserName)) return BadRequest("UserName is taken");
            var user = _mapper.Map<AppUser>(registerDto);
            using var hmac = new HMACSHA512();

            user.UserName = registerDto.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            return new UserDto
            {
                UserName = user.UserName,
                Token = TokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<UserDto>> Login(DtoLogin loginDto)
        {
            var user = await UserRepository.GetUserByUserNameAsync(loginDto.UserName);
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
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                Token = TokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }
        
        private async Task<bool> UserExist(string userName)
        {
            return await Context.Users.AnyAsync(u => u.UserName.Equals(userName.ToLower()));
        }
    }
}
