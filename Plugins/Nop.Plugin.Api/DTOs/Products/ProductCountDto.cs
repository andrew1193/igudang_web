using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Products
{
    [JsonObject(Title = "counts")]
    public class ProductCountDto
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
