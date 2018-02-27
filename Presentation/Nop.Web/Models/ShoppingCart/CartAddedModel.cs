using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.ShoppingCart
{
    public partial class CartAddedModel
    {
        //public CartAddedModel()
        //{
        //    Product = new Product();
        //    ProductAttributeSelected = new List<string>();
        //}
        public int cartTypeId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public string StrPrice { get; set; }

        public string PriceValue { get; set; }

        public string StrOldPrice { get; set; }

        public string ProductPictureUrl { get; set; }

        //public string StrSubTotal { get; set; }

        //public string StrTotal { get; set; }

        //public Product Product { get; set; }

        public List<string> ProductAttributeSelected { get; set; }

        public MyShoppingCartOrWishlist ShoppingCartOrWishlist { get; set; }
    }

    public partial class MyShoppingCartOrWishlist
    {
        public int ItemCount { get; set; }

        public string SubTotal { get; set; }

        public string Total { get; set; }
    }
}