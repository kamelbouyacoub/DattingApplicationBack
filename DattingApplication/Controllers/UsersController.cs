using AutoMapper;
using DattingApplication.Data;
using DattingApplication.DTOs;
using DattingApplication.Entities;
using DattingApplication.Extensions;
using DattingApplication.Helpers;
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
        private readonly IPhotoService PhotoService;
        private readonly IMapper mapper;

        public UsersController(IUserRepository userREpository, IPhotoService photoService, IMapper mapper)
        {
            this.UserRepository = userREpository;
            this.mapper = mapper;
            this.PhotoService = photoService;
        }

 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await UserRepository.GetUserByUserNameAsync(User.GetUserName());
            userParams.CurrentUsername = User.GetUserName();
                if (string.IsNullOrEmpty(userParams.Gender))
            userParams.Gender = user.Gender == "male" ? "female" : "male";
            var users = await UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount ,users.TotalPages);
            return Ok(users);
        }

        [Authorize(Roles = "Member")]
        [HttpGet("{username}", Name ="GetUser" )]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await UserRepository.GetMemberAsync(username);
             return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var userName = User.GetUserName();
            var user = await this.UserRepository.GetUserByUserNameAsync(userName);
            mapper.Map(memberUpdateDto, user);
            UserRepository.Update(user);
            if (await UserRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await this.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            var result = await this.PhotoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);
            if (await this.UserRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new {username = user.UserName }, mapper.Map<Photo, PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }


        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> setMainPhoto(int photoId)
        {
            var user = await this.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo.IsMain) return BadRequest("This is already your main photo");
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await this.UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }
        
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await this.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            if( photo.PublicId != null)
            {
                var result = await this.PhotoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);
            if (await this.UserRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo");
        }



    }
}
