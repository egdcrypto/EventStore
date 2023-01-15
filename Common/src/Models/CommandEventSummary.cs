using EGD.Command.Models.Enums;

namespace EGD.Command.Models
{
    public class CommandEventSummary
    {
        public string Action { get; set; }
        public CommandStatus Status { get; set; }
        public Guid Correlationid { get; set; }
        public int Count { get; set; }
    }
}
