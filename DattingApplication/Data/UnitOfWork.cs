using AutoMapper;
using DattingApplication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DattingApplication.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        public DataContext _context { get; set; }
        public IMapper _mapper { get; set; }
        public UnitOfWork(DataContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public IUserRepository UserRepository => new UserRepository(_context, _mapper);

        public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

        public ILikesRepository LikesRepository => new LikesRepository(_context);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChange()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}
