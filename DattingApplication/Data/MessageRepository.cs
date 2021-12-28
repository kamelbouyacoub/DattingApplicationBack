using AutoMapper;
using AutoMapper.QueryableExtensions;
using DattingApplication.DTOs;
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
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context ,IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }
        public void AddMessage(Message message)
        {
            this._context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this._context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this._context.Messages.Include(m => m.Sender).Include(m => m.Recipient).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
                                   
            var test = query.ToList();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.UserName && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.UserName && u.SenderDeleted== false),
                _ => query.Where(u => u.Recipient.UserName == messageParams.UserName && u.DateRead == null && u.RecipientDeleted == false)
            };

            var message = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
      
                var count = message.Count();
                return await PagedList<MessageDto>.CreateAsync(message, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUsername)
        {
            var messages = await _context.Messages
                                                    .Include(u => u.Sender).ThenInclude(u => u.Photos)
                                                    .Include(u => u.Recipient).ThenInclude(u => u.Photos).
                                                     Where(m => m.Recipient.UserName == currentUserName && m.RecipientDeleted == false
                                                         && m.Sender.UserName == recipientUsername
                                                         || m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUserName && m.SenderDeleted == false
                                                        )
                                                        .OrderBy(m => m.MessageSent)
                                                        .ToListAsync();
            var unredMessages = messages.Where(m => m.DateRead == null && m.Recipient.UserName == currentUserName).ToList();

            if (unredMessages.Any())
            {
                foreach(var message in unredMessages)
                {
                    message.DateRead = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map <IEnumerable<MessageDto>>(messages);

        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
 
    }
}
