using ServerSide.Models;

namespace ServerSide.Data
{
    public static class ClientStore
    {
        public static List<Client> Clients { get; } = new List<Client>();
    }
}
