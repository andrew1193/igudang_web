using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.ShippingMethods
{
    [JsonObject(Title = "ShippingMethods")]
    public class ShippingMethodsDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("displayOrder")]
        public string DisplayOrder { get; set; }
    }
}
