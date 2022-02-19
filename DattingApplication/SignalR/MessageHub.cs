using AutoMapper;
using DattingApplication.Extensions;
using DattingApplication.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace DattingApplication.SignalR
{
    public class MessageHub: Hub
    {
        private readonly IMapper _mapper;
        private readonly IMessageRepository _messageRepository;


        public MessageHub(IMessageRepository messageRepository, IMapper mapper)
        {
            this._mapper = mapper;
            this._messageRepository = messageRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var messages = await _messageRepository.GetMessageThread(Context.User.GetUserName(), otherUser);
            await Clients.Group(groupName).SendAsync("ReceivedMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string caller, string other)
        {
            var stringcompare = string.CompareOrdinal(caller, other) < 0;
            return stringcompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
