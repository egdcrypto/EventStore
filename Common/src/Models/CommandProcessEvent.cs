using EGD.Command.Models.Enums;

namespace EGD.Command.Models
{
    public class CommandProcessEvent<T> : CommandEvent
    {
        public T ProcessData { get; set; }
        public CommandOperation Operation { get; set; } = CommandOperation.Process;

    }
}
