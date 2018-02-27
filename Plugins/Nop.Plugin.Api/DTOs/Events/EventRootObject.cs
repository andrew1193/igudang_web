using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Events
{
    public class EventRootObject : ISerializableObject
    {
        public EventRootObject()
        {
            Event = new EventDto();
        }

        [JsonProperty("event")]
        public EventDto Event { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "event";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(EventDto);
        }
    }
}
