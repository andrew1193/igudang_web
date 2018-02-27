using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.ShoppingCartsParameters;
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
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Factories;
using static Nop.Plugin.Api.Controllers.ShoppingCartItemsController;
using Nop.Services.Common;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Services.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Services.Payments;
using Nop.Services.Tax;
using Nop.Services.Shipping;

namespace Nop.Plugin.Api.Controllers
{
    //[BearerTokenAuthorize]
    public class ShoppingCartItemsController : BaseApiController
    {
        private readonly IShoppingCartItemApiService _shoppingCartItemApiService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IFactory<ShoppingCartItem> _factory;
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IDTOHelper _dtoHelper;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPaymentService _paymentService;
        private readonly ITaxService _taxService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly IShippingService _shippingService;

        private TaxSettings _taxSettings;
        private TaxSettings TaxSettings
        {
            get
            {
                if (_taxSettings == null)
                {
                    _taxSettings = EngineContext.Current.Resolve<TaxSettings>();
                }

                return _taxSettings;
            }
        }

        private RewardPointsSettings _rewardPointsSettings;
        private RewardPointsSettings RewardPointsSettings
        {
            get
            {
                if (_rewardPointsSettings == null)
                {
                    _rewardPointsSettings = EngineContext.Current.Resolve<RewardPointsSettings>();
                }
                return _rewardPointsSettings;
            }
        }

        public ShoppingCartItemsController(IShoppingCartItemApiService shoppingCartItemApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IFactory<ShoppingCartItem> factory,
            IPictureService pictureService,
            IProductAttributeConverter productAttributeConverter,
            IDTOHelper dtoHelper,
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IGenericAttributeService genericAttributeService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IPaymentService paymentService,
            ITaxService taxService,
            ICheckoutModelFactory checkoutModelFactory,
            IShippingService shippingService)
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
            _shoppingCartItemApiService = shoppingCartItemApiService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _factory = factory;
            _productAttributeConverter = productAttributeConverter;
            _dtoHelper = dtoHelper;
            _storeContext = storeContext;
            _workContext = workContext;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _genericAttributeService = genericAttributeService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _paymentService = paymentService;
            _taxService = taxService;
            _checkoutModelFactory = checkoutModelFactory;
            _shippingService = shippingService;
        }

