using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Api.DTOs.ShoppingCarts
{
    [JsonObject(Title = "shopping_cart")]
    public class ShoppingCartDto
    {
        public ShoppingCartDto()
        {
            Items = new List<CartItemDto>();
            Warnings = new List<string>();
            //EstimateShipping = new EstimateShippingModel();
            DiscountBox = new DiscountBoxDto();
            //GiftCardBox = new GiftCardBoxDto();
            CheckoutAttributes = new List<CheckoutAttributeDto>();
            //OrderReviewData = new OrderReviewDataDto();

            //ButtonPaymentMethodActionNames = new List<string>();
            //ButtonPaymentMethodControllerNames = new List<string>();
            //ButtonPaymentMethodRouteValues = new List<RouteValueDictionary>();
        }

        [JsonProperty("one_page_checkout_enabled")]
        public bool OnePageCheckoutEnabled { get; set; }

        [JsonProperty("show_sku")]
        public bool ShowSku { get; set; }

        [JsonProperty("show_product_images")]
        public bool ShowProductImages { get; set; }

        [JsonProperty("is_editable")]
        public bool IsEditable { get; set; }

        [JsonProperty("items")]
        public IList<CartItemDto> Items { get; set; }

        [JsonProperty("checkout_attribute_info")]
        public string CheckoutAttributeInfo { get; set; }

        [JsonProperty("checkout_attributes")]
        public IList<CheckoutAttributeDto> CheckoutAttributes { get; set; }

        [JsonProperty("warnings")]
        public IList<string> Warnings { get; set; }

        [JsonProperty("min_order_subtotal_warning")]
        public string MinOrderSubtotalWarning { get; set; }

        [JsonProperty("display_tax_shipping_info")]
        public bool DisplayTaxShippingInfo { get; set; }

        [JsonProperty("terms_of_service_on_shopping_cart_page")]
        public bool TermsOfServiceOnShoppingCartPage { get; set; }

        [JsonProperty("terms_of_servicec_on_order_confirm_page")]
        public bool TermsOfServiceOnOrderConfirmPage { get; set; }

        //[JsonProperty("estimate_shipping")]
        //public EstimateShippingModel EstimateShipping { get; set; }

        [JsonProperty("discount_box")]
        public DiscountBoxDto DiscountBox { get; set; }

        //[JsonProperty("gift_card_box")]
        //public GiftCardBoxDto GiftCardBox { get; set; }

        //[JsonProperty("order_review_data")]
        //public OrderReviewDataDto OrderReviewData { get; set; }

        //[JsonProperty("button_payment_method_action_names")]
        //public IList<string> ButtonPaymentMethodActionNames { get; set; }

        //[JsonProperty("button_payment_method_controller_names")]
        //public IList<string> ButtonPaymentMethodControllerNames { get; set; }

        //[JsonProperty("button_payment_method_route_values")]
        //public IList<RouteValueDictionary> ButtonPaymentMethodRouteValues { get; set; }

        //[JsonProperty("hide_checkout_button")]
        //public bool HideCheckoutButton { get; set; }
    }

    [JsonObject(Title = "cart_item")]
    public class CartItemDto
    {
        public CartItemDto()
        {
            Picture = new PictureDto();
            AllowedQuantities = new List<SelectListItem>();
            Warnings = new List<string>();
        }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("picture")]
        public PictureDto Picture { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("product_se_name")]
        public string ProductSeName { get; set; }

        [JsonProperty("unit_price")]
        public string UnitPrice { get; set; }

        [JsonProperty("sub_total")]
        public string SubTotal { get; set; }

        [JsonProperty("discount")]
        public string Discount { get; set; }

        [JsonProperty("maximum_discounted_qty")]
        public int? MaximumDiscountedQty { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("allowed_quantities")]
        public List<SelectListItem> AllowedQuantities { get; set; }

        [JsonProperty("attribute_info")]
        public string AttributeInfo { get; set; }

        [JsonProperty("recuring_info")]
        public string RecurringInfo { get; set; }

        [JsonProperty("rental_info")]
        public string RentalInfo { get; set; }

        [JsonProperty("allow_item_editing")]
        public bool AllowItemEditing { get; set; }

        [JsonProperty("disable_removal")]
        public bool DisableRemoval { get; set; }

        [JsonProperty("warnings")]
        public IList<string> Warnings { get; set; }
    }

    [JsonObject(Title = "picture")]
    public class PictureDto
    {
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("thumb_image_url")]
        public string ThumbImageUrl { get; set; }

        [JsonProperty("full_size_image_url")]
        public string FullSizeImageUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("alternate_text")]
        public string AlternateText { get; set; }
    }

    [JsonObject(Title = "discount_box")]
    public class DiscountBoxDto
    {
        public DiscountBoxDto()
        {
            AppliedDiscountsWithCodes = new List<DiscountInfoDto>();
            Messages = new List<string>();
        }

        [JsonProperty("applied_discounts_with_codes")]
        public List<DiscountInfoDto> AppliedDiscountsWithCodes { get; set; }

        [JsonProperty("display")]
        public bool Display { get; set; }

        [JsonProperty("messages")]
        public List<string> Messages { get; set; }

        [JsonProperty("is_applied")]
        public bool IsApplied { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    [JsonObject(Title = "discount_info")]
    public class DiscountInfoDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("coupon_code")]
        public string CouponCode { get; set; }
    }

    [JsonObject(Title = "giftcard_box")]
    public class GiftCardBoxDto
    {
        [JsonProperty("display")]
        public bool Display { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("is_applied")]
        public bool IsApplied { get; set; }
    }

    [JsonObject(Title = "checkout_attribute")]
    public partial class CheckoutAttributeDto
    {
        public CheckoutAttributeDto()
        {
            AllowedFileExtensions = new List<string>();
            Values = new List<CheckoutAttributeValueDto>();
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }

        [JsonProperty("text_prompt")]
        public string TextPrompt { get; set; }

        [JsonProperty("is_required")]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Selected day value for datepicker
        /// </summary>
        [JsonProperty("selected_day")]
        public int? SelectedDay { get; set; }
        /// <summary>
        /// Selected month value for datepicker
        /// </summary>
        [JsonProperty("selected_month")]
        public int? SelectedMonth { get; set; }
        /// <summary>
        /// Selected year value for datepicker
        /// </summary>
        [JsonProperty("selected_year")]
        public int? SelectedYear { get; set; }

        /// <summary>
        /// Allowed file extensions for customer uploaded files
        /// </summary>
        [JsonProperty("allowed_file_extensions")]
        public IList<string> AllowedFileExtensions { get; set; }

        [JsonProperty("attributeControlType")]
        public AttributeControlType AttributeControlType { get; set; }

        [JsonProperty("values")]
        public IList<CheckoutAttributeValueDto> Values { get; set; }
    }

    [JsonObject(Title = "checkout_attribute_value")]
    public partial class CheckoutAttributeValueDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color_squares_rbg")]
        public string ColorSquaresRgb { get; set; }

        [JsonProperty("price_adjustment")]
        public string PriceAdjustment { get; set; }

        [JsonProperty("is_preselected")]
        public bool IsPreSelected { get; set; }
    }

    [JsonObject(Title = "order_review_data")]
    public partial class OrderReviewDataDto
    {
        public OrderReviewDataDto()
        {
            this.BillingAddress = new AddressDto();
            this.ShippingAddress = new AddressDto();
            this.PickupAddress = new AddressDto();
            this.CustomValues = new Dictionary<string, object>();
        }

        [JsonProperty("display")]
        public bool Display { get; set; }

        [JsonProperty("billing_address")]
        public AddressDto BillingAddress { get; set; }

        [JsonProperty("is_shippable")]
        public bool IsShippable { get; set; }

        [JsonProperty("shipping_address")]
        public AddressDto ShippingAddress { get; set; }

        [JsonProperty("selected_pickup_in_store")]
        public bool SelectedPickUpInStore { get; set; }

        [JsonProperty("pickup_address")]
        public AddressDto PickupAddress { get; set; }

        [JsonProperty("shipping_method")]
        public string ShippingMethod { get; set; }

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("custom_values")]
        public Dictionary<string, object> CustomValues { get; set; }
    }
}
