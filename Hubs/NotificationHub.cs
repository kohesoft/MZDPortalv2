using Ganss.Xss;
using Microsoft.AspNet.SignalR;

public class NotificationHub : Hub
{
    public void SendNotification(string message)
    {
        var sanitizer = new HtmlSanitizer();
        var sanitizedMessage = sanitizer.Sanitize(message);
        Clients.All.showNotification(sanitizedMessage);
    }
}
