using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.GHLDirect.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.GHLDirectAPIUrl")]
        public string GHLDirectAPIUrl { get; set; }
        public bool GHLDirectAPIUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.PaymentMethod")]
        public string PaymentMethod { get; set; }
        public bool PaymentMethod_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.MerchantServiceId")]
        public string MerchantServiceId { get; set; }
        public bool MerchantServiceId_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.MerchantPassword")]
        public string MerchantPassword { get; set; }
        public bool MerchantPassword_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.MerchantName")]
        public string MerchantName { get; set; }
        public bool MerchantName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.PageTimeoutForCompeletePaymentProcess")]
        public int PageTimeoutForCompeletePaymentProcess { get; set; }
        public bool PageTimeoutForCompeletePaymentProcess_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.PDTValidateOrderTotal")]
        public bool PdtValidateOrderTotal { get; set; }
        public bool PdtValidateOrderTotal_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.PassProductNamesAndTotals")]
        public bool PassProductNamesAndTotals { get; set; }
        public bool PassProductNamesAndTotals_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.EnablePassingBillingAddressToPaymentGateway")]
        public bool EnablePassingBillingAddressToPaymentGateway { get; set; }
        public bool EnablePassingBillingAddressToPaymentGateway_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.EnablePassingShippingAddressToPaymentGateway")]
        public bool EnablePassingShippingAddressToPaymentGateway { get; set; }
        public bool EnablePassingShippingAddressToPaymentGateway_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GHLDirect.Fields.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage")]
        public bool ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage { get; set; }
        public bool ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore { get; set; }
    }
}