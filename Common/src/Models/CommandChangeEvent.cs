using EGD.Command.Models.Enums;

namespace EGD.Command.Models
{
    public class CommandChangeEvent<TBefore, TAfter> : CommandEvent
    {
        public ChangeDataCapture<TBefore, TAfter> ChangeData { get; set; }
        public CommandOperation Operation { get; set; }
    }
}
