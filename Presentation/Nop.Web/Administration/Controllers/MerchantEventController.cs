using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Helpers;
using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Events;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Events;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Controllers
{
    public partial class MerchantEventController : BaseAdminController
    {
        #region Fields

        private readonly IEventService _eventService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IVendorService _vendorService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Constructors

        public MerchantEventController(IEventService eventService,
            IManufacturerService manufacturerService, IProductService productService, 
            ICustomerService customerService,
            IUrlRecordService urlRecordService, 
            IPictureService pictureService, 
            ILanguageService languageService,
            ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService,
            IDiscountService discountService,
            IPermissionService permissionService,
            IAclService aclService, 
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IVendorService vendorService, 
            ICustomerActivityService customerActivityService,
            CatalogSettings catalogSettings,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IDateTimeHelper dateTimeHelper)
        {
            this._eventService = eventService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._discountService = discountService;
            this._permissionService = permissionService;
            this._vendorService = vendorService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._customerActivityService = customerActivityService;
            this._catalogSettings = catalogSettings;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual EventModel PrepareEventModelForList(Event ev)
        {
            return new EventModel
            {
                Id = ev.Id,
                Name = ev.Name,
                AvailableStartDateTimeUtc = _dateTimeHelper.ConvertToUserTime(ev.StartedOnUtc, DateTimeKind.Utc),
                AvailableEndDateTimeUtc = _dateTimeHelper.ConvertToUserTime(ev.EndedOnUtc, DateTimeKind.Utc),
                AlreadyJoined = ev.Id == _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId).EventId
            };
        }

        [HttpPost]
        public virtual ActionResult JoinEvent(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            var vendor = _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId);
            vendor.EventId = id;
            _vendorService.UpdateVendor(vendor);

            return Json(new { result = true });
        }

        //[NonAction]
        //protected virtual void UpdatePictureSeoNames(Event ev)
        //{
        //    var picture = _pictureService.GetPictureById(ev.PictureId);
        //    if (picture != null)
        //        _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(ev.Name));
        //}

        [NonAction]
        protected virtual void PrepareAllEventsModel(EventModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableEvents.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Events.Events.Fields.Parent.None"),
                Value = "0"
            });
            var events = SelectListHelper.GetEventList(_eventService, _cacheManager, true);
            foreach (var c in events)
                model.AvailableEvents.Add(c);

        }
        

        [NonAction]
        protected virtual void PrepareStoresMappingModel(EventModel model, Event ev, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && ev != null)
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(ev).ToList();

            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString(),
                    Selected = model.SelectedStoreIds.Contains(store.Id)
                });
            }
        }

        [NonAction]
        protected virtual void SaveStoreMappings(Event ev, EventModel model)
        {
            ev.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = _storeMappingService.GetStoreMappings(ev);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        _storeMappingService.InsertStoreMapping(ev, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        _storeMappingService.DeleteStoreMapping(storeMappingToDelete);
                }
            }
        }

        #endregion
        
        #region List

        public virtual ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            var model = new EventListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult List(DataSourceRequest command, EventListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedKendoGridJson();

            var events = _eventService.GetAllEvents(model.SearchEventName, 
                model.SearchStoreId, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = events.Select(PrepareEventModelForList),
                Total = events.TotalCount
            };
            //var gridModel = new DataSourceResult
            //{
            //    Data = events.Select(x =>
            //    {
            //        var eventModel = x.ToModel();
            //        return eventModel;
            //    }),
            //    Total = events.TotalCount
            //};
            return Json(gridModel);
        }
        
        #endregion

        #region Create / Edit / Delete

        public virtual ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            var model = new EventModel();
            
            //events
            PrepareAllEventsModel(model);
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.PageSize = _catalogSettings.DefaultCategoryPageSize;
            model.PageSizeOptions = _catalogSettings.DefaultCategoryPageSizeOptions;
            model.Published = true;
            model.AllowCustomersToSelectPageSize = true;            

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Create(EventModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var ev = model.ToEntity();
                ev.StartedOnUtc = model.AvailableStartDateTimeUtc;
                ev.EndedOnUtc = model.AvailableEndDateTimeUtc;
                ev.CreatedOnUtc = DateTime.UtcNow;
                ev.UpdatedOnUtc = DateTime.UtcNow;
                _eventService.InsertEvent(ev);
              
                //update picture seo file name
                //UpdatePictureSeoNames(ev);
                
                //Stores
                SaveStoreMappings(ev, model);

                //activity log
                _customerActivityService.InsertActivity("AddNewEvent", _localizationService.GetResource("ActivityLog.AddNewEvent"), ev.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Events.Events.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = ev.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
           
            //events
            PrepareAllEventsModel(model);
            //Stores
            PrepareStoresMappingModel(model, null, true);
            return View(model);
        }

        public virtual ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            var ev = _eventService.GetEventById(id);
            if (ev == null || ev.Deleted) 
                //No event found with the specified id
                return RedirectToAction("List");

            var model = ev.ToModel();
            
            //events
            PrepareAllEventsModel(model);
            //Stores
            PrepareStoresMappingModel(model, ev, false);

            model.AvailableStartDateTimeUtc = ev.StartedOnUtc;
            model.AvailableEndDateTimeUtc = ev.EndedOnUtc;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual ActionResult Edit(EventModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            var ev = _eventService.GetEventById(model.Id);
            if (ev == null || ev.Deleted)
                //No event found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //int prevPictureId = ev.PictureId;
                ev = model.ToEntity(ev);
                ev.UpdatedOnUtc = DateTime.UtcNow;
                _eventService.UpdateEvent(ev);
               
                _eventService.UpdateEvent(ev);
                //delete an old picture (if deleted or updated)
                //if (prevPictureId > 0 && prevPictureId != ev.PictureId)
                //{
                //    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                //    if (prevPicture != null)
                //        _pictureService.DeletePicture(prevPicture);
                //}
                //update picture seo file name
                //UpdatePictureSeoNames(ev);
            
                //Stores
                SaveStoreMappings(ev, model);

                //activity log
                _customerActivityService.InsertActivity("EditEvent", _localizationService.GetResource("ActivityLog.EditEvent"), ev.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Events.Events.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new {id = ev.Id});
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
           
            //events
            PrepareAllEventsModel(model);
           
            //Stores
            PrepareStoresMappingModel(model, ev, true);

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            var ev = _eventService.GetEventById(id);
            if (ev == null)
                //No ev found with the specified id
                return RedirectToAction("List");

            _eventService.DeleteEvent(ev);

            //activity log
            _customerActivityService.InsertActivity("DeleteEvent", _localizationService.GetResource("ActivityLog.DeleteEvent"), ev.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Events.Events.Deleted"));
            return RedirectToAction("List");
        }
        

        #endregion

        #region Products

        [HttpPost]
        public virtual ActionResult ProductList(DataSourceRequest command, int eventId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedKendoGridJson();

            var productEvents = _eventService.GetProductEventsByEventId(eventId,
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = productEvents.Select(x => new EventModel.EventProductModel
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    ProductId = x.ProductId,
                    ProductName = _productService.GetProductById(x.ProductId).Name,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = productEvents.TotalCount
            };

            return Json(gridModel);
        }

        public virtual ActionResult ProductUpdate(EventModel.EventProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            var productEvent = _eventService.GetProductEventById(model.Id);
            if (productEvent == null)
                throw new ArgumentException("No product event mapping found with the specified id");

            productEvent.IsFeaturedProduct = model.IsFeaturedProduct;
            productEvent.DisplayOrder = model.DisplayOrder;
            _eventService.UpdateProductEvent(productEvent);

            return new NullJsonResult();
        }

        public virtual ActionResult ProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            var productEvent = _eventService.GetProductEventById(id);
            if (productEvent == null)
                throw new ArgumentException("No product event mapping found with the specified id");

            //var eventId = productEvent.EventId;
            _eventService.DeleteProductEvent(productEvent);

            return new NullJsonResult();
        }

        public virtual ActionResult ProductAddPopup(int eventId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();
            
            var model = new EventModel.AddEventProductModel();
            //events
            model.AvailableEvents.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var events = SelectListHelper.GetEventList(_eventService, _cacheManager, true);
            foreach (var c in events)
                model.AvailableEvents.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var manufacturers = SelectListHelper.GetManufacturerList(_manufacturerService, _cacheManager, true);
            foreach (var m in manufacturers)
                model.AvailableManufacturers.Add(m);

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var vendors = SelectListHelper.GetVendorList(_vendorService, _cacheManager, true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(v);

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult ProductAddPopupList(DataSourceRequest command, EventModel.AddEventProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedKendoGridJson();

            var gridModel = new DataSourceResult();
            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchEventId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            gridModel.Data = products.Select(x => x.ToModel());
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }
        
        [HttpPost]
        [FormValueRequired("save")]
        public virtual ActionResult ProductAddPopup(string btnId, string formId, EventModel.AddEventProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMerchantEvents))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingProductEvents = _eventService.GetProductEventsByEventId(model.EventId, showHidden: true);
                        if (existingProductEvents.FindProductEvent(id, model.EventId) == null)
                        {
                            _eventService.InsertProductEvent(
                                new ProductEvent
                                {
                                    EventId = model.EventId,
                                    ProductId = id,
                                    IsFeaturedProduct = false,
                                    DisplayOrder = 1
                                });
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion
    }
}
