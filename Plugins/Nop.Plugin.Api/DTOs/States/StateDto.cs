using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.Countries;

namespace Nop.Plugin.Api.DTOs.States
{
    [JsonObject(Title = "states")]
    public class StateDto
    {
        /// <summary>
        /// Gets or sets the id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country-id")]
        public string CountryId { get; set; }

        /// <summary>
        /// Gets or sets the country
        /// </summary>
        [JsonProperty("country")]
        public CountryDto CountryDto { get; set; }

        [JsonProperty("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("display-order")]
        public bool DisplayOrder { get; set; }
    }
}
