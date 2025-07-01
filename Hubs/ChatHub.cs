using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using MZDNETWORK.Attributes;

[DynamicAuthorize(Permission = "Operational.Chat", Action = "Create")]
public class ChatHub : Hub
{
    private static HashSet<string> ConnectedUsers = new HashSet<string>();

    public void Send(string user, string message)
    {
        // Yetki kontrolü: Operational.Chat.Create veya Manage
        string username = Context.User?.Identity?.Name ?? "";
        bool canCreate = MZDNETWORK.Helpers.DynamicPermissionHelper.CheckPermission(username, "Operational.Chat", "Create");
        bool canManage = MZDNETWORK.Helpers.DynamicPermissionHelper.CheckPermission(username, "Operational.Chat", "Manage");

        if (!canCreate && !canManage)
        {
            Clients.Caller.error("Bu işlemi yapmak için yetkiniz yok.");
            return;
        }

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
