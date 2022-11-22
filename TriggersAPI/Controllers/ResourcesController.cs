using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Text;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TriggersAPI.Controllers
{
    
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private static readonly IEnumerable<Resource> Resources = new List<Resource>()
        {
            new Resource
            {
                Id = 1, RequestUrl = "/teams/getAllMessages"
            },
            new Resource
            {
                Id = 2, RequestUrl = "/teams/{team-id}/channels/{channel-id}/messages"
            },
            new Resource
            {
                Id = 3, RequestUrl = "/chats/{chat-id}/messages"
            },
            new Resource
            {
                Id = 4, RequestUrl = "/chats/getAllMessages"
            },
            new Resource
            {
                Id = 5, RequestUrl = "/users/{user-id}/chats/getAllMessages"
            }
        };

        [Route("resources")]
        [HttpGet]
        public IActionResult GetResources()
        {
            return Ok(Resources);
        }

        [Route("resources/{id}/schema")]
        [Produces("application/json")]
        [HttpGet]
        public IActionResult GetResourceSchema(int id)
        {
            var schema = FetchResourceSchema(id);
            if (schema != null)
            {
                return Content(schema.ToString(), "application/json", Encoding.UTF8);
            }
            return NoContent();
        }

        [Route("properties")]
        [Produces("application/json")]
        [HttpGet]
        public IActionResult GetPropertiesSchema(int id)
        {
            return Content(GetProperties(), "application/json", Encoding.UTF8);
        }

        [Route("resources/{id}/subscribe")]
        [HttpPost]
        public async Task<IActionResult> Submit(int id)
        {
            string bodyPayload;
            using (var body = Request.Body)
            {
                using var reader = new StreamReader(body);
                bodyPayload = await reader.ReadToEndAsync();
            }

            var requestUrl = Resources.FirstOrDefault(x => x.Id.Equals(id))?.RequestUrl;            
            var requestBodyJArray = JObject.Parse(GetSampleSubscription());
            var payload = GetSampleSubscription();
            var subscription = JsonConvert.DeserializeObject<Subscription>(payload);

            if (requestUrl != null)
            {
                requestUrl = ReplaceUrlKeySegmentWithIdValue(requestUrl, requestBodyJArray);
                subscription.SubscriptionProperties.Url = requestUrl;
            }            

            return Ok();
        }

        [Route("resources/{id}/unsubscribe")]
        [HttpDelete]
        public IActionResult DeleteSubscription()
        {

            return Ok();
        }

        private string? ReplaceUrlKeySegmentWithIdValue(string url, JObject input)
        {            
            var keySegments = input.SelectToken("subscriptionProperties")?
                .Children()
                .Select(x => x.ToString())
                .Where(y => y.Contains("id", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (keySegments != null && url != null)
            {
                foreach (var keySegment in keySegments)
                {
                    var cleanedKeySegment = keySegment.Replace("\"", string.Empty);
                    var keySegmentArray = cleanedKeySegment.Split(":", StringSplitOptions.RemoveEmptyEntries);

                    Regex rx = new($"(\\{{)({keySegmentArray[0]})(\\}})");
                    url = rx.Replace(url, keySegmentArray[1].Trim(), 1);
                }           
            }

            return url;
        }

        private List<string>? GetKeySegments(string url)
        {
            Regex rx = new("(?<=\\{)(.*?)(?=\\})");
            if (url != null)
            {
                return rx.Matches(url).Select(x => x.Value).ToList();
            }
            return null;
        }

        private string FetchResourceSchema(int id)
        {
            string? url = Resources.FirstOrDefault(x => x.Id == id)?.RequestUrl;
            if (url != null)
            {
                var segments = GetKeySegments(url);
                return GetProperties(segments);
            }
            return null;            
        }

        private string GetProperties(List<string>? segments = null)
        {            
            string? properties = null;
            
            if (segments != null)
            {
                foreach (var segment in segments)
                {
                    var segmentName = $"{segment.Split('-').First()}Id";
                    string description = $"The {segment.Split('-').First()} id";
                    properties += string.Format(@"
    ""{0}"": {{
      ""type"": ""string"",
      ""title"": ""{1}"",
      ""description"": ""{2}"",
      ""x-ms-visibility"": ""important""
     }},", segment, segment, description);
                }
            }
            

            var schema = string.Format(
@"{{  
""type"": ""object"",
""properties"": {{
   {0}
   {1}
  }}
}}", properties?.Trim(), GetProperties());

            return schema;
        }

        private string GetProperties()
        {
            return
@" ""changeType"": {
    ""type"": ""array"",
    ""title"": ""Change Type"",
    ""description"": ""Indicates the type of change in the subscribed resource that will raise a change notification. The supported values are: created, updated, deleted. Multiple values can be combined using a comma-separated list."",
    ""x-ms-visibility"": ""important"",
    ""items"": {
        ""type"": ""string"",
        ""enum"": [
            ""created"",
            ""updated"",
            ""deleted""
        ],
        ""default"": ""created""
        }
    },
   ""expirationDateTime"": {
    ""type"": ""string"",
    ""title"": ""Expiration DateTime"",
    ""description"": ""Date and time when the webhook subscription expires. Example format 2022-09-25T11:00:00.0000000Z"",
    ""x-ms-visibility"": ""important"",
    ""pattern"": ""^[0-9]{4,}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])T([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]([.][0-9]{1,12})?(Z|[+-][0-9][0-9]:[0-9][0-9])$"",
    ""format"": ""date-time""
    }";

        }

        private string GetSampleSubscription()
        {
            return
                @"{
  ""includeResourceData"":false,
  ""subscriptionProperties"":
  {
    ""chat-id"":""12314"",
    ""changeType"":[""created""],
    ""expirationDateTime"":""2022-09-25T11:00:00Z""
  },
  ""notificationUrl"":""https://prod-240.westeurope.logic.azure.com/workflows/d8af7c4807ab4445946636561426af31/triggers/Create_a_Microsoft_Graph_change_notification_subscription/versions/08585330776762131491/run?api-version=2016-06-01&sp=%2Ftriggers%2FCreate_a_Microsoft_Graph_change_notification_subscription%2Fversions%2F08585330776762131491%2Frun%2C%2Ftriggers%2FCreate_a_Microsoft_Graph_change_notification_subscription%2Fversions%2F08585330776762131491%2Fread&sv=1.0&sig=xaQhHnuPwjllhVk5gcyO1KtGdIg9Pto_Q-UP_bW74k0""}";
        }
    }
}
