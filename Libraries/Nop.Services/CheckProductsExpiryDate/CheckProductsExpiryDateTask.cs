using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Tasks;
using Nop.Services.Vendors;
using Nop.Services.Helpers;

namespace Nop.Services.CheckProductsExpiryDate
{
    public partial class CheckProductsExpiryDateTask : ITask
    {

        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IEventService _eventService;
        private readonly IDateTimeHelper _datetimeHelper;


        public CheckProductsExpiryDateTask(IProductService productService,
            IVendorService vendorService,
            IEventService eventService,
            IDateTimeHelper datetimeHelper)
        {
            this._productService = productService;
            this._vendorService = vendorService;
            this._eventService = eventService;
            this._datetimeHelper = datetimeHelper;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            try
            {
                var products = _productService.GetProducts();
                foreach (var product in products)
                {
                    CheckValidity(product);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void CheckValidity(Product product)
        {

            var vendor = _vendorService.GetVendorById(product.VendorId);
            if (vendor == null)
            {
                return;
            }
            var ev = _eventService.GetEventById(vendor.EventId);
            if (ev == null)
            {
                return;
            }

            DateTime now = _datetimeHelper.ConvertToUtcTime(DateTime.Now, DateTimeKind.Local);

            if (ev.StartedOnUtc>now || ev.EndedOnUtc<now)
            {
                product.Published = false;
            }
            //else
            //{
            //    product.Published = true;
            //}
            _productService.UpdateProduct(product);

        }

    }
}
