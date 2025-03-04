using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
namespace MZDNETWORK.Hubs
{
    public class OnlineUsersHub : Hub
    {
        private static ConcurrentDictionary<string, string> connectedUsers = new ConcurrentDictionary<string, string>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override Task OnConnected()
        {
            var user = Context.User.Identity.Name ?? "Anonymous";
            var connectionId = Context.ConnectionId;

            connectedUsers.TryAdd(connectionId, user);

            // Loglama
            Logger.Info($"{user} kullanýcýsý baðlandý. Baðlantý ID: {connectionId}");

            Clients.All.updateOnlineUsers(connectedUsers.Count);
            Clients.All.updateUserList(connectedUsers.Values);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionId = Context.ConnectionId;

            if (connectedUsers.TryRemove(connectionId, out var user))
            {
                // Loglama
                Logger.Info($"{user} kullanýcýsý ayrýldý. Baðlantý ID: {connectionId}");

                Clients.All.updateOnlineUsers(connectedUsers.Count);
                Clients.All.updateUserList(connectedUsers.Values);
            }

            return base.OnDisconnected(stopCalled);
        }

        public void SendTestMessage(string message)
        {
            Clients.All.testMessage(message);
        }

        // Çevrimiçi kullanýcý sayýsýný döndüren statik metot
        public static int GetOnlineUsersCount()
        {
            return connectedUsers.Count;
        }
    }
}