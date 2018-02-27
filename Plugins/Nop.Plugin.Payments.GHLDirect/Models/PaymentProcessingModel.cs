using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.GHLDirect.Models
{
    public class PaymentProcessingModel : BaseNopModel
    {
        [AllowHtml]
        public string GHLDirectAPIUrl { get; set; }
        [AllowHtml]
        public string TransactionType { get; set; }
        [AllowHtml]
        public string PymtMethod { get; set; }
        [AllowHtml]
        public string ServiceID { get; set; }
        [AllowHtml]
        public string PaymentID { get; set; }
        [AllowHtml]
        public string OrderNumber { get; set; }
        [AllowHtml]
        public string PaymentDesc { get; set; }
        [AllowHtml]
        public string MerchantReturnURL { get; set; }
        [AllowHtml]
        public string MerchantCallBackURL { get; set; }

        [AllowHtml]
        public string MerchantUnApprovalURL { get; set; }
        [AllowHtml]
        public string Amount { get; set; }
        [AllowHtml]
        public string CurrencyCode { get; set; }
        [AllowHtml]
        public string PageTimeout { get; set; }
        [AllowHtml]
        public string CustIP { get; set; }
        [AllowHtml]
        public string CustName { get; set; }
        [AllowHtml]
        public string CustEmail { get; set; }
        [AllowHtml]
        public string CustPhone { get; set; }
        [AllowHtml]
        public string HashValue { get; set; }
        [AllowHtml]
        public string MerchantName { get; set; }
        [AllowHtml]
        public string LanguageCode { get; set; }

        //Billing Address
        [AllowHtml]
        public string BillAddr { get; set; }
        [AllowHtml]
        public string BillPostal { get; set; }
        [AllowHtml]
        public string BillCity { get; set; }
        [AllowHtml]
        public string BillRegion { get; set; }
        [AllowHtml]
        public string BillCountry { get; set; }

        //Shipping Address
        [AllowHtml]
        public string ShipAddr { get; set; }
        [AllowHtml]
        public string ShipPostal { get; set; }
        [AllowHtml]
        public string ShipCity { get; set; }
        [AllowHtml]
        public string ShipRegion { get; set; }
        [AllowHtml]
        public string ShipCountry { get; set; }
    }
}