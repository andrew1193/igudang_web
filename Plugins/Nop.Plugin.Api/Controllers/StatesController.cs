using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.Serializers;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Plugin.Api.Helpers;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.DTOs.States;
using Nop.Plugin.Api.Models.StatesParameters;

namespace Nop.Plugin.Api.Controllers
{
    public class StatesController : BaseApiController
    {
        private readonly IStateApiService _stateApiService;
        private readonly IProductService _productService;
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IDTOHelper _dtoHelper;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private IProductAttributeService _productAttributeService;


        public StatesController(IStateApiService stateApiService,
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
            _stateApiService = stateApiService;
        }


        /// <summary>
        /// Receive a list of all states
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(StatesRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetStates(StatesParametersModel parameters)
        {
            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            IList<StateProvince> states = _stateApiService.GetStates(limit: int.MaxValue,
                                                                       page: parameters.Page);

            List<StateDto> statesDtos = states.Select(state =>
            {
                return _dtoHelper.PrepareStateDTO(state);
            }).ToList();

            var coutriesRootObject = new StatesRootObject()
            {
                States = statesDtos
            };

            var json = _jsonFieldsSerializer.Serialize(coutriesRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        /// Receive a list of all states by country id
        /// </summary>
        /// <param name="countryId">Id of the country which states you want to get</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(StatesRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetStatesByCountryId(int countryId, StatesForCountryParametersModel parameters)
        {
            if (countryId <= Configurations.DefaultCountryId)
            {
                return Error(HttpStatusCode.BadRequest, "country_id", "invalid country_id");
            }

            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            IList<StateProvince> states = _stateApiService.GetStates(countryId, int.MaxValue,
                parameters.Page);

            if (states == null)
            {
                return Error(HttpStatusCode.NotFound, "state", "not found");
            }

            List<StateDto> statesDtos = states.Select(state =>
            {
                return _dtoHelper.PrepareStateDTO(state);
            }).ToList();

            var statesRootObject = new StatesRootObject()
            {
                States = statesDtos
            };

            var json = _jsonFieldsSerializer.Serialize(statesRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }


    }
}

