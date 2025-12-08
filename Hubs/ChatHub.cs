using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Linq;
using MZDNETWORK.Attributes;
using MZDNETWORK.Data;
using MZDNETWORK.Models;
using System;

[DynamicAuthorize(Permission = "Operasyon.Sohbet", Action = "Create")]
public class ChatHub : Hub
{
    private static HashSet<string> ConnectedUsers = new HashSet<string>();

    // Grup bazlı mesaj gönderme
    public void SendToGroup(int groupId, string message)
    {
        string username = Context.User?.Identity?.Name ?? "";
        
        using (var _db = new MZDNETWORKContext())
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            var group = _db.ChatGroups.Find(groupId);
            if (user == null || group == null || !group.IsActive)
            {
                Clients.Caller.error("Grup veya kullanıcı bulunamadı.");
                return;
            }
            // Yetki kontrolü: AllowedRoles veya Members
            bool isAllowed = group.Members.Any(m => m.Id == user.Id) ||
                user.UserRoles.Any(ur => group.AllowedRoles.Any(r => r.Id == ur.RoleId));
            if (!isAllowed)
            {
                Clients.Caller.error("Bu gruba mesaj göndermek için yetkiniz yok.");
                return;
            }
            // Mesajı kaydet
            var chatMessage = new ChatMessage
            {
                ChatGroupId = groupId,
                UserId = user.Id,
                Content = message,
                SentAt = System.DateTime.Now,
                IsActive = true
            };
            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();
        }
        
        // SignalR grubu
        Groups.Add(Context.ConnectionId, $"chatgroup_{groupId}");
        Clients.Group($"chatgroup_{groupId}").broadcastMessage(username, message);
    }

    // Gruba katılma
    public void JoinGroup(int groupId)
    {
        Groups.Add(Context.ConnectionId, $"chatgroup_{groupId}");
    }

    // Gruptan ayrılma
    public void LeaveGroup(int groupId)
    {
        Groups.Remove(Context.ConnectionId, $"chatgroup_{groupId}");
    }

    public override System.Threading.Tasks.Task OnConnected()
    {
        try
        {
            string userName = Context.User?.Identity?.Name ?? "Anonymous";
            lock (ConnectedUsers)
            {
                ConnectedUsers.Add(userName);
            }
            Clients.All.userConnected(userName, ConnectedUsers);
            return base.OnConnected();
        }
        catch (Exception ex)
        {
            // Log the exception
            System.Diagnostics.Debug.WriteLine($"Error in OnConnected: {ex.Message}");
            return base.OnConnected();
        }
    }

    public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
    {
        try
        {
            string userName = Context.User?.Identity?.Name ?? "Anonymous";
            lock (ConnectedUsers)
            {
                ConnectedUsers.Remove(userName);
            }
            Clients.All.userDisconnected(userName, ConnectedUsers);
            return base.OnDisconnected(stopCalled);
        }
        catch (Exception ex)
        {
            // Log the exception
            System.Diagnostics.Debug.WriteLine($"Error in OnDisconnected: {ex.Message}");
            return base.OnDisconnected(stopCalled);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleanup any resources if needed
        }
        base.Dispose(disposing);
    }
}

