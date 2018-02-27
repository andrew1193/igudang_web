using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.ShoppingCarts
{
    public class ShoppingCartRootObject : ISerializableObject
    {
        public ShoppingCartRootObject()
        {
            ShoppingCart = new ShoppingCartDto();
        }

        [JsonProperty("shopping_cart")]
        public ShoppingCartDto ShoppingCart { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "shopping_cart";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ShoppingCartDto);
        }
    }
}
