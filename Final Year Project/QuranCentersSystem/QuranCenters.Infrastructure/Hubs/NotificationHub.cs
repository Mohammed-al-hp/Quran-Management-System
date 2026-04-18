using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace QuranCenters.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        // Hub logic for real-time communication
        public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
