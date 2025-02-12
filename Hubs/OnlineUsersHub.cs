using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MZDNETWORK.Hubs
{
    public class OnlineUsersHub : Hub
    {
        private static int onlineUsersCount = 0;

        public override Task OnConnected()
        {
            onlineUsersCount++;
            Clients.All.updateOnlineUsers(onlineUsersCount);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            onlineUsersCount--;
            Clients.All.updateOnlineUsers(onlineUsersCount);
            return base.OnDisconnected(stopCalled);
        }

        public void SendTestMessage(string message)
        {
            Clients.All.testMessage(message);
        }
    }

}