        /// <summary>
        /// Receive a list of all shopping cart items
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(ShoppingCartItemsRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetShoppingCartItems(ShoppingCartItemsParametersModel parameters)
        {
            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            IList<ShoppingCartItem> shoppingCartItems = _shoppingCartItemApiService.GetShoppingCartItems(customerId: null,
                                                                                                         createdAtMin: parameters.CreatedAtMin,
                                                                                                         createdAtMax: parameters.CreatedAtMax,
                                                                                                         updatedAtMin: parameters.UpdatedAtMin,
                                                                                                         updatedAtMax: parameters.UpdatedAtMax,
                                                                                                         limit: int.MaxValue,
                                                                                                         page: parameters.Page);

            List<ShoppingCartItemDto> shoppingCartItemsDtos = shoppingCartItems.Select(shoppingCartItem =>
            {
                return _dtoHelper.PrepareShoppingCartItemDTO(shoppingCartItem);
            }).ToList();

            var shoppingCartsRootObject = new ShoppingCartItemsRootObject()
            {
                ShoppingCartItems = shoppingCartItemsDtos
            };

            var json = _jsonFieldsSerializer.Serialize(shoppingCartsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        /// Receive a list of all shopping cart items by customer id
        /// </summary>
        /// <param name="customerId">Id of the customer whoes shopping cart items you want to get</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ResponseType(typeof(ShoppingCartItemsRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetShoppingCartItemsByCustomerId(int customerId, ShoppingCartItemsForCustomerParametersModel parameters)
        {
            if (customerId <= Configurations.DefaultCustomerId)
            {
                return Error(HttpStatusCode.BadRequest, "customer_id", "invalid customer_id");
            }

            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            IList<ShoppingCartItem> shoppingCartItems = _shoppingCartItemApiService.GetShoppingCartItems(customerId,
                                                                                                         parameters.CreatedAtMin,
                                                                                                         parameters.CreatedAtMax, parameters.UpdatedAtMin,
                                                                                                         parameters.UpdatedAtMax, int.MaxValue,
                                                                                                         parameters.Page);

            if (shoppingCartItems == null)
            {
                shoppingCartItems = new List<ShoppingCartItem>();
            }

            List<ShoppingCartItemDto> shoppingCartItemsDtos = shoppingCartItems.Select(shoppingCartItem =>
            {
                return _dtoHelper.PrepareShoppingCartItemDTO(shoppingCartItem);
            }).ToList();

            var shoppingCartsRootObject = new ShoppingCartItemsRootObject()
            {
                ShoppingCartItems = shoppingCartItemsDtos
            };

            var json = _jsonFieldsSerializer.Serialize(shoppingCartsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [ResponseType(typeof(ShoppingCartItemsRootObject))]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult GetWishlistItemsByCustomerId(ShoppingCartItemsForCustomerParametersModel parameters)
        {
            if (parameters.CustomerId <= Configurations.DefaultCustomerId)
            {
                return Error(HttpStatusCode.BadRequest, "customer_id", "invalid customer_id");
            }

            //if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            //{
            //    return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            //}

            //if (parameters.Page < Configurations.DefaultPageValue)
            //{
            //    return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            //}

            //IList<ShoppingCartItem> shoppingCartItems = _shoppingCartItemApiService.GetShoppingCartItems(customerId,
            //                                                                                             parameters.CreatedAtMin,
            //                                                                                             parameters.CreatedAtMax, parameters.UpdatedAtMin,
            //                                                                                             parameters.UpdatedAtMax, parameters.Limit,
            //                                                                                             parameters.Page);

            IList<ShoppingCartItem> shoppingCartItems = _shoppingCartItemApiService.GetWishlistItems(customerId: parameters.CustomerId, cartTypeId: 2);

            if (shoppingCartItems == null)
            {
                shoppingCartItems = new List<ShoppingCartItem>();
            }

            List<ShoppingCartItemDto> shoppingCartItemsDtos = shoppingCartItems.Select(shoppingCartItem =>
            {
                return _dtoHelper.PrepareShoppingCartItemDTO(shoppingCartItem);
            }).ToList();

            var shoppingCartsRootObject = new ShoppingCartItemsRootObject()
            {
                ShoppingCartItems = shoppingCartItemsDtos
            };

            var json = _jsonFieldsSerializer.Serialize(shoppingCartsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [ResponseType(typeof(ShoppingCartItemsRootObject))]
        public IHttpActionResult CreateShoppingCartItem([ModelBinder(typeof(JsonModelBinder<ShoppingCartItemDto>))] Delta<ShoppingCartItemDto> shoppingCartItemDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            string key = shoppingCartItemDelta.Dto.CustomerId.ToString() + "_" + shoppingCartItemDelta.Dto.Quantity.ToString() + "_" + shoppingCartItemDelta.Dto.ShoppingCartType + "_" + shoppingCartItemDelta.Dto.ProductId.ToString() + "_aDd*TO#cArt99315";
            var strMd5 = CommonHelper.CalculateMD5Hash(key);
            if (strMd5 != shoppingCartItemDelta.Dto.ApiKey)
            {
                return Error(HttpStatusCode.BadRequest, "error", "key not match");
            }

            ShoppingCartItem newShoppingCartItem = _factory.Initialize();
            shoppingCartItemDelta.Merge(newShoppingCartItem);

            // We know that the product id and customer id will be provided because they are required by the validator.
            // TODO: validate
            Product product = _productService.GetProductById(newShoppingCartItem.ProductId);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            Customer customer = _customerService.GetCustomerById(newShoppingCartItem.CustomerId);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            ShoppingCartType shoppingCartType = (ShoppingCartType)Enum.Parse(typeof(ShoppingCartType), shoppingCartItemDelta.Dto.ShoppingCartType);

            if (!product.IsRental)
            {
                newShoppingCartItem.RentalStartDateUtc = null;
                newShoppingCartItem.RentalEndDateUtc = null;
            }

            string attributesXml = _productAttributeConverter.ConvertToXml(shoppingCartItemDelta.Dto.Attributes, product.Id);

            int currentStoreId = _storeContext.CurrentStore.Id;

            IList<string> warnings = _shoppingCartService.AddToCart(customer, product, shoppingCartType, currentStoreId, attributesXml, 0M,
                                        newShoppingCartItem.RentalStartDateUtc, newShoppingCartItem.RentalEndDateUtc,
                                        shoppingCartItemDelta.Dto.Quantity ?? 1);

            if (warnings.Count > 0)
            {
                foreach (var warning in warnings)
                {
                    ModelState.AddModelError("shopping cart item", warning);
                }

                return Error(HttpStatusCode.BadRequest);
            }
            else
            {
                // the newly added shopping cart item should be the last one
                newShoppingCartItem = customer.ShoppingCartItems.LastOrDefault();
            }

            // Preparing the result dto of the new product category mapping
            ShoppingCartItemDto newShoppingCartItemDto = _dtoHelper.PrepareShoppingCartItemDTO(newShoppingCartItem);

            var shoppingCartsRootObject = new ShoppingCartItemsRootObject();

            shoppingCartsRootObject.ShoppingCartItems.Add(newShoppingCartItemDto);

            var json = _jsonFieldsSerializer.Serialize(shoppingCartsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [ResponseType(typeof(ShoppingCartItemsRootObject))]
        public IHttpActionResult UpdateShoppingCartItem([ModelBinder(typeof(JsonModelBinder<ShoppingCartItemDto>))] Delta<ShoppingCartItemDto> shoppingCartItemDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            // We kno that the id will be valid integer because the validation for this happens in the validator which is executed by the model binder.
            ShoppingCartItem shoppingCartItemForUpdate =
                _shoppingCartItemApiService.GetShoppingCartItem(int.Parse(shoppingCartItemDelta.Dto.Id));

            if (shoppingCartItemForUpdate == null)
            {
                return Error(HttpStatusCode.NotFound, "shopping_cart_item", "not found");
            }

            shoppingCartItemDelta.Merge(shoppingCartItemForUpdate);

            if (!shoppingCartItemForUpdate.Product.IsRental)
            {
                shoppingCartItemForUpdate.RentalStartDateUtc = null;
                shoppingCartItemForUpdate.RentalEndDateUtc = null;
            }

            if (shoppingCartItemDelta.Dto.Attributes != null)
            {
                shoppingCartItemForUpdate.AttributesXml = _productAttributeConverter.ConvertToXml(shoppingCartItemDelta.Dto.Attributes, shoppingCartItemForUpdate.Product.Id);
            }

            // The update time is set in the service.
            var warnings = _shoppingCartService.UpdateShoppingCartItem(shoppingCartItemForUpdate.Customer, shoppingCartItemForUpdate.Id,
                shoppingCartItemForUpdate.AttributesXml, shoppingCartItemForUpdate.CustomerEnteredPrice,
                shoppingCartItemForUpdate.RentalStartDateUtc, shoppingCartItemForUpdate.RentalEndDateUtc,
                shoppingCartItemForUpdate.Quantity);

            if (warnings.Count > 0)
            {
                foreach (var warning in warnings)
                {
                    ModelState.AddModelError("shopping cart item", warning);
                }

                return Error(HttpStatusCode.BadRequest);
            }
            else
            {
                shoppingCartItemForUpdate = _shoppingCartItemApiService.GetShoppingCartItem(shoppingCartItemForUpdate.Id);
            }

            // Preparing the result dto of the new product category mapping
            ShoppingCartItemDto newShoppingCartItemDto = _dtoHelper.PrepareShoppingCartItemDTO(shoppingCartItemForUpdate);

            var shoppingCartsRootObject = new ShoppingCartItemsRootObject();

            shoppingCartsRootObject.ShoppingCartItems.Add(newShoppingCartItemDto);

            var json = _jsonFieldsSerializer.Serialize(shoppingCartsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [GetRequestsErrorInterceptorActionFilter]
        public IHttpActionResult DeleteShoppingCartItem(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            ShoppingCartItem shoppingCartItemForDelete = _shoppingCartItemApiService.GetShoppingCartItem(id);

            if (shoppingCartItemForDelete == null)
            {
                return Error(HttpStatusCode.NotFound, "shopping_cart_item", "not found");
            }

            _shoppingCartService.DeleteShoppingCartItem(shoppingCartItemForDelete);

            //activity log
            _customerActivityService.InsertActivity("DeleteShoppingCartItem", _localizationService.GetResource("ActivityLog.DeleteShoppingCartItem"), shoppingCartItemForDelete.Id);

            return new RawJsonActionResult("{}");
        }

        public class Data : BaseNopModel
        {
            public List<cartItemAcc> cartlist { get; set; }
            public Guid customerId { get; set; }
        }

        public class cartItemAcc
        {
            public string Id { get; set; }
            public string Quantity { get; set; }

        }

        [HttpPost]
        public IHttpActionResult UpdateCartList(Data model)
        {
            Customer customer = _customerService.GetCustomerByGuid(model.customerId);
            _workContext.CurrentCustomer = customer;

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            // var allIdsToRemove = form["removefromcart"] != null ? form["removefromcart"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();
            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                foreach (var formKey in model.cartlist)
                    if (int.Parse(formKey.Id).Equals(sci.Id))
                    {
                        int newQuantity;
                        if (int.TryParse(formKey.Quantity, out newQuantity))
                        {
                            var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(customer,
                                sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                newQuantity, true);
                            innerWarnings.Add(sci.Id, currSciWarnings);
                        }
                        break;
                    }
            }
            //updated cart
            cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            return Successful("");
        }

        [HttpGet]
        public IHttpActionResult RemoveCartList(int cartId, Guid? customerGuid)
        {
            Customer customer = customerGuid.HasValue ?
              _customerService.GetCustomerByGuid(customerGuid.Value)
              : _workContext.CurrentCustomer;
            _workContext.CurrentCustomer = customer;

            var cart = customer.ShoppingCartItems
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            foreach (var sci in cart)
            {
                if (sci.Id == cartId)
                {
                    _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
                }
            }
            return Successful("");
        }

        public class CouponModel
        {
            public string discountcouponcode { get; set; }

            public Guid? customerGuid { get; set; }

            //public int discountId { get; set; }
        }

        [HttpPost]
        public IHttpActionResult ApplyDiscountCoupon(CouponModel couponModel)
        {
            if (!couponModel.customerGuid.HasValue)
            {
                return Error(HttpStatusCode.BadRequest, "customerGuid", "invalid customer guid");
            }

            if (string.IsNullOrEmpty(couponModel.discountcouponcode))
            {
                return Error(HttpStatusCode.BadRequest, "coupon code", "invalid coupon code");
            }

            Customer customer = couponModel.customerGuid.HasValue ?
              _customerService.GetCustomerByGuid(couponModel.customerGuid.Value)
              : _workContext.CurrentCustomer;

            _workContext.CurrentCustomer = customer;

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            var model = new ShoppingCartModel();
            if (!String.IsNullOrWhiteSpace(couponModel.discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discount = _discountService.GetDiscountByCouponCode(couponModel.discountcouponcode, true);
                if (discount != null && discount.RequiresCouponCode)
                {
                    var validationResult = _discountService.ValidateDiscount(discount, customer, new string[] { couponModel.discountcouponcode });
                    if (validationResult.IsValid)
                    {
                        //valid
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCouponCode, couponModel.discountcouponcode);
                        model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied");
                        model.DiscountBox.IsApplied = true;
                        return Successful("");
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(validationResult.UserError))
                        {
                            //some user error
                            model.DiscountBox.Message = validationResult.UserError;
                            model.DiscountBox.IsApplied = false;
                            return ErrorOccured(model.DiscountBox.Message);
                        }
                        else
                        {
                            //general error text
                            model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                            model.DiscountBox.IsApplied = false;
                            return ErrorOccured(model.DiscountBox.Message);
                        }
                    }
                }
                else
                {
                    //discount cannot be found
                    model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                    model.DiscountBox.IsApplied = false;
                    return ErrorOccured(model.DiscountBox.Message);
                }
            }
            else
            {
                //empty coupon code
                model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                model.DiscountBox.IsApplied = false;
                return ErrorOccured(model.DiscountBox.Message);
            }
        }

        [HttpPost]
        public IHttpActionResult RemoveDiscountCoupon(CouponModel couponModel)
        {
            if (!couponModel.customerGuid.HasValue)
            {
                return Error(HttpStatusCode.BadRequest, "customerGuid", "invalid customer guid");
            }

            //if (couponModel.discountId <= 0)
            //{
            //    return Error(HttpStatusCode.BadRequest, "discount id", "invalid discount id");
            //}

            Customer customer = couponModel.customerGuid.HasValue ?
            _customerService.GetCustomerByGuid(couponModel.customerGuid.Value)
            : _workContext.CurrentCustomer;

            _workContext.CurrentCustomer = customer;

            var model = new ShoppingCartModel();
            var listDiscountId = new List<int>();
            var discountCouponCodes = _workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes();
            foreach (var couponCode in discountCouponCodes)
            {
                var discounts = _discountService.GetAllDiscountsForCaching(couponCode: couponCode)
                    .FirstOrDefault();

                if (discounts != null)
                {
                    listDiscountId.Add(discounts.Id);
                }
            }

            if (listDiscountId.Count() > 0)
            {
                foreach (var item in listDiscountId)
                {
                    var discount = _discountService.GetDiscountById(item);
                    if (discount != null)
                        _workContext.CurrentCustomer.RemoveDiscountCouponCode(discount.CouponCode);
                }
            }
            


            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);

            var shoppingCartAsDto = _dtoHelper.PrepareShoppingCartDto(model);

            var shoppingCartRootObject = new ShoppingCartRootObject();

            shoppingCartRootObject.ShoppingCart = shoppingCartAsDto;

            var json = _jsonFieldsSerializer.Serialize(shoppingCartRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        public IHttpActionResult GetOrderTotal(Guid? customerId)
        {
            if (!customerId.HasValue)
            {
                return Error(HttpStatusCode.BadRequest, "customer id", "invalid customer id");
            }

            Customer customer = customerId.HasValue ?
            _customerService.GetCustomerByGuid(customerId.Value)
            : _workContext.CurrentCustomer;

            _workContext.CurrentCustomer = customer;

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
               .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
               .LimitPerStore(_storeContext.CurrentStore.Id)
               .ToList();

            _shippingService.SetDefaultShippingMethodToCustomer(cart, "Ninja Van", "Shipping.FixedOrByWeight", _workContext.CurrentCustomer);

            var model = _shoppingCartModelFactory.PrepareOrderTotalsModel(cart, false);
            return Json(new
            {
                success = true,
                data = model
            });

        }



        [HttpPost]
        [ResponseType(typeof(ShoppingCartRootObject))]
        public IHttpActionResult ApplyCouponCodeDiscount(CouponModel couponModel)
        {
            if (string.IsNullOrEmpty(couponModel.discountcouponcode))
            {
                return Error(HttpStatusCode.BadRequest, "coupon code", "invalid coupon code");
            }

            if (!couponModel.customerGuid.HasValue)
            {
                return Error(HttpStatusCode.BadRequest, "guid", "invalid guid");
            }

            Customer customer = couponModel.customerGuid.HasValue ?
              _customerService.GetCustomerByGuid(couponModel.customerGuid.Value)
              : _workContext.CurrentCustomer;

            _workContext.CurrentCustomer = customer;

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var model = new ShoppingCartModel();
            if (!String.IsNullOrWhiteSpace(couponModel.discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discounts = _discountService.GetAllDiscountsForCaching(couponCode: couponModel.discountcouponcode, showHidden: true)
                    .Where(d => d.RequiresCouponCode)
                    .ToList();
                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    var anyValidDiscount = discounts.Any(discount =>
                    {
                        var validationResult = _discountService.ValidateDiscount(discount, _workContext.CurrentCustomer, new[] { couponModel.discountcouponcode });
                        userErrors.AddRange(validationResult.Errors);

                        return validationResult.IsValid;
                    });

                    if (anyValidDiscount)
                    {
                        //valid
                        _workContext.CurrentCustomer.ApplyDiscountCouponCode(couponModel.discountcouponcode);
                        model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied"));
                        model.DiscountBox.IsApplied = true;
                    }
                    else
                    {
                        if (userErrors.Any())
                            //some user errors
                            model.DiscountBox.Messages = userErrors;
                        else
                            //general error text
                            model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                    }
                }
                else
                    //discount cannot be found
                    model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
            }
            else
                //empty coupon code
                model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));

            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);

            var shoppingCartAsDto = _dtoHelper.PrepareShoppingCartDto(model);

            var shoppingCartRootObject = new ShoppingCartRootObject();

            shoppingCartRootObject.ShoppingCart = shoppingCartAsDto;

            var json = _jsonFieldsSerializer.Serialize(shoppingCartRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }
    }


}