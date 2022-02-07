using AutoMapper;
using DattingApplication.Data;
using DattingApplication.DTOs;
using DattingApplication.Entities;
using DattingApplication.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DattingApplication.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<AppUser> UserManager;
        private readonly SignInManager<AppUser> SignInManager;

        public readonly ITokenService TokenService;
        public readonly IUserRepository UserRepository;
        public readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IUserRepository userRepository, IMapper mapper)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            TokenService = tokenService;
            UserRepository = userRepository;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.UserName)) return BadRequest("UserName is taken");
            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.UserName.ToLower();

            var result = await UserManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            var roleResult = await UserManager.AddToRoleAsync(user, "Member");
            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                UserName = user.UserName,
                Token = await TokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<UserDto>> Login(DtoLogin loginDto)
        {
            var user = await UserManager.Users.Include(u => u.Photos).SingleOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());
            if (user == null) return Unauthorized("Invalid userName");
            var result = await SignInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized();
            return new UserDto
            {
                UserName = user.UserName,
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                Token = await TokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        
        private async Task<bool> UserExist(string userName)
        {
            return await UserManager.Users.AnyAsync(u => u.UserName.Equals(userName.ToLower()));
        }
    }
}
