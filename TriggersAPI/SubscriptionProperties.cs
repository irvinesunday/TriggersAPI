using Newtonsoft.Json;

namespace TriggersAPI
{
    public class SubscriptionProperties
    {
        public string Url { get; set; }

        [JsonProperty("changeType")]
        //[JsonConverter(typeof(StringEnumConverter))]
        public string[] ChangeType { get; set; }

        [JsonProperty("expirationDateTime")]
        public string ExpirationDateTime { get; set; }
    }
}
