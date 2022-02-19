using DattingApplication.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DattingApplication.SignalR
{
    [Authorize]
    public class PrensenceHub: Hub
    {
        private readonly PresenceTracker _tracker;
        public PrensenceHub(PresenceTracker _tracker)
        {
            this._tracker = _tracker;
        }
        public override async Task OnConnectedAsync()
        {
            await _tracker.UserConnected(Context.User.GetUserName(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOnLine", Context.User.GetUserName());

            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _tracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());
            await base.OnDisconnectedAsync(exception);
        }
    }
}
