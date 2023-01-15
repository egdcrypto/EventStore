using EGD.Command.Models;
using Microsoft.AspNetCore.SignalR;

namespace Hubs
{
    public class EventHub : Hub<IEventHub>
    {
        public async Task BroadCastActionStatusCorrelationId(string action, string status, string correlationId)
        {
            await Clients.All.BroadCastActionStatusCorrelationId(action, status, correlationId);
        }
        public async Task BroadcastCommandEvent(CommandEventStore commandEvent)
        {
            await Clients.Client(commandEvent.Action).BroadcastCommandEvent(commandEvent);
        }
        public async Task BroadcastCorrelationId(Guid correlationId)
        {
            await Clients.All.BroadcastCorrelationId(correlationId);
        }
    }

    #region IEventHub
    public interface IEventHub
    {
        Task BroadCastActionStatusCorrelationId(string action, string status, string correlationId);
        Task BroadcastCorrelationId(Guid correlationId);
        Task BroadcastCommandEvent(CommandEventStore commandEvent);
    }
    #endregion
}
