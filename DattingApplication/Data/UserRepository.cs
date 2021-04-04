using AutoMapper;
using AutoMapper.QueryableExtensions;
using DattingApplication.Entities;
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

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await Context.Users.ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                                      .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await Context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName)
        {
            return await Context.Users.Include(u => u.Photos).SingleOrDefaultAsync(x => x.UserName == userName);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await Context.Users.Include(u => u.Photos).ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await Context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            Context.Entry(user).State = EntityState.Modified;

        }
    }
}
