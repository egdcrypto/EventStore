using EGD.Command.Models.Enums;

namespace EGD.Command.Models
{
    /// <summary>
    /// This class is used to write to the event store
    /// May need to change dynamic to Dictionary<string,object>, depends on serialization of the data.
    /// If we use elasticsearch as the event store, it will accept a dictionary object.
    /// </summary>
    public class CommandEventStore : CommandEvent
    {
        public ChangeDataCapture<dynamic, dynamic> ChangeData { get; set; }
        public dynamic ProcessData { get; set; }
        public CommandOperation Operation { get; set; }
    }
}
