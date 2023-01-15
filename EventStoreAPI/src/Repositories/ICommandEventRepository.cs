using EventStoreAPI.Context;
using EGD.Command.Models;
using EGD.Command.Models.Enums;

namespace EventStoreAPI.Repositories
{
    public interface ICommandEventRepository
    {
        Task<List<CommandEventStorePublic>> GetCommandEventsAsync(string action, int? limit, int? skip);
        Task<List<CommandEventStorePublic>> GetCommandEventsAsync(Guid? correlationId, int? limit, int? skip);
        Task<List<CommandEventStorePublic>> GetCommandEventsAsync(string action, Guid? correlationId, CommandStatus? status, int? limit, int? skip);
        Task<List<CommandEventSummary>> GetSummary(Guid correlationId);
    }
}
