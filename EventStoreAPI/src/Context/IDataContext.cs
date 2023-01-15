
using EGD.Command.Models;
using EGD.Command.Models.Enums;

namespace EventStoreAPI.Context
{
    public interface IDataContext
    {
        Task<List<CommandEventStorePublic>> Get(string action, Guid? correlationId = null, CommandStatus? status = null, int? limit = null, int? skip = null);
        Task<List<CommandEventStorePublic>> Get(Guid? correlationId, int? limit = null, int? skip = null);
        Task<CommandEventStorePublic> GetLatest(Guid? correlationId);
        Task<CommandEventStorePublic> GetEarliest(Guid? correlationId);
        Task<List<CommandEventSummary>> GroupByActionStatus(Guid? correlationId);
    }
}
