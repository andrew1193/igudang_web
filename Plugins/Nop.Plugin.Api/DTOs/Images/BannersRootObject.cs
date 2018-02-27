using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Images
{
    public class BannersRootObject : ISerializableObject
    {
        public BannersRootObject()
        {
            Banners = new List<BannerDto>();
        }

        [JsonProperty("banners")]
        public IList<BannerDto> Banners { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "banners";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(BannerDto);
        }
    }
}
