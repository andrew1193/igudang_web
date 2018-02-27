using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Images
{
    public class SlidersRootObject : ISerializableObject
    {
        public SlidersRootObject()
        {
            Sliders = new List<SliderDto>();
        }

        [JsonProperty("sliders")]
        public IList<SliderDto> Sliders { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "sliders";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(SliderDto);
        }
    }
}
