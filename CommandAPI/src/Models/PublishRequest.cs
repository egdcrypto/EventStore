using Newtonsoft.Json.Linq;

namespace CommandAPI.Models
{
    public class PublishRequest
    {
        public string Action { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}
