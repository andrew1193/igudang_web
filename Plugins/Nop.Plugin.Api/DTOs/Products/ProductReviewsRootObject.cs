using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Products
{
    public class ProductReviewsRootObject : ISerializableObject
    {
        public ProductReviewsRootObject()
        {
            ProductReviews = new List<ProductReviewDto>();
        }

        [JsonProperty("product_reviews")]
        public IList<ProductReviewDto> ProductReviews { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "product_reviews";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ProductReviewDto);
        }
    }
}
