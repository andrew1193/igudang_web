using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Images
{
    public class BannerDto
    {
        [JsonProperty("image")]
        public ImageDto imageDto { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
