using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BlueCheese.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message).ConfigureAwait(false);
        }
    }
}