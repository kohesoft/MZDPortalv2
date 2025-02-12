using Microsoft.AspNet.SignalR;
using System.Collections.Generic;


public class ChatHub : Hub
{
    private static HashSet<string> ConnectedUsers = new HashSet<string>();

    public void Send(string user, string message)
    {
        Clients.All.broadcastMessage(user, message);
    }

    public override System.Threading.Tasks.Task OnConnected()
    {
        string userName = Context.User.Identity.Name;
        ConnectedUsers.Add(userName);
        Clients.All.userConnected(userName, ConnectedUsers);
        return base.OnConnected();
    }

    public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
    {
        string userName = Context.User.Identity.Name;
        ConnectedUsers.Remove(userName);
        Clients.All.userDisconnected(userName, ConnectedUsers);
        return base.OnDisconnected(stopCalled);
    }

}
