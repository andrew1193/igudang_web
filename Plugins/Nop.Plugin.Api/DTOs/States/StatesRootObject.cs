using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.States
{
    public class StatesRootObject : ISerializableObject
    {
        public StatesRootObject()
        {
            States = new List<StateDto>();
        }

        [JsonProperty("states")]
        public IList<StateDto> States { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "states";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(StateDto);
        }
    }
}
