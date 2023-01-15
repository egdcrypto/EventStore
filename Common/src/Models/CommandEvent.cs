using EGD.Command.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EGD.Command.Models
{
    public class CommandEvent
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public Guid RecordId { get; set; } = Guid.NewGuid();
        public Guid BatchId { get; set; } = Guid.NewGuid();
        public string Action { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        [JsonConverter(typeof(StringEnumConverter))]
        public CommandStatus Status { get; set; }
        public string Actor { get; set; }
        public List<EntityReference> EntityReferences { get; set; }
    }
}