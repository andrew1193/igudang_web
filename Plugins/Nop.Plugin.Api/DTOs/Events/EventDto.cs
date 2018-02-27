using Newtonsoft.Json;
using System;

namespace Nop.Plugin.Api.DTOs.Events
{
    [JsonObject(Title = "event")]
    public class EventDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("limited_to_stores")]
        public bool LimitedToStores { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("display_order")]
        public int DisplayOrder { get; set; }

        [JsonProperty("created_on_utc")]
        public DateTime CreatedOnUtc { get; set; }

        [JsonProperty("updated_on_utc")]
        public DateTime UpdatedOnUtc { get; set; }

        [JsonProperty("started_on_utc")]
        public DateTime StartedOnUtc { get; set; }

        [JsonProperty("ended_on_utc")]
        public DateTime EndedOnUtc { get; set; }
    }
}
