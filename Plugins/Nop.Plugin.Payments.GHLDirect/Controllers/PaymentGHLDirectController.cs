using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.GHLDirect.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.GHLDirect.Controllers
{
    public class PaymentGHLDirectController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ITaxService _taxService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly PaymentSettings _paymentSettings;
        private readonly GHLDirectPaymentSettings _ghlDirectPaymentSettings;

        public PaymentGHLDirectController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPaymentService paymentService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ITaxService taxService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            ILogger logger,
            IWebHelper webHelper,
            PaymentSettings paymentSettings,
            GHLDirectPaymentSettings ghlDirectPaymentSettings)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._taxService = taxService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._logger = logger;
            this._webHelper = webHelper;
            this._paymentSettings = paymentSettings;
            this._ghlDirectPaymentSettings = ghlDirectPaymentSettings;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var ghlDirectPaymentSettings = _settingService.LoadSetting<GHLDirectPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.GHLDirectAPIUrl = ghlDirectPaymentSettings.GHLDirectAPIUrl;
            model.PaymentMethod = ghlDirectPaymentSettings.PaymentMethod;
            model.MerchantServiceId = ghlDirectPaymentSettings.MerchantServiceId;
            model.MerchantPassword = ghlDirectPaymentSettings.MerchantPassword;
            model.MerchantName = ghlDirectPaymentSettings.MerchantName;
            model.PageTimeoutForCompeletePaymentProcess = ghlDirectPaymentSettings.PageTimeoutForCompeletePaymentProcess;
            model.PdtValidateOrderTotal = ghlDirectPaymentSettings.PdtValidateOrderTotal;
            model.AdditionalFee = ghlDirectPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = ghlDirectPaymentSettings.AdditionalFeePercentage;
            model.PassProductNamesAndTotals = ghlDirectPaymentSettings.PassProductNamesAndTotals;
            model.EnablePassingBillingAddressToPaymentGateway = ghlDirectPaymentSettings.EnablePassingBillingAddressToPaymentGateway;
            model.EnablePassingShippingAddressToPaymentGateway = ghlDirectPaymentSettings.EnablePassingShippingAddressToPaymentGateway;
            model.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage = ghlDirectPaymentSettings.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.GHLDirectAPIUrl_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.GHLDirectAPIUrl, storeScope);
                model.PaymentMethod_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.PaymentMethod, storeScope);
                model.MerchantServiceId_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.MerchantServiceId, storeScope);
                model.MerchantPassword_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.MerchantPassword, storeScope);
                model.MerchantName_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.MerchantName, storeScope);
                model.PageTimeoutForCompeletePaymentProcess_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.PageTimeoutForCompeletePaymentProcess, storeScope);
                model.PdtValidateOrderTotal_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.PdtValidateOrderTotal, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PassProductNamesAndTotals_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
                model.EnablePassingBillingAddressToPaymentGateway_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.EnablePassingBillingAddressToPaymentGateway, storeScope);
                model.EnablePassingBillingAddressToPaymentGateway_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.EnablePassingShippingAddressToPaymentGateway, storeScope);
                model.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore = _settingService.SettingExists(ghlDirectPaymentSettings, x => x.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage, storeScope);
            }

            return View("~/Plugins/Payments.GHLDirect/Views/PaymentGHLDirect/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var ghlDirectPaymentSettings = _settingService.LoadSetting<GHLDirectPaymentSettings>(storeScope);

            //save settings
            ghlDirectPaymentSettings.GHLDirectAPIUrl = model.GHLDirectAPIUrl;
            ghlDirectPaymentSettings.PaymentMethod = model.PaymentMethod;
            ghlDirectPaymentSettings.MerchantServiceId = model.MerchantServiceId;
            ghlDirectPaymentSettings.MerchantPassword = model.MerchantPassword;
            ghlDirectPaymentSettings.MerchantName = model.MerchantName;
            ghlDirectPaymentSettings.PageTimeoutForCompeletePaymentProcess = model.PageTimeoutForCompeletePaymentProcess;
            ghlDirectPaymentSettings.PdtValidateOrderTotal = model.PdtValidateOrderTotal;
            ghlDirectPaymentSettings.AdditionalFee = model.AdditionalFee;
            ghlDirectPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            ghlDirectPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            ghlDirectPaymentSettings.EnablePassingBillingAddressToPaymentGateway = model.EnablePassingBillingAddressToPaymentGateway;
            ghlDirectPaymentSettings.EnablePassingShippingAddressToPaymentGateway = model.EnablePassingShippingAddressToPaymentGateway;
            ghlDirectPaymentSettings.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage = model.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.GHLDirectAPIUrl_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.GHLDirectAPIUrl, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.GHLDirectAPIUrl, storeScope);

            if (model.PaymentMethod_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.PaymentMethod, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.PaymentMethod, storeScope);

            if (model.MerchantServiceId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.MerchantServiceId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.MerchantServiceId, storeScope);

            if (model.MerchantPassword_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.MerchantPassword, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.MerchantPassword, storeScope);

            if (model.MerchantName_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.MerchantName, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.MerchantName, storeScope);

            if (model.PageTimeoutForCompeletePaymentProcess_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.PageTimeoutForCompeletePaymentProcess, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.PageTimeoutForCompeletePaymentProcess, storeScope);

            if (model.PdtValidateOrderTotal_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.PdtValidateOrderTotal, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.PdtValidateOrderTotal, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.PassProductNamesAndTotals_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.PassProductNamesAndTotals, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);

            if (model.EnablePassingBillingAddressToPaymentGateway_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.EnablePassingBillingAddressToPaymentGateway, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.EnablePassingBillingAddressToPaymentGateway, storeScope);

            if (model.EnablePassingShippingAddressToPaymentGateway_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.EnablePassingShippingAddressToPaymentGateway, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.EnablePassingShippingAddressToPaymentGateway, storeScope);

            if (model.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ghlDirectPaymentSettings, x => x.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ghlDirectPaymentSettings, x => x.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.GHLDirect/Views/PaymentGHLDirect/PaymentInfo.cshtml");
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult PDTHandler(FormCollection form)
        {
            var notes = string.Empty;
            var values = new Dictionary<string, string>();
            string response;

            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.GHLDirect") as GHLDirectPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("GHL Direct module cannot be loaded");

            string txnStatus = string.Empty; //get transation status and decide order is sucess or not.
            if (form != null && form.Count > 0)
            {
                values = form.AllKeys.ToDictionary(k => k, v => form[v]);
                values.TryGetValue("TxnStatus", out txnStatus);
            }
            else
            {
                txnStatus = "1";
            }

            if (txnStatus == "0")
            {
                string orderNumber = string.Empty;
                values.TryGetValue("OrderNumber", out orderNumber);

                int orderId = 0;
                try
                {
                    int.TryParse(orderNumber, out orderId);
                }
                catch { }
                Order order = _orderService.GetOrderById(orderId);
                if (order != null)
                {
                    decimal total = decimal.Zero;
                    try
                    {
                        total = decimal.Parse(values["Amount"], new CultureInfo("en-US"));
                    }
                    catch (Exception exc)
                    {
                        _logger.Error("eGHL PDT. Error getting Amount", exc);
                    }


                    
                    //order note
                    if (values.Count > 0)
                    {
                        notes = PrepareOrderNote(values);
                    }
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = "eGHL Direct PDT Success. " + Environment.NewLine + notes,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    //load settings for a chosen store scope
                    var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
                    var ghlDirectPaymentSettings = _settingService.LoadSetting<GHLDirectPaymentSettings>(storeScope);

                    //validate order total
                    if (ghlDirectPaymentSettings.PdtValidateOrderTotal && !Math.Round(total, 2).Equals(Math.Round(order.OrderTotal, 2)))
                    {
                        string errorStr = string.Format("eGHL PDT. Returned order total {0} doesn't equal order total {1}", total, order.OrderTotal);
                        _logger.Error(errorStr);

                        return RedirectToAction("Index", "Home", new { area = "" });
                    }

                    //mark order as paid
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        string txnID = string.Empty;
                        values.TryGetValue("TxnID", out txnID);
                        order.AuthorizationTransactionId = txnID;
                        _orderService.UpdateOrder(order);

                        _orderProcessingService.MarkOrderAsPaid(order);
                    }
                }

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            else
            {
                string orderNumber = string.Empty;

                values.TryGetValue("OrderNumber", out orderNumber);
                int orderId = 0;
                try
                {
                    int.TryParse(orderNumber, out orderId);
                }
                catch { }
                Order order = _orderService.GetOrderById(orderId);
                if (order != null)
                {
                    //order note
                    if (values.Count > 0)
                    {
                        notes = PrepareOrderNote(values);
                    }
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = "eGHL Direct PDT failed. " + Environment.NewLine + notes,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
                //return RedirectToAction("Index", "Home", new { area = "" });
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
        }

        public ActionResult CancelOrder(FormCollection form)
        {
            if (_ghlDirectPaymentSettings.ReturnFromGHLDirectWithoutPaymentRedirectsToOrderDetailsPage)
            {
                var order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                    customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                    .FirstOrDefault();
                if (order != null)
                {
                    return RedirectToRoute("OrderDetails", new { orderId = order.Id });
                }
            }

            return RedirectToAction("Index", "Home", new { area = "" });
            //var order2 = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
            //        customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
            //        .FirstOrDefault();
            //return RedirectToRoute("OrderDetails", new { orderId = order2.Id });
        }

        /// <summary>
        /// Preparing order status note
        /// </summary>
        /// <param name="values">response key value pair</param>
        /// <returns></returns>
        private string PrepareOrderNote(Dictionary<string, string> values)
        {
            string transactionType = string.Empty;
            values.TryGetValue("TransactionType", out transactionType);
            string pymtMethod = string.Empty;
            values.TryGetValue("PymtMethod", out pymtMethod);
            string serviceID = string.Empty;
            values.TryGetValue("ServiceID", out serviceID);
            string paymentID = string.Empty;
            values.TryGetValue("PaymentID", out paymentID);
            string currencyCode = string.Empty;
            values.TryGetValue("CurrencyCode", out currencyCode);
            string amount = string.Empty;
            values.TryGetValue("Amount", out amount);
            string hashValue = string.Empty;
            values.TryGetValue("HashValue", out hashValue);
            string txnID = string.Empty;
            values.TryGetValue("TxnID", out txnID);
            string txnStatus = string.Empty;
            values.TryGetValue("TxnStatus", out txnStatus);
            string issuingBank = string.Empty;
            values.TryGetValue("IssuingBank", out issuingBank);
            string respCode = string.Empty;
            values.TryGetValue("RespCode", out respCode);
            string authCode = string.Empty;
            values.TryGetValue("AuthCode", out authCode);
            string txnMessage = string.Empty;
            values.TryGetValue("TxnMessage", out txnMessage);
            string tokenType = string.Empty;
            values.TryGetValue("TokenType", out tokenType);
            string token = string.Empty;
            values.TryGetValue("Token", out token);

            var sb = new StringBuilder();
            sb.AppendLine("Total: " + amount);
            sb.AppendLine("Transaction Type: " + transactionType);
            sb.AppendLine("Pymt Method: " + pymtMethod);
            sb.AppendLine("Service ID: " + serviceID);
            sb.AppendLine("Payment ID: " + paymentID);
            sb.AppendLine("Currency Code: " + currencyCode);
            sb.AppendLine("Hash Value: " + hashValue);
            sb.AppendLine("TxnID: " + txnID);
            sb.AppendLine("TxnStatus: " + txnStatus);
            sb.AppendLine("Issuing Bank: " + issuingBank);
            sb.AppendLine("Resp Code: " + respCode);
            sb.AppendLine("Auth Code: " + authCode);
            sb.AppendLine("Txn Message: " + txnMessage);
            sb.AppendLine("Token Type: " + tokenType);
            sb.AppendLine("Token: " + token);
            return Convert.ToString(sb);
        }

        /// <summary>
        /// Gets GHL Direct URL
        /// </summary>
        /// <returns></returns>
        private string GetGHLDirectUrl()
        {
            return _ghlDirectPaymentSettings.GHLDirectAPIUrl; //before going for live change this url
        }

        private string GetHashValue(string value)
        {
            ////Payment request’s Hash Value should be generated based on the following fields:
            ////Hash Key = Password + ServiceID + PaymentID + MerchantReturnURL + Amount + CurrencyCode + CustIP + PageTimeout
            ////Hash Key Example
            ////abc123S22PAYTEST123https://www.shop.com/success.asp12.34MYR113.210.6.150900
            ////Hash Value (SHA256)
            ////957f5fdbf4928ae5b220854a4abb6638d9f5cc626a653ff30a01340a6406ec63

            StringBuilder hashKey = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    hashKey.Append(b.ToString("x2"));
            }

            return hashKey.ToString();

        }

        [ValidateInput(false)]
        public ActionResult RedirectToGHLDirect(string orderGuid)
        {
            if (string.IsNullOrEmpty(orderGuid))
            {
                string errorStr = string.Format("Order Processing Failed. No OrderGUID found while processing for eGHL Direct Payment for the customer number '{0}'", _workContext.CurrentCustomer.Id);
                _logger.Error(errorStr);

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            //Fetching other detail and processing it.
            Guid orderNumberGuid;
            Guid.TryParse(orderGuid, out orderNumberGuid);
            Order order = _orderService.GetOrderByGuid(orderNumberGuid);
            var model = new PaymentProcessingModel();
            if (order != null)
            {
                var builder = new StringBuilder();
                model.GHLDirectAPIUrl = GetGHLDirectUrl();
                model.TransactionType = "SALE"; //It is always SALE for payment request.
                model.PymtMethod = _ghlDirectPaymentSettings.PaymentMethod;
                model.ServiceID = _ghlDirectPaymentSettings.MerchantServiceId;

                var paymentId = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 1000);
                model.PaymentID = paymentId;
                model.OrderNumber = Convert.ToString(order.Id);

                if (_ghlDirectPaymentSettings.PassProductNamesAndTotals)
                {
                    //get the items in the cart
                    int totalQuantity = 0;
                    var cartItems = order.OrderItems;
                    foreach (var item in cartItems)
                    {
                        totalQuantity += item.Quantity;
                    }
                    builder.AppendFormat("Total Items: {0}, Total Quantity: {1}", order.OrderItems.Count, totalQuantity);
                }
                else //Use this as of now
                {
                    //pass order number
                    builder.AppendFormat("Order Number {0}", order.Id);
                }

                model.PaymentDesc = Convert.ToString(builder);

                string returnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentGHLDirect/PDTHandler";
                string cancelReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentGHLDirect/CancelOrder";
                model.MerchantReturnURL = returnUrl;
                model.MerchantCallBackURL = returnUrl;
                model.MerchantUnApprovalURL = cancelReturnUrl;

                model.Amount = Math.Round(order.OrderTotal, 2).ToString("0.00", CultureInfo.InvariantCulture);
                model.CurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
                model.PageTimeout = _ghlDirectPaymentSettings.PageTimeoutForCompeletePaymentProcess <= 0 ? string.Empty : _ghlDirectPaymentSettings.PageTimeoutForCompeletePaymentProcess.ToString();

                model.CustIP = order.CustomerIp;
                model.CustName = order.BillingAddress.FirstName + " " + order.BillingAddress.LastName;
                model.CustEmail = order.BillingAddress.Email;
                model.CustPhone = order.BillingAddress.PhoneNumber;

                ////Hash Key = Password + ServiceID + PaymentID + MerchantReturnURL + Amount + CurrencyCode + CustIP + PageTimeout
                var hashKey = _ghlDirectPaymentSettings.MerchantPassword + model.ServiceID + model.PaymentID + model.MerchantReturnURL + model.MerchantCallBackURL  + model.Amount + _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode + model.CustIP + model.PageTimeout;
                model.HashValue = GetHashValue(hashKey);

                model.MerchantName = !string.IsNullOrEmpty(_ghlDirectPaymentSettings.MerchantName) ? _ghlDirectPaymentSettings.MerchantName : string.Empty;
                model.LanguageCode = "en";

                //Billing Address
                model.BillAddr = _ghlDirectPaymentSettings.EnablePassingBillingAddressToPaymentGateway ? order.BillingAddress.Address1 + (!string.IsNullOrEmpty(order.BillingAddress.Address2) ? ", " + order.BillingAddress.Address2 : string.Empty) : string.Empty;
                model.BillPostal = _ghlDirectPaymentSettings.EnablePassingBillingAddressToPaymentGateway ? order.BillingAddress.ZipPostalCode : string.Empty;
                model.BillCity = _ghlDirectPaymentSettings.EnablePassingBillingAddressToPaymentGateway ? order.BillingAddress.City : string.Empty;
                model.BillRegion = _ghlDirectPaymentSettings.EnablePassingBillingAddressToPaymentGateway ? (order.BillingAddress.StateProvince != null ? order.BillingAddress.StateProvince.Abbreviation : "") : string.Empty;
                model.BillCountry = _ghlDirectPaymentSettings.EnablePassingBillingAddressToPaymentGateway ? (order.BillingAddress.Country != null ? order.BillingAddress.Country.TwoLetterIsoCode : "") : string.Empty;

                //Shipping Address
                model.ShipAddr = _ghlDirectPaymentSettings.EnablePassingShippingAddressToPaymentGateway ? order.ShippingAddress.Address1 + (!string.IsNullOrEmpty(order.ShippingAddress.Address2) ? ", " + order.ShippingAddress.Address2 : string.Empty) : string.Empty;
                model.ShipPostal = _ghlDirectPaymentSettings.EnablePassingShippingAddressToPaymentGateway ? order.ShippingAddress.ZipPostalCode : string.Empty;
                model.ShipCity = _ghlDirectPaymentSettings.EnablePassingShippingAddressToPaymentGateway ? order.ShippingAddress.City : string.Empty;
                model.ShipRegion = _ghlDirectPaymentSettings.EnablePassingShippingAddressToPaymentGateway ? (order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.Abbreviation : "") : string.Empty;
                model.ShipCountry = _ghlDirectPaymentSettings.EnablePassingShippingAddressToPaymentGateway ? (order.ShippingAddress.Country != null ? order.ShippingAddress.Country.TwoLetterIsoCode : "") : string.Empty;
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "Payment for this order is going to initiated with PaymentId="+ model.PaymentID + " And " + "OrderId=" + model.OrderNumber,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }

            //Passing model to view and than view will be submitted to the payment gateway
            return View("~/Plugins/Payments.GHLDirect/Views/PaymentGHLDirect/RedirectToGHLDirect.cshtml", model);
        }
    }
}