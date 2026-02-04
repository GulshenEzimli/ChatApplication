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
            await Clients.All.SendAsync("listgroups", GroupStore.Groups);
        }

        public async Task AddGroupAsync(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            Group group = new Group(){ GroupName = groupName };
            var client = ClientStore.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            group.Clients.Add(client);

            GroupStore.Groups.Add(group);
            await Clients.All.SendAsync("listgroups", GroupStore.Groups);
        }

        public async Task JoinToGroupAsync(IEnumerable<string> groupNames)
        {
            var client = ClientStore.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            foreach (var groupName in groupNames)
            {
                var group = GroupStore.Groups.FirstOrDefault(g => g.GroupName == groupName);
                bool result = group.Clients.Any(c => c.ConnectionId == client.ConnectionId);
                
                if(!result)
                { 
                    group.Clients.Add(client);
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                }
            }
        }

        public async Task GetGroupClients(List<string> groupNames)
        {
            if (groupNames.Count == 1 && groupNames[0] =="-1")
            {
                await Clients.Caller.SendAsync("listclients", ClientStore.Clients);
            }

            var clients = new List<Client>();
            foreach (var groupName in groupNames)
            {
                var group = GroupStore.Groups.FirstOrDefault(g => g.GroupName == groupName);
                if (group is not null && group?.Clients.Count > 0)
                    clients.AddRange(group.Clients);
            }

            clients = clients.DistinctBy(c => c.NickName).ToList();
            if (clients.Count > 0)
            {
                await Clients.Caller.SendAsync("listclients", clients);
            }
        }

        public async Task SendMessageToGroupAsync(string message, List<string> groupNames)
        {
            string senderNickName = ClientStore.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId)?.NickName ?? "";
            if(senderNickName is "")
            {
                await Clients.Caller.SendAsync("getErrorMessage", "Kullanıcı bulunamadı. Lütfen tekrar giriş yapın.");
                return;
            }
            foreach (var groupName in groupNames)
            {
                await Clients.Group(groupName).SendAsync("receiveMessage", new { Message = message, Client = $"{groupName}:{senderNickName}" });
            }
        }
    }
}
