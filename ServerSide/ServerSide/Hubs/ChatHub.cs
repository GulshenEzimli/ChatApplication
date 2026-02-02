using Microsoft.AspNetCore.SignalR;
using ServerSide.Data;
using ServerSide.Models;

namespace ServerSide.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessageAsync(string message, string receiverNickName)
        {
            string senderNickName = ClientStore.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)?.NickName ?? "";

            if(senderNickName is "")
            {
                await Clients.Caller.SendAsync("getErrorMessage", "Kullanıcı bulunamadı. Lütfen tekrar giriş yapın.");
                return;
            }

            if (receiverNickName == "Tümü")
            {
                await Clients.Others.SendAsync("receiveMessage", new { Message = message, Client = senderNickName });
                return;
            }

            string receiverConnectionId = ClientStore.Clients.FirstOrDefault(c => c.NickName == receiverNickName)?.ConnectionId ?? "";

            if (receiverConnectionId is "")
            {
                await Clients.Caller.SendAsync("getErrorMessage", $"Kullanıcı '{receiverNickName}' bulunamadı.");
                return;
            }

            await Clients.Client(receiverConnectionId).SendAsync("receiveMessage", new { Message = message, Client = senderNickName });
        }

        public async Task GetNickNameAsync(string nickName)
        {
            Client client = new Client()
            {
                ConnectionId = Context.ConnectionId,
                NickName = nickName
            };

            ClientStore.Clients.Add(client);
            await Clients.Others.SendAsync("clientJoined", nickName);
            await Clients.All.SendAsync("listclients", ClientStore.Clients);
        }

        public override async  Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("listclients", ClientStore.Clients); 
        }

        public async Task AddGroupAsync(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            Group group = new Group(){ GroupName = groupName }; 
            GroupStore.Groups.Add(group);
            await Clients.All.SendAsync("listgroups", GroupStore.Groups);
        }
    }
}
