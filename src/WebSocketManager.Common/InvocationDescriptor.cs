using Newtonsoft.Json;

namespace WebSocketManager.Common
{
    public class InvocationDescriptor
    {
        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("controller")]
        public string Controller { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonPropertyAttribute("arguments")]
        public object[] Arguments { get; set; }
    }
}