using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTOs.Events;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.Serializers;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Nop.Plugin.Api.Controllers
{
    public class EventsController : BaseApiController
    {
        private readonly IEventService _eventService;
        private readonly IDTOHelper _dtoHelper;

        public EventsController(IEventService eventService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IDTOHelper dtoHelper) : base(jsonFieldsSerializer,
                 aclService,
                 customerService,
                 storeMappingService,
                 storeService,
                 discountService,
                 customerActivityService,
                 localizationService,
                 pictureService)
        {
            _eventService = eventService;
            _dtoHelper = dtoHelper;
        }

        [HttpGet]
        [ResponseType(typeof(EventRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetCurrentEvent()
        {
            var currentEvent = _eventService.GetCurrentEvent();

            EventDto eventAsDto = _dtoHelper.PrepareEventDTO(currentEvent);

            var eventRootObject = new EventRootObject()
            {
                Event = eventAsDto
            };

            var json = _jsonFieldsSerializer.Serialize(eventRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }
    }
}
