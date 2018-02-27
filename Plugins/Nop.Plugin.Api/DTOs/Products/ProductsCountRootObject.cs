using Newtonsoft.Json;
using System;

namespace Nop.Plugin.Api.DTOs.Products
{
    public class ProductsCountRootObject : ISerializableObject
    {
        public ProductsCountRootObject()
        {
            Count = new ProductCountDto();
        }

        [JsonProperty("count")]
        public ProductCountDto Count { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "count";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ProductCountDto);
        }
    }
}