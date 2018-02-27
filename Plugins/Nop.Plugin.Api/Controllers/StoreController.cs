using System;
using System.Globalization;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTOs.Stores;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.Serializers;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using static System.Decimal;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.DTOs.Images;
using Nop.Web.Framework.Controllers;
using Nop.Services.Configuration;
using Nop.Plugin.Widgets.NivoSlider;
using System.Collections.Generic;
using Nop.Plugin.Api.Models.StoresParameters;
using Nop.Plugin.Api.Services;

namespace Nop.Plugin.Api.Controllers
{
    //[BearerTokenAuthorize]
    public class StoreController : BaseApiController
    {
        private IStoreContext _storeContext;
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IDTOHelper _dtoHelper;
        private readonly IWorkContext _workContext;
        //private readonly BaseController _baseController;
        private ISettingService _settingService;
        private readonly IStoreApiService _storeApiService;

        public StoreController(IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IStoreContext storeContext,
            CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IDTOHelper dtoHelper,
            IWorkContext workContext,
            //BaseController baseController,
            ISettingService settingService,
            IStoreApiService storeApiService)
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
            _storeContext = storeContext;
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _dtoHelper = dtoHelper;
            _workContext = workContext;
            //_baseController = baseController;
            _settingService = settingService;
            _storeApiService = storeApiService;
        }

        /// <summary>
        /// Retrieve category by spcified id
        /// </summary>
        /// <param name="fields">Fields from the category you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(StoresRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetCurrentStore(string fields = "")
        {
            Store store = _storeContext.CurrentStore;

            if (store == null)
            {
                return Error(HttpStatusCode.NotFound, "store", "store not found");
            }

            StoreDto storeDto = store.ToDto();

            Currency primaryCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);

            if (!String.IsNullOrEmpty(primaryCurrency.DisplayLocale))
            {
                storeDto.PrimaryCurrencyDisplayLocale = primaryCurrency.DisplayLocale;
            }

            var storesRootObject = new StoresRootObject();

            storesRootObject.Stores.Add(storeDto);

            var json = _jsonFieldsSerializer.Serialize(storesRootObject, fields);

            return new RawJsonActionResult(json);
        }


        [HttpGet]
        [ResponseType(typeof(BannersRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetBannerImages()
        {
            //var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var nivoSliderSettings = _settingService.LoadSetting<NivoSliderSettings>(1);

            var banners = new List<BannerDto>();
            if (nivoSliderSettings.Picture1Id > 0)
            {
                var pic = _pictureService.GetPictureById(nivoSliderSettings.Picture1Id);
                var dto = _dtoHelper.PrepareBannerImageDto(pic);
                var bandto = new BannerDto();
                bandto.imageDto = dto;
                bandto.Text = nivoSliderSettings.Text1;
                bandto.Link = nivoSliderSettings.Link1;
                banners.Add(bandto);
            }

            if (nivoSliderSettings.Picture2Id > 0)
            {
                var pic = _pictureService.GetPictureById(nivoSliderSettings.Picture2Id);
                var dto = _dtoHelper.PrepareBannerImageDto(pic);
                var bandto = new BannerDto();
                bandto.imageDto = dto;
                bandto.Text = nivoSliderSettings.Text2;
                bandto.Link = nivoSliderSettings.Link2;
                banners.Add(bandto);
            }

            if (nivoSliderSettings.Picture3Id > 0)
            {
                var pic = _pictureService.GetPictureById(nivoSliderSettings.Picture3Id);
                var dto = _dtoHelper.PrepareBannerImageDto(pic);
                var bandto = new BannerDto();
                bandto.imageDto = dto;
                bandto.Text = nivoSliderSettings.Text3;
                bandto.Link = nivoSliderSettings.Link3;
                banners.Add(bandto);
            }

            if (nivoSliderSettings.Picture4Id > 0)
            {
                var pic = _pictureService.GetPictureById(nivoSliderSettings.Picture4Id);
                var dto = _dtoHelper.PrepareBannerImageDto(pic);
                var bandto = new BannerDto();
                bandto.imageDto = dto;
                bandto.Text = nivoSliderSettings.Text4;
                bandto.Link = nivoSliderSettings.Link4;
                banners.Add(bandto);
            }

            if (nivoSliderSettings.Picture5Id > 0)
            {
                var pic = _pictureService.GetPictureById(nivoSliderSettings.Picture5Id);
                var dto = _dtoHelper.PrepareBannerImageDto(pic);
                var bandto = new BannerDto();
                bandto.imageDto = dto;
                bandto.Text = nivoSliderSettings.Text5;
                bandto.Link = nivoSliderSettings.Link5;
                banners.Add(bandto);
            }

            if (nivoSliderSettings.RightPicture1Id > 0)
            {
                var pic = _pictureService.GetPictureById(nivoSliderSettings.RightPicture1Id);
                var dto = _dtoHelper.PrepareBannerImageDto(pic);
                var bandto = new BannerDto();
                bandto.imageDto = dto;
                bandto.Text = nivoSliderSettings.RightText1;
                bandto.Link = nivoSliderSettings.RightLink1;
                banners.Add(bandto);
            }

            if (nivoSliderSettings.RightPicture2Id > 0)
            {
                var pic = _pictureService.GetPictureById(nivoSliderSettings.RightPicture2Id);
                var dto = _dtoHelper.PrepareBannerImageDto(pic);
                var bandto = new BannerDto();
                bandto.imageDto = dto;
                bandto.Text = nivoSliderSettings.RightText2;
                bandto.Link = nivoSliderSettings.RightLink2;
                banners.Add(bandto);
            }

            IList<BannerDto> bannerAsDtos = new List<BannerDto>();

            if (banners.Count > 0)
            {
                bannerAsDtos = banners;
            }

            var bannersRootObject = new BannersRootObject()
            {
                Banners = bannerAsDtos
            };

            var json = _jsonFieldsSerializer.Serialize(bannersRootObject, "");

            return new RawJsonActionResult(json);
        }


        [HttpGet]
        [ResponseType(typeof(SlidersRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetAnywaySliderImages(int sliderId)
        {
            try
            {
                if (sliderId <= 0)
                {
                    return Error(HttpStatusCode.BadRequest, "slider id", "Invalid slider id");
                }

                var slider = _storeApiService.GetSliderById(sliderId);

                if (slider == null)
                {
                    return Error(HttpStatusCode.NotFound, "slider", "Slider not found");
                }

                var sliderImages = _storeApiService.GetImagesBySliderId(sliderId);

                IList<SliderDto> sliderAsDtos = new List<SliderDto>();

                if (slider != null)
                {
                    var sliderList = new List<SliderDto>();
                    var tempSlider = _dtoHelper.PrepareSliderDto(slider, sliderImages);
                    sliderList.Add(tempSlider);
                    sliderAsDtos = sliderList;
                }

                var slidersRootObject = new SlidersRootObject()
                {
                    Sliders = sliderAsDtos
                };

                var json = _jsonFieldsSerializer.Serialize(slidersRootObject, "");

                return new RawJsonActionResult(json);
            }
            catch (Exception ex)
            {
                return Error(HttpStatusCode.BadRequest, "error", ex.Message);
            }
        }
    }
}
