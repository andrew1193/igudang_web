using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Products
{
    [JsonObject(Title = "product_rating")]
    public class ProductRatingDto
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("rating_sum")]
        public int RatingSum { get; set; }

        [JsonProperty("total_reviews")]
        public int TotalReviews { get; set; }

        [JsonProperty("allow_Customer_reviews")]
        public bool AllowCustomerReviews { get; set; }

        [JsonProperty("rating_percent")]
        public int RatingPercent { get; set; }
    }
}
