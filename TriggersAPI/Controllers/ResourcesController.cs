using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Schema;
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

        // GET: api/<ResourcesController>
        //[Route("api/[controller]")]
        [Route("resources")]
        [HttpGet]
        public IActionResult GetResources()
        {
            return Ok(Resources);
        }

        // GET: api/<ResourcesController>
        [Route("resources/{id}/schema")]
        [HttpGet]
        public IActionResult GetResourceSchema(int id)
        {
            return Ok(FetchResourceSchema(id));
        }

        private IEnumerable<Resource> GetResourceUrls()
        {
            return new List<Resource>()
            {
                new Resource
                {
                    Id = 1, RequestUrl = "/teams/getAllMessages"
                },
                new Resource
                {
                    Id = 2, RequestUrl = "/teams/{id}/channels/{id}/messages"
                },
                new Resource
                {
                    Id = 3, RequestUrl = "/chats/{id}/messages"
                },
                new Resource
                {
                    Id = 4, RequestUrl = "/chats/getAllMessages"
                },
                new Resource
                {
                    Id = 5, RequestUrl = "/users/{id}/chats/getAllMessages"
                }
            };
        }

        private string FetchResourceSchema(int id)
        {
            string? url = Resources.FirstOrDefault(x => x.Id == id)?.RequestUrl;
            Regex rx = new ("(?<=\\{)(.*?)(?=\\})");
            if (url != null)
            {
                var keySegments = rx.Matches(url).ToList();
                if (keySegments.Any())
                    return GetPropertiesFromSegments(keySegments);
            }

            return null;
        }

        private string GetPropertiesFromSegments(List<Match> segments)
        {            
            string? properties = null;
            
            foreach (var segment in segments)
            {
                var segmentName = $"{segment.Value.Split('-').First()}Id";
                string description = $"The {segment.Value.Split('-').First()} id";
                properties += string.Format(@"
    ""{0}"": {{
      ""type"": ""string"",
      ""title"": ""{0}"",
      ""description"": ""{1}""
     }},", segmentName, description);
            }

            //            var schema = string.Format(@"{{
            //  ""schema"": {{
            //    ""type"": ""object"",
            //    ""properties"": {{
            //       {0}
            //     }}
            //   }}
            //}}", properties?.Trim().Trim(','));

            var schema = string.Format(@"{{  
""type"": ""object"",
""properties"": {{
   {0}
  }}
}}", properties?.Trim().Trim(','));

            return schema;
        }

    }
}
