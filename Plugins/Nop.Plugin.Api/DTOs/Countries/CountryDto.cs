using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Countries
{
    [JsonObject(Title = "countries")]
    public class CountryDto
    {
        /// <summary>
        /// Gets or sets the id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("allows-billing")]
        public bool AllowsBilling { get; set; }

        [JsonProperty("allows-shipping")]
        public bool AllowsShipping { get; set; }

        [JsonProperty("two-letter-iso-code")]
        public string TwoLetterIsoCode { get; set; }

        [JsonProperty("three-letter-iso-code")]
        public string ThreeLetterIsoCode { get; set; }

        [JsonProperty("numeric-iso-code")]
        public int NumericIsoCode { get; set; }

        [JsonProperty("subject-to-vat")]
        public bool SubjectToVat { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("display-order")]
        public int DisplayOrder { get; set; }

        [JsonProperty("limited-to-stores")]
        public bool LimitedToStores { get; set; }

    }
}
