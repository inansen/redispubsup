using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Redis.Consumer.Hubs
{
    public class RedisMessageHub: Hub
    {

        public async Task SendMessage(long time, string message)
        {
            var date = DateTime.FromBinary(time);
            await Clients.All.SendAsync("ReceiveMessage", date, message);
        }
    }
}
