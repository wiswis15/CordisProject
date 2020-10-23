using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication1.hubs
{

    public class ChatHub : Hub
    {
        public static List<CurrentVariableValue> userMessages = new List<CurrentVariableValue>();

        public async Task SendMessage()
        {
            AddNewDummyValue();

            await Clients.All.SendAsync("ReceiveMessage", userMessages);
        }

        public override async Task OnConnectedAsync()
        {
            AddNewDummyValue();
            AddNewDummyValue();
            await Clients.All.SendAsync("ReceiveMessage", userMessages);
        }

        private void AddNewDummyValue()
        {
            int value = new Random().Next();
            var newvalue = new CurrentVariableValue() { CurrentValue = ""+ value, LastUpdated = DateTime.Now, VariablePath = "dummy/dummy"+ value };

            userMessages.Add(newvalue);
        }
    }
}
