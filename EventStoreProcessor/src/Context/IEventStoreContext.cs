using EGD.Command.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrations.Messaging.Context
{
    public interface IEventStoreContext
    {
        Task InsertOneAsync(CommandEventStore commandEvent);
    }
}
