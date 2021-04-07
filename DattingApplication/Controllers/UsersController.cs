using AutoMapper;
using DattingApplication.Data;
using DattingApplication.DTOs;
using DattingApplication.Entities;
using DattingApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DattingApplication.Controllers
{
  
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserRepository UserRepository;
        private readonly IMapper mapper;

        public UsersController(IUserRepository userREpository, IMapper mapper)
        {
            UserRepository = userREpository;
            this.mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await UserRepository.GetMembersAsync();
            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await UserRepository.GetMemberAsync(username);
             return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await this.UserRepository.GetUserByUserNameAsync(userName);
            mapper.Map(memberUpdateDto, user);
            UserRepository.Update(user);
            if (await UserRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");
        }
    }
}
