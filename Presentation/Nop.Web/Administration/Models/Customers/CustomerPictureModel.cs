using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Customers;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Customers
{
    public partial class CustomerPictureModel : BaseNopEntityModel
    {
        //picture thumbnail
        [NopResourceDisplayName("Admin.Customer.CustomerPicture.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }

        [NopResourceDisplayName("Admin.Customer.CustomerPicture.Fields.CustomerName")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.Customer.CustomerPicture.Fields.Id")]
        public int Id { get; set; }

        [NopResourceDisplayName("Admin.Customer.CustomerPicture.Fields.UploadDateTime")]
        public DateTime UploadDateTime { get; set; }

        [NopResourceDisplayName("Admin.Customer.CustomerPicture.Fields.Published")]
        public bool Published { get; set; }
    }
}