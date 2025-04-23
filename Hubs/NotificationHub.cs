using Microsoft.AspNetCore.SignalR;

namespace FliesProject.Hubs
{
    public class NotificationHub : Hub
    {
        private static Dictionary<string, string> _userConnections = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(string userId, string message, string type)
        {
            if (_userConnections.TryGetValue(userId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveNotification", message, type);
            }
        }

        public async Task SendNotification(string message, string type)
        {
            await Clients.All.SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendTableUpdate(string tableData)
        {
            await Clients.All.SendAsync("ReceiveTableUpdate", tableData);
        }
    }
} 