using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTOs.Images
{
    public class SliderDto
    {
        private ICollection<ImageDto> _images;

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("system_name")]
        public string SystemName { get; set; }

        [JsonProperty("slider_type")]
        public int SliderType { get; set; }

        [JsonProperty("language_id")]
        public int LanguageId { get; set; }

        [JsonProperty("limited_to_stores")]
        public bool LimitedToStores { get; set; }

        [JsonProperty("images")]
        public ICollection<ImageDto> images
        {
            get { return _images; }
            set { _images = value; }
        }
    }
}
