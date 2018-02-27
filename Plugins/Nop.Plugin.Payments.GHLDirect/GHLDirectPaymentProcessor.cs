using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.GHLDirect.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using System.Security.Cryptography;

namespace Nop.Plugin.Payments.GHLDirect
{
    /// <summary>
    /// GHLDirect payment processor
    /// </summary>
    public class GHLDirectPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly GHLDirectPaymentSettings _ghlDirectPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ITaxService _taxService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly HttpContextBase _httpContext;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public GHLDirectPaymentProcessor(GHLDirectPaymentSettings ghlDirectPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService,
            IOrderTotalCalculationService orderTotalCalculationService, HttpContextBase httpContext,
            ILocalizationService localizationService)
        {
            this._ghlDirectPaymentSettings = ghlDirectPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._taxService = taxService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._httpContext = httpContext;
            this._localizationService = localizationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            string url = _webHelper.GetStoreLocation(false) + "Plugins/PaymentGHLDirect/RedirectToGHLDirect/{0}";
            url = string.Format(url, postProcessPaymentRequest.Order.OrderGuid);
            _httpContext.Response.Redirect(url);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _ghlDirectPaymentSettings.AdditionalFee, _ghlDirectPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentGHLDirect";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.GHLDirect.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentGHLDirect";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.GHLDirect.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentGHLDirectController);
        }

        public override void Install()
        {
            //settings
            var settings = new GHLDirectPaymentSettings
            {
                GHLDirectAPIUrl = "https://test2pay.ghl.com/IPGSG/Payment.aspx",
                PaymentMethod = "ANY",
                MerchantServiceId = "sit",
                MerchantPassword = "sit12345",
                MerchantName = "iGudang",
                PageTimeoutForCompeletePaymentProcess = 900,
                PdtValidateOrderTotal = true,
                PassProductNamesAndTotals = true,
                EnablePassingBillingAddressToPaymentGateway = true,
                EnablePassingShippingAddressToPaymentGateway = false,
                ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage = true
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PaymentProcessingTip", "Wait while your transaction is being processed...........");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.RedirectionTip", "You will be redirected to eGHL direct payment gateway to complete the order.");
           
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.GHLDirectAPIUrl", "eGHL Direct API url");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.GHLDirectAPIUrl.Hint", "API url provided by eGHL direct to merchant. Please use appropriate API url as per store configuration(Live or Testing).");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PaymentMethod", "Payment Method");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PaymentMethod.Hint", "Payment Method -> CC – Credit Card, DD – Direct Debit, WA – e-Wallet, ANY – All payment method(s) registered with eGHL.");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantServiceId", "Merchant Service ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantServiceId.Hint", "Specify Merchant Service ID given by eGHL.");
           
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantPassword", "Merchant Password");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantPassword.Hint", "Specify Merchant Password given by eGHL.");
           
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantName", "Merchant Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantName.Hint", "Specify Merchant’s business name.");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PageTimeoutForCompeletePaymentProcess", "Page Timeout For Compelete Payment Process");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PageTimeoutForCompeletePaymentProcess.Hint", "eGHL Payment Info Collection Page timeout in seconds Applicable for merchant system which would like to bring forward to Payment Gateway, the time remaining before product/order is released.");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PDTValidateOrderTotal", "PDT. Validate order total");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PDTValidateOrderTotal.Hint", "Check if PDT handler should validate order totals.");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
           
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PassProductNamesAndTotals", "Pass product names and order totals to eGHL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PassProductNamesAndTotals.Hint", "Check if product names and order totals should be passed to eGHL.");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.EnablePassingBillingAddressToPaymentGateway", "Enable to Pass Billing Address to eGHL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.EnablePassingBillingAddressToPaymentGateway.Hint", "Check if billing address of the Customer should be passed to eGHL.");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.EnablePassingShippingAddressToPaymentGateway", "Enable to Pass Shipping Address to eGHL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.EnablePassingShippingAddressToPaymentGateway.Hint", "Check if shipping address of the Customer should be passed to eGHL.");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage", "Return to order details page");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage.Hint", "Enable if a customer should be redirected to the order details page when he clicks \"return to store\" link on eGHL setting WITHOUT completing a payment");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.GHLDirect.PaymentMethodDescription", "Pay by credit / debit card");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<GHLDirectPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PaymentProcessingTip");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.GHLDirectAPIUrl");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.GHLDirectAPIUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantServiceId");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantServiceId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantPassword");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantPassword.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantName");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.MerchantName.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PageTimeoutForCompeletePaymentProcess");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PageTimeoutForCompeletePaymentProcess.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PdtValidateOrderTotal");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PdtValidateOrderTotal.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.AdditionalFeePercentage.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PassProductNamesAndTotals");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.PassProductNamesAndTotals.Hint");


            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.EnablePassingBillingAddressToPaymentGateway");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.EnablePassingBillingAddressToPaymentGateway.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.EnablePassingShippingAddressToPaymentGateway");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.EnablePassingShippingAddressToPaymentGateway.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.Fields.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.GHLDirect.PaymentMethodDescription");

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to GHL site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.GHLDirect.PaymentMethodDescription"); }
        }

        #endregion
    }
}
