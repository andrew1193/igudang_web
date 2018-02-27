using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.Customers;

namespace Nop.Plugin.Api.DTOs.Products
{
    [JsonObject(Title = "product_review")]
    public class ProductReviewDto
    {
        private CustomerDto _customerDto;

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty("customer")]
        public CustomerDto Customer
        {
            get { return _customerDto; }
            set { _customerDto = value; }
        }

        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("store_id")]
        public string StoreId { get; set; }

        [JsonProperty("is_approved")]
        public bool IsApproved { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("review_text")]
        public string ReviewText { get; set; }

        [JsonProperty("reply_text")]
        public string ReplyText { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("helpful_yes_total")]
        public int HelpfulYesTotal { get; set; }

        [JsonProperty("helpful_no_total")]
        public int HelpfulNoTotal { get; set; }

        [JsonProperty("created_on_utc")]
        public DateTime CreatedOnUtc { get; set; }
    }
}
