using System;
using Newtonsoft.Json;
using Nop.Plugin.Api.Constants;

namespace Nop.Plugin.Api.Models.StatesParameters
{
    // JsonProperty is used only for swagger
    public class BaseStatesParametersModel
    {
        public BaseStatesParametersModel()
        {
            Limit = Configurations.DefaultLimit;
            Page = Configurations.DefaultPageValue;
            Fields = string.Empty;
        }


        /// <summary>
        /// Amount of results (default: 50) (maximum: 250)
        /// </summary>
        [JsonProperty("limit")]
        public int Limit { get; set; }

        /// <summary>
        /// Page to show (default: 1)
        /// </summary>
        [JsonProperty("page")]
        public int Page { get; set; }

        /// <summary>
        /// comma-separated list of fields to include in the response
        /// </summary>
        [JsonProperty("fields")]
        public string Fields { get; set; }
    }
}