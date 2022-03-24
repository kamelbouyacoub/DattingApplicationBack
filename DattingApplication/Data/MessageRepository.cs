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

        public void AddGroup(Group group)
        {
            this._context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            this._context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this._context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(g => g.Connections).Where(g => g.Connections.Any(c => c.ConnectionId == connectionId)).FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this._context.Messages.Include(m => m.Sender).Include(m => m.Recipient).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent)
                                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                                .AsQueryable();
                                   
            var test = query.ToList();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername== messageParams.UserName && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUserName== messageParams.UserName && u.SenderDeleted== false),
                _ => query.Where(u => u.RecipientUsername== messageParams.UserName && u.DateRead == null && u.RecipientDeleted == false)
            };

                return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUsername)
        {
            var messages = await _context.Messages. Where(m => m.Recipient.UserName == currentUserName && m.RecipientDeleted == false
                                                         && m.Sender.UserName == recipientUsername
                                                         || m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUserName && m.SenderDeleted == false
                                                        )
                                                        .OrderBy(m => m.MessageSent)
                                                        .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                                                        .ToListAsync();

            var unredMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUserName).ToList();

            if (unredMessages.Any())
            {
                foreach(var message in unredMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return messages;

        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
 
    }
}
