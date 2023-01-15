using EventStoreAPI.Context;
using EGD.Command.Models;
using EGD.Command.Models.Enums;
using System;

namespace EventStoreAPI.Repositories
{
    public class CommandEventRepository : ICommandEventRepository
    {
        private readonly IDataContext _context;
        private readonly ILogger<CommandEventRepository> _logger;
        public CommandEventRepository(ILogger<CommandEventRepository> logger, IDataContext dataContext)
        {
            _logger = logger;
            _context = dataContext;

        }
        public Task<List<CommandEventStorePublic>> GetCommandEventsAsync(string action, int? limit = null, int? skip = null)
        {
            return _context.Get(action, null, null, limit, skip);
        }

        public Task<List<CommandEventStorePublic>> GetCommandEventsAsync(Guid? correlationId, int? limit = null, int? skip = null)
        {
            return _context.Get(correlationId, limit, skip);
        }

        public Task<List<CommandEventStorePublic>> GetCommandEventsAsync(string action, Guid? correlationId, CommandStatus? status, int? limit, int? skip)
        {
            return _context.Get(action, correlationId, status, limit, skip);
        }

        public Task<List<CommandEventSummary>> GetSummary(Guid correlationId)
        {
            return _context.GroupByActionStatus(correlationId);
        }
    }
}
