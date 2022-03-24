using AutoMapper;
using AutoMapper.QueryableExtensions;
using DattingApplication.Entities;
using DattingApplication.Helpers;
using DattingApplication.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DattingApplication.Data
{
    public class UserRepository : IUserRepository
    {
        public readonly DataContext Context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            Context = context;
            this.mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await Context.Users.Where(u => u.UserName == username)
                                      .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                                      .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = Context.Users.AsQueryable();
                           
            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);
             
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider).AsNoTracking(), 
                                                           userParams.PageNumber,
                                                           userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await Context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName)
        {
            return await Context.Users.Include(u => u.Photos).SingleOrDefaultAsync(x => x.UserName == userName);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await Context.Users.Where(x => x.UserName == username).Select(u => u.Gender).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await Context.Users.Include(u => u.Photos).ToListAsync();
        }

        public void Update(AppUser user)
        {
            Context.Entry(user).State = EntityState.Modified;

        }
    }
}
