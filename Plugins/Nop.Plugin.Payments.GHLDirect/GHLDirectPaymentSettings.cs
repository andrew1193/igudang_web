using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.GHLDirect
{
    public class GHLDirectPaymentSettings : ISettings
    {
        public string GHLDirectAPIUrl { get; set; }
        public string PaymentMethod { get; set; }
        public string MerchantServiceId { get; set; }
        public string MerchantPassword { get; set; }
        public string MerchantName { get; set; }
        public int PageTimeoutForCompeletePaymentProcess { get; set; }
        public bool PdtValidateOrderTotal { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        public bool PassProductNamesAndTotals { get; set; }

        /// <summary>
        /// Enable to Pass billing address 
        /// </summary>
        public bool EnablePassingBillingAddressToPaymentGateway { get; set; }
        /// <summary>
        /// Enable to pass shipping address 
        /// </summary>
        public bool EnablePassingShippingAddressToPaymentGateway { get; set; }
           
        /// <summary>
        /// Enable if a customer should be redirected to the order details page
        /// when he clicks "return to store" link in eGHL setting 
        /// WITHOUT completing a payment
        /// </summary>
        public bool ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage { get; set; }
    }
}
