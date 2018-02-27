using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Countries
{
    public class CountriesRootObject : ISerializableObject
    {
        public CountriesRootObject()
        {
            Countries = new List<CountryDto>();
        }

        [JsonProperty("countries")]
        public IList<CountryDto> Countries { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "countries";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(CountryDto);
        }
    }
}
