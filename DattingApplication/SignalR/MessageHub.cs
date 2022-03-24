using AutoMapper;
using DattingApplication.DTOs;
using DattingApplication.Entities;
using DattingApplication.Extensions;
using DattingApplication.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DattingApplication.SignalR
{
    public class MessageHub: Hub
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
 
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;


        public MessageHub(IUnitOfWork unitOfWork, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker,  IMapper mapper)
        {
            this._mapper = mapper;
            this._unitOfWork = unitOfWork;
    
            this._presenceHub = presenceHub;
            this._tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group =  await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUserName(), otherUser);
            if (_unitOfWork.HasChange()) await _unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceivedMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUserName();
            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Not found user");
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUserName = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if(group.Connections.Any(x => x.UserName == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connection = await _tracker.GetConnectionForUser(recipient.UserName);
                if(connection != null)
                {
                    await _presenceHub.Clients.Clients(connection).SendAsync("NewMessageReceived", new { 
                        userName = sender.UserName, knowAs = sender.KnownAs
                    });
                }
            }

            _unitOfWork.MessageRepository.AddMessage(message);


            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
                

        }
      
        private async Task<Group> AddToGroup(string groupName)
        {
            var group =await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if(group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);
            if(await _unitOfWork.Complete()) return group;
            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);
            if( await _unitOfWork.Complete()) return group;
            throw new HubException("Failed to remove from group");

        }


        private string GetGroupName(string caller, string other)
        {
            var stringcompare = string.CompareOrdinal(caller, other) < 0;
            return stringcompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
