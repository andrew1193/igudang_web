using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.ShippingMethods
{
    public class ShippingMethodsRootObject : ISerializableObject
    {
        public ShippingMethodsRootObject()
        {
            ShippingMethods = new List<ShippingMethodsDto>();
        }

        [JsonProperty("shippingMethods")]
        public IList<ShippingMethodsDto> ShippingMethods { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "shippingMethods";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ShippingMethodsDto);
        }
    }
}
