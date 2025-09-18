using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using System.Configuration;

namespace MZDNETWORK.Hubs
{
    public class OnlineUsersHub : Hub
    {
        private static ConcurrentDictionary<string, string> connectedUsers = new ConcurrentDictionary<string, string>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override Task OnConnected()
        {
            try
            {
                var user = Context.User?.Identity?.Name ?? "Anonymous";
                var connectionId = Context.ConnectionId;

                connectedUsers.TryAdd(connectionId, user);

                if (ConfigurationManager.AppSettings["LogSignalRErrors"] != "false")
                {
                    Logger.Info($"{user} kullanýcýsý baðlandý. Baðlantý ID: {connectionId}");
                }

                Clients.All.updateOnlineUsers(connectedUsers.Count);
                Clients.All.updateUserList(connectedUsers.Values);

                return base.OnConnected();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in OnConnected for connection {Context.ConnectionId}");
                return base.OnConnected();
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var connectionId = Context.ConnectionId;

                if (connectedUsers.TryRemove(connectionId, out var user))
                {
                    if (ConfigurationManager.AppSettings["LogSignalRErrors"] != "false")
                    {
                        Logger.Info($"{user} kullanýcýsý ayrýldý. Baðlantý ID: {connectionId}");
                    }

                    Clients.All.updateOnlineUsers(connectedUsers.Count);
                    Clients.All.updateUserList(connectedUsers.Values);
                }

                return base.OnDisconnected(stopCalled);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error in OnDisconnected for connection {Context.ConnectionId}");
                return base.OnDisconnected(stopCalled);
            }
        }

        public void SendTestMessage(string message)
        {
            try
            {
                Clients.All.testMessage(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in SendTestMessage");
            }
        }

        // Çevrimiçi kullanýcý sayýsýný döndüren statik metot
        public static int GetOnlineUsersCount()
        {
            return connectedUsers.Count;
        }

        // Cleanup method for periodic maintenance
        public static void CleanupStaleConnections()
        {
            // This method can be called periodically to clean up any stale connections
            // For now, we'll just log the current count
            Logger.Debug($"Current online users count: {connectedUsers.Count}");
        }
    }
}