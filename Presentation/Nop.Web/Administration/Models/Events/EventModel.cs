using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Events;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Events
{
    [Validator(typeof(EventValidator))]
    public partial class EventModel : BaseNopEntityModel
    {
        public EventModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            AvailableEvents = new List<SelectListItem>();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Events.Events.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.PageSize")]
        public int PageSize { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.AvailableStartDateTime")]
        [UIHint("DateTimeNullable")]
        public DateTime AvailableStartDateTimeUtc { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.AvailableEndDateTimeUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime AvailableEndDateTimeUtc { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Events.Events.Fields.Picture")]
        public int BackgroundPictureId { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.Fields.AlreadyJoined")]
        public bool AlreadyJoined { get; set; }

        //ACL (customer roles)
        [NopResourceDisplayName("Admin.Events.Events.Fields.AclCustomerRoles")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
        
        //store mapping
        [NopResourceDisplayName("Admin.Events.Events.Fields.LimitedToStores")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }


        public IList<SelectListItem> AvailableEvents { get; set; }

        public string isOtherActive { get; set; }


        #region Nested classes

        public partial class EventProductModel : BaseNopEntityModel
        {
            public int EventId { get; set; }

            public int ProductId { get; set; }

            [NopResourceDisplayName("Admin.Events.Events.Products.Fields.Product")]
            public string ProductName { get; set; }

            [NopResourceDisplayName("Admin.Events.Events.Products.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [NopResourceDisplayName("Admin.Events.Events.Products.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public partial class AddEventProductModel : BaseNopModel
        {
            public AddEventProductModel()
            {
                AvailableEvents = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableStores = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.Events.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }
            [NopResourceDisplayName("Admin.Events.Products.List.SearchEvent")]
            public int SearchEventId { get; set; }
            [NopResourceDisplayName("Admin.Events.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }
            [NopResourceDisplayName("Admin.Events.Products.List.SearchStore")]
            public int SearchStoreId { get; set; }
            [NopResourceDisplayName("Admin.Events.Products.List.SearchVendor")]
            public int SearchVendorId { get; set; }
            [NopResourceDisplayName("Admin.Events.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableEvents { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableStores { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public int EventId { get; set; }

            public int[] SelectedProductIds { get; set; }
        }

        #endregion
    }

}