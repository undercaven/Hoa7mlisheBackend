using Microsoft.AspNetCore.SignalR;

namespace hoa7mlishe.API.Trading
{
    public class TradesHub : Hub
    {
        public async Task SendTradeMessage(string message)
        {
            await this.Clients.All.SendAsync("Receive", message);
        }
    }
}
