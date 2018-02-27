using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Products
{
    public class ProductRatingRootObject : ISerializableObject
    {
        public ProductRatingRootObject()
        {
            ProductRating = new ProductRatingDto();
        }

        [JsonProperty("product_rating")]
        public ProductRatingDto ProductRating { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "product_rating";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ProductRatingDto);
        }
    }
}
