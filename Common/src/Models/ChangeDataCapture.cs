namespace EGD.Command.Models
{
    /// <summary>
    /// This class is used for change data capture.
    /// </summary>
    /// <typeparam name="TBefore"></typeparam>
    /// <typeparam name="TAfter"></typeparam>
    public class ChangeDataCapture<TBefore, TAfter>
    {
        public string Database { get; set; }
        public string Table { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TBefore Before { get; set; }
        public TAfter After { get; set; }
        public string SourceRowId { get; set; }
    }
}
