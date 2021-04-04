using AutoMapper;
using DattingApplication.Data;
using DattingApplication.Entities;
using DattingApplication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Authorize]
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await UserRepository.GetMemberAsync(username);
             return Ok(user);
        }
    }
}
