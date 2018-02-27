using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.Serializers;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Plugin.Api.Helpers;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.DTOs.Countries;
using Nop.Plugin.Api.Models.CountriesParameters;

namespace Nop.Plugin.Api.Controllers
{
    public class CountriesController : BaseApiController
    {
        private readonly ICountryApiService _countryApiService;
        private readonly IProductService _productService;
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IDTOHelper _dtoHelper;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private IProductAttributeService _productAttributeService;

        public CountriesController(ICountryApiService countryApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IProductService productService,
            IPictureService pictureService,
            IProductAttributeConverter productAttributeConverter,
            IDTOHelper dtoHelper,
            IStoreContext storeContext,
            IWorkContext workContext,
            IProductAttributeService productAttrbuteService)
            : base(jsonFieldsSerializer,
                 aclService,
                 customerService,
                 storeMappingService,
                 storeService,
                 discountService,
                 customerActivityService,
                 localizationService,
                 pictureService)
        {
            _productService = productService;
            _productAttributeConverter = productAttributeConverter;
            _dtoHelper = dtoHelper;
            _storeContext = storeContext;
            _workContext = workContext;
            _productAttributeService = productAttrbuteService;
            _countryApiService = countryApiService;
        }


        /// <summary>
        /// Receive a list of all countries
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(CountriesRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetCountries(CountriesParametersModel parameters)
        {
            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            IList<Country> countries = _countryApiService.GetCountries(createdAtMin: parameters.CreatedAtMin,
                                                                                                         createdAtMax: parameters.CreatedAtMax,
                                                                                                         updatedAtMin: parameters.UpdatedAtMin,
                                                                                                         updatedAtMax: parameters.UpdatedAtMax,
                                                                                                         limit: int.MaxValue,
                                                                                                         page: parameters.Page);

            //Filter by only published
            countries = countries.Where(x => x.Published == true).ToList();

            List<CountryDto> countriesDtos = countries.Select(country =>
            {
                return _dtoHelper.PrepareCountryDTO(country);
            }).ToList();

            var coutriesRootObject = new CountriesRootObject()
            {
                Countries = countriesDtos
            };

            var json = _jsonFieldsSerializer.Serialize(coutriesRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }


    }
}

