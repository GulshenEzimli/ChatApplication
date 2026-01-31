using Microsoft.AspNetCore.SignalR;
using ServerSide.Data;
using ServerSide.Models;

namespace ServerSide.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessageAsync(string message, string otherClientNickName)
        {
            string connectionId = ClientStore.Clients.FirstOrDefault(c => c.NickName == otherClientNickName)?.ConnectionId ?? "";

            if (connectionId is "")
            {
                await Clients.Caller.SendAsync("getErrorMessage", $"Kullanıcı '{otherClientNickName}' bulunamadı.");
                return;
            }

            await Clients.Client(connectionId).SendAsync("receiveMessage", new { Message = message, Client = otherClientNickName });
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
    }
}
