using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Plugin.Api.DTOs.Images;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.DTOs.Categories;
using Nop.Plugin.Api.DTOs.Customers;
using Nop.Plugin.Api.DTOs.Orders;
using Nop.Plugin.Api.Services;
using Nop.Services.Media;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.DTOs.ShippingMethods;
using Nop.Core.Domain.Shipping;
using Nop.Web.Models.Catalog;
using Nop.Web.Infrastructure.Cache;
using Nop.Core.Caching;
using Nop.Plugin.Api.DTOs.Countries;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.DTOs.States;
using Nop.Plugin.Api.Models.StoresParameters;
using Nop.Core.Domain.Events;
using Nop.Plugin.Api.DTOs.Events;
using Nop.Web.Models.ShoppingCart;
using Nop.Plugin.Api.DTOs;

namespace Nop.Plugin.Api.Helpers
{
    public class DTOHelper : IDTOHelper
    {
        private IProductService _productService;
        private IAclService _aclService;
        private IStoreMappingService _storeMappingService;
        private IPictureService _pictureService;
        private IProductAttributeService _productAttributeService;
        private ICustomerApiService _customerApiService;
        private IProductAttributeConverter _productAttributeConverter;
        private CatalogSettings _catalogSettings;
        private ICacheManager _cacheManager;

        public DTOHelper(IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            ICustomerApiService customerApiService,
            IProductAttributeConverter productAttributeConverter,
            CatalogSettings catalogSettings,
            ICacheManager cacheManager)
        {
            _productService = productService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _customerApiService = customerApiService;
            _productAttributeConverter = productAttributeConverter;
            _catalogSettings = catalogSettings;
            _cacheManager = cacheManager;
        }

        public ProductDto PrepareProductDTO(Product product)
        {
            ProductDto productDto = product.ToDto();

            PrepareProductImages(product.ProductPictures, productDto);
            PrepareProductAttributes(product.ProductAttributeMappings, productDto);

            productDto.SeName = product.GetSeName();
            productDto.DiscountIds = product.AppliedDiscounts.Select(discount => discount.Id).ToList();
            productDto.ManufacturerIds = product.ProductManufacturers.Select(pm => pm.ManufacturerId).ToList();
            productDto.RoleIds = _aclService.GetAclRecords(product).Select(acl => acl.CustomerRoleId).ToList();
            productDto.StoreIds = _storeMappingService.GetStoreMappings(product).Select(mapping => mapping.StoreId).ToList();
            productDto.Tags = product.ProductTags.Select(tag => tag.Name).ToList();
            productDto.CategoriesIds = product.ProductCategories.Select(x => x.CategoryId).ToList();


            decimal percentage = 0;
            if (productDto.OldPrice > 0)
            {
                percentage = Math.Round(((decimal)productDto.OldPrice - (decimal)productDto.Price) / (decimal)productDto.OldPrice * 100, 2);
                //if (percentage > 0)
                //{
                //    productDto.DiscountPercentage = percentage;
                //}
            }
            productDto.DiscountPercentage = percentage;

            productDto.AssociatedProductIds =
                _productService.GetAssociatedProducts(product.Id, showHidden: true)
                    .Select(associatedProduct => associatedProduct.Id)
                    .ToList();

            return productDto;
        }

        public CategoryDto PrepareCategoryDTO(Category category)
        {
            CategoryDto categoryDto = category.ToDto();

            Picture picture = _pictureService.GetPictureById(category.PictureId);
            ImageDto imageDto = PrepareImageDto(picture);

            if (imageDto != null)
            {
                categoryDto.Image = imageDto;
            }

            categoryDto.SeName = category.GetSeName();
            categoryDto.DiscountIds = category.AppliedDiscounts.Select(discount => discount.Id).ToList();
            categoryDto.RoleIds = _aclService.GetAclRecords(category).Select(acl => acl.CustomerRoleId).ToList();
            categoryDto.StoreIds = _storeMappingService.GetStoreMappings(category).Select(mapping => mapping.StoreId).ToList();

            return categoryDto;
        }

        public OrderDto PrepareOrderDTO(Order order)
        {
            OrderDto orderDto = order.ToDto();

            CustomerDto customerDto = _customerApiService.GetCustomerById(order.Customer.Id);

            if (customerDto != null)
            {
                orderDto.Customer = customerDto.ToOrderCustomerDto();
            }


            if (orderDto.OrderItemDtos.Count() > 0)
            {
                foreach (var items in orderDto.OrderItemDtos)
                {
                    int pId = ConvertIntWithoutNull(items.ProductId);
                    if (pId > 0)
                    {
                        var product = _productService.GetProductById(pId);
                        PrepareProductImages(product.ProductPictures, items.Product);
                    }

                    //List<ImageMappingDto> imageDtos = new List<ImageMappingDto>();
                    //int pId = ConvertIntWithoutNull(items.ProductId);
                    //if (pId > 0)
                    //{
                    //    var productPictures = _pictureService.GetPicturesByProductId(pId);
                    //    if (productPictures.Count() > 0)
                    //    {
                    //        foreach (var item_productPictures in productPictures)
                    //        {
                    //            var picture = PrepareImageDto(item_productPictures);
                    //            if (picture != null)
                    //            {
                    //                ImageMappingDto productImageDto = new ImageMappingDto();
                    //                productImageDto.Id = item_productPictures.Id;
                    //                productImageDto.Position = items;
                    //                productImageDto.Src = picture.Src;
                    //                productImageDto.Attachment = picture.Attachment;

                    //                imageDtos.Add(productImageDto);
                    //            }
                    //        }
                    //    }
                    //}
                    //items.Product.Images = imageDtos;
                }
            }

            return orderDto;
        }

        private int ConvertIntWithoutNull(int? id)
        {
            int returnId = 0;
            if (id != null)
            {
                returnId = (int)id;
            }
            return returnId;
        }

        public ShoppingCartItemDto PrepareShoppingCartItemDTO(ShoppingCartItem shoppingCartItem)
        {
            var dto = shoppingCartItem.ToDto();
            dto.ProductDto = PrepareProductDTO(shoppingCartItem.Product);
            dto.CustomerDto = shoppingCartItem.Customer.ToCustomerForShoppingCartItemDto();
            dto.Attributes = _productAttributeConverter.Parse(shoppingCartItem.AttributesXml);
            return dto;
        }



        private void PrepareProductImages(IEnumerable<ProductPicture> productPictures, ProductDto productDto)
        {
            if (productDto.Images == null)
                productDto.Images = new List<ImageMappingDto>();

            // Here we prepare the resulted dto image.
            foreach (var productPicture in productPictures)
            {
                ImageDto imageDto = PrepareImageDto(productPicture.Picture);

                if (imageDto != null)
                {
                    ImageMappingDto productImageDto = new ImageMappingDto();
                    productImageDto.Id = productPicture.Id;
                    productImageDto.Position = productPicture.DisplayOrder;
                    productImageDto.Src = imageDto.Src;
                    productImageDto.Attachment = imageDto.Attachment;

                    productDto.Images.Add(productImageDto);
                }
            }
        }

        protected ImageDto PrepareImageDto(Picture picture)
        {
            ImageDto image = null;

            if (picture != null)
            {
                // We don't use the image from the passed dto directly 
                // because the picture may be passed with src and the result should only include the base64 format.
                image = new ImageDto()
                {
                    //Attachment = Convert.ToBase64String(picture.PictureBinary),
                    Src = _pictureService.GetPictureUrl(picture)
                };
            }

            return image;
        }

        private void PrepareProductAttributes(IEnumerable<ProductAttributeMapping> productAttributeMappings, ProductDto productDto)
        {
            if (productDto.ProductAttributeMappings == null)
                productDto.ProductAttributeMappings = new List<ProductAttributeMappingDto>();

            foreach (var productAttributeMapping in productAttributeMappings)
            {
                ProductAttributeMappingDto productAttributeMappingDto = PrepareProductAttributeMappingDto(productAttributeMapping);

                if (productAttributeMappingDto != null)
                {
                    productDto.ProductAttributeMappings.Add(productAttributeMappingDto);
                }
            }
        }

        private ProductAttributeMappingDto PrepareProductAttributeMappingDto(ProductAttributeMapping productAttributeMapping)
        {
            ProductAttributeMappingDto productAttributeMappingDto = null;

            if (productAttributeMapping != null)
            {
                productAttributeMappingDto = new ProductAttributeMappingDto()
                {
                    Id = productAttributeMapping.Id,
                    ProductAttributeId = productAttributeMapping.ProductAttributeId,
                    ProductAttributeName = _productAttributeService.GetProductAttributeById(productAttributeMapping.ProductAttributeId).Name,
                    TextPrompt = productAttributeMapping.TextPrompt,
                    DefaultValue = productAttributeMapping.DefaultValue,
                    AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                    DisplayOrder = productAttributeMapping.DisplayOrder,
                    IsRequired = productAttributeMapping.IsRequired,
                    ProductAttributeValues = productAttributeMapping.ProductAttributeValues.Select(x => PrepareProductAttributeValueDto(x, productAttributeMapping.Product)).ToList()
                };
            }

            return productAttributeMappingDto;
        }

        private ProductAttributeValueDto PrepareProductAttributeValueDto(ProductAttributeValue productAttributeValue, Product product)
        {
            ProductAttributeValueDto productAttributeValueDto = null;

            if (productAttributeValue != null)
            {
                productAttributeValueDto = productAttributeValue.ToDto();
                if (productAttributeValue.ImageSquaresPictureId > 0)
                {
                    Picture imageSquaresPicture = _pictureService.GetPictureById(productAttributeValue.ImageSquaresPictureId);
                    ImageDto imageDto = PrepareImageDto(imageSquaresPicture);
                    productAttributeValueDto.ImageSquaresImage = imageDto;
                }

                if (productAttributeValue.PictureId > 0)
                {
                    // make sure that the picture is mapped to the product
                    // This is needed since if you delete the product picture mapping from the nopCommerce administrationthe
                    // then the attribute value is not updated and it will point to a picture that has been deleted
                    var productPicture = product.ProductPictures.FirstOrDefault(pp => pp.PictureId == productAttributeValue.PictureId);
                    if (productPicture != null)
                    {
                        productAttributeValueDto.ProductPictureId = productPicture.Id;
                    }
                }
            }

            return productAttributeValueDto;
        }

        public ShippingMethodsDto PrepareShippingMethodsDTO(ShippingMethod shippingMethod)
        {
            ShippingMethodsDto shippingMethodsDto = shippingMethod.ToDto();

            return shippingMethodsDto;
        }

        public ProductReviewDto PrepareProductReviewDto(ProductReview productReview)
        {
            ProductReviewDto productReviewDto = productReview.ToDto();

            return productReviewDto;
        }

        public ProductReviewOverviewModel PrepareProductReviewOverviewModel(Product product)
        {
            ProductReviewOverviewModel productReview;

            if (_catalogSettings.ShowProductReviewsPerStore)
            {
                string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_REVIEWS_MODEL_KEY, product.Id, 1);

                productReview = _cacheManager.Get(cacheKey, () =>
                {
                    return new ProductReviewOverviewModel
                    {
                        RatingSum = product.ProductReviews
                                .Where(pr => pr.IsApproved && pr.StoreId == 1)
                                .Sum(pr => pr.Rating),
                        TotalReviews = product
                                .ProductReviews
                                .Count(pr => pr.IsApproved && pr.StoreId == 1)
                    };
                });
            }
            else
            {
                productReview = new ProductReviewOverviewModel()
                {
                    RatingSum = product.ApprovedRatingSum,
                    TotalReviews = product.ApprovedTotalReviews
                };
            }
            if (productReview != null)
            {
                productReview.ProductId = product.Id;
                productReview.AllowCustomerReviews = product.AllowCustomerReviews;
            }
            return productReview;
        }

        public CountryDto PrepareCountryDTO(Country country)
        {
            var dto = country.ToDto();
            return dto;
        }

        public StateDto PrepareStateDTO(StateProvince state)
        {
            var dto = state.ToDto();
            dto.CountryDto = PrepareCountryDTO(state.Country);
            return dto;
        }

        public ImageDto PrepareBannerImageDto(Picture picture)
        {
            var imageDto = new ImageDto();
            imageDto = PrepareImageDto(picture);
            return imageDto;
        }

        public SliderDto PrepareSliderDto(SliderModel slider, List<SliderImageModel> sliderImages)
        {
            var sliderDto = new SliderDto();
            sliderDto.Id = slider.Id;
            sliderDto.SystemName = slider.SystemName;
            sliderDto.SliderType = slider.SliderType;
            sliderDto.LanguageId = slider.LanguageId;
            sliderDto.LimitedToStores = slider.LimitedToStores;

            var imageDtos = new List<ImageDto>();
            if (sliderImages != null)
            {
                foreach (var items in sliderImages)
                {
                    var pic = _pictureService.GetPictureById(items.PictureId);
                    var image = PrepareImageDto(pic);
                    imageDtos.Add(image);
                }
                sliderDto.images = imageDtos;
            }

            return sliderDto;
        }

        public EventDto PrepareEventDTO(Event ev)
        {
            var dto = ev.ToDto();
            return dto;
        }

        public ShoppingCartDto PrepareShoppingCartDto(ShoppingCartModel shoppingCartModel)
        {
            var shoppingCartDto = new ShoppingCartDto();
            if (shoppingCartModel != null)
            {
                shoppingCartDto.OnePageCheckoutEnabled = shoppingCartModel.OnePageCheckoutEnabled;
                shoppingCartDto.ShowSku = shoppingCartModel.ShowSku;
                shoppingCartDto.ShowProductImages = shoppingCartModel.ShowProductImages;
                shoppingCartDto.IsEditable = shoppingCartModel.IsEditable;
                var cartItemDtos = new List<CartItemDto>();
                if (shoppingCartModel.Items.Count() > 0)
                {
                    foreach (var item in shoppingCartModel.Items)
                    {
                        var itemDto = new CartItemDto();
                        itemDto.Sku = item.Sku;

                        var picDto = new PictureDto();
                        picDto.ImageUrl = item.Picture.ImageUrl;
                        picDto.ThumbImageUrl = item.Picture.ThumbImageUrl;
                        picDto.FullSizeImageUrl = item.Picture.FullSizeImageUrl;
                        picDto.Title = item.Picture.Title;
                        picDto.AlternateText = item.Picture.AlternateText;
                        itemDto.Picture = picDto;

                        itemDto.ProductId = item.ProductId;
                        itemDto.ProductName = item.ProductName;
                        itemDto.ProductSeName = item.ProductSeName;
                        itemDto.UnitPrice = item.UnitPrice;
                        itemDto.SubTotal = item.SubTotal;
                        itemDto.Discount = item.Discount;
                        itemDto.MaximumDiscountedQty = item.MaximumDiscountedQty;
                        itemDto.Quantity = item.Quantity;
                        itemDto.AllowedQuantities = item.AllowedQuantities;
                        itemDto.AttributeInfo = item.AttributeInfo;
                        itemDto.RecurringInfo = item.RecurringInfo;
                        itemDto.RentalInfo = item.RentalInfo;
                        itemDto.AllowItemEditing = item.AllowItemEditing;
                        itemDto.DisableRemoval = item.DisableRemoval;
                        itemDto.Warnings = item.Warnings;
                        cartItemDtos.Add(itemDto);
                    }
                }

                shoppingCartDto.Items = cartItemDtos;
                shoppingCartDto.CheckoutAttributeInfo = shoppingCartModel.CheckoutAttributeInfo;

                var checkoutAttDto = new List<CheckoutAttributeDto>();
                if (shoppingCartModel.CheckoutAttributes.Count() > 0)
                {
                    foreach (var item in shoppingCartModel.CheckoutAttributes)
                    {
                        var attDto = new CheckoutAttributeDto();
                        attDto.Name = item.Name;
                        attDto.DefaultValue = item.DefaultValue;
                        attDto.TextPrompt = item.TextPrompt;
                        attDto.IsRequired = item.IsRequired;
                        attDto.SelectedDay = item.SelectedDay;
                        attDto.SelectedMonth = item.SelectedMonth;
                        attDto.SelectedYear = item.SelectedYear;
                        attDto.AllowedFileExtensions = item.AllowedFileExtensions;
                        attDto.AttributeControlType = item.AttributeControlType;
                        var attValDtos = new List<CheckoutAttributeValueDto>();
                        if (item.Values.Count() > 0)
                        {
                            foreach (var item2 in item.Values)
                            {
                                var attVal = new CheckoutAttributeValueDto();
                                attVal.Name = item2.Name;
                                attVal.ColorSquaresRgb = item2.ColorSquaresRgb;
                                attVal.PriceAdjustment = item2.PriceAdjustment;
                                attVal.IsPreSelected = item2.IsPreSelected;
                                attValDtos.Add(attVal);
                            }
                        }
                        attDto.Values = attValDtos;
                        checkoutAttDto.Add(attDto);
                    }

                }
                shoppingCartDto.CheckoutAttributes = checkoutAttDto;

                shoppingCartDto.Warnings = shoppingCartModel.Warnings;
                shoppingCartDto.DisplayTaxShippingInfo = shoppingCartModel.DisplayTaxShippingInfo;
                shoppingCartDto.TermsOfServiceOnShoppingCartPage = shoppingCartModel.TermsOfServiceOnShoppingCartPage;
                shoppingCartDto.TermsOfServiceOnOrderConfirmPage = shoppingCartModel.TermsOfServiceOnOrderConfirmPage;
                //shoppingCartDto.EstimateShipping = shoppingCartModel.EstimateShipping;

                var discountBoxDto = new DiscountBoxDto();
                var discountInfoDtos = new List<DiscountInfoDto>();
                if (shoppingCartModel.DiscountBox.AppliedDiscountsWithCodes.Count() > 0)
                {
                    foreach (var item in shoppingCartModel.DiscountBox.AppliedDiscountsWithCodes)
                    {
                        var infoDto = new DiscountInfoDto();
                        infoDto.Id = item.Id.ToString();
                        infoDto.CouponCode = item.CouponCode;
                        discountInfoDtos.Add(infoDto);
                    }
                }
                discountBoxDto.AppliedDiscountsWithCodes = discountInfoDtos;
                discountBoxDto.Display = shoppingCartModel.DiscountBox.Display;
                discountBoxDto.Messages = shoppingCartModel.DiscountBox.Messages;
                discountBoxDto.IsApplied = shoppingCartModel.DiscountBox.IsApplied;
                discountBoxDto.Message = shoppingCartModel.DiscountBox.Message;
                shoppingCartDto.DiscountBox = discountBoxDto;

                //var giftCardBoxDto = new GiftCardBoxDto();
                //giftCardBoxDto.Display = shoppingCartModel.GiftCardBox.Display;
                //giftCardBoxDto.Message = shoppingCartModel.GiftCardBox.Message;
                //giftCardBoxDto.IsApplied = shoppingCartModel.GiftCardBox.IsApplied;
                //shoppingCartDto.GiftCardBox = giftCardBoxDto;

                //var orderReviewDataDto = new OrderReviewDataDto();
                //orderReviewDataDto.Display = shoppingCartModel.OrderReviewData.Display;
                //var billAddress = new AddressDto();
                //billAddress.Address1 = shoppingCartModel.OrderReviewData.BillingAddress.Address1;
                //billAddress.Address2 = shoppingCartModel.OrderReviewData.BillingAddress.Address2;
                //billAddress.City = shoppingCartModel.OrderReviewData.BillingAddress.City;
                //billAddress.Company = shoppingCartModel.OrderReviewData.BillingAddress.Company;
                //billAddress.CountryId = shoppingCartModel.OrderReviewData.BillingAddress.CountryId;
                //billAddress.CountryName = shoppingCartModel.OrderReviewData.BillingAddress.CountryName;
                //billAddress.Email = shoppingCartModel.OrderReviewData.BillingAddress.Email;
                //billAddress.FaxNumber = shoppingCartModel.OrderReviewData.BillingAddress.FaxNumber;
                //billAddress.FirstName = shoppingCartModel.OrderReviewData.BillingAddress.FirstName;
                //billAddress.Id = shoppingCartModel.OrderReviewData.BillingAddress.Id.ToString();
                //billAddress.StateProvinceId = shoppingCartModel.OrderReviewData.BillingAddress.StateProvinceId;
                //billAddress.StateProvinceName = shoppingCartModel.OrderReviewData.BillingAddress.StateProvinceName;
                //billAddress.ZipPostalCode = shoppingCartModel.OrderReviewData.BillingAddress.ZipPostalCode;
                //orderReviewDataDto.BillingAddress = billAddress;
                //orderReviewDataDto.IsShippable = shoppingCartModel.OrderReviewData.IsShippable;
                //var shipAddress = new AddressDto();
                //shipAddress.Address1 = shoppingCartModel.OrderReviewData.ShippingAddress.Address1;
                //shipAddress.Address2 = shoppingCartModel.OrderReviewData.ShippingAddress.Address2;
                //shipAddress.City = shoppingCartModel.OrderReviewData.ShippingAddress.City;
                //shipAddress.Company = shoppingCartModel.OrderReviewData.ShippingAddress.Company;
                //shipAddress.CountryId = shoppingCartModel.OrderReviewData.ShippingAddress.CountryId;
                //shipAddress.CountryName = shoppingCartModel.OrderReviewData.ShippingAddress.CountryName;
                //shipAddress.Email = shoppingCartModel.OrderReviewData.ShippingAddress.Email;
                //shipAddress.FaxNumber = shoppingCartModel.OrderReviewData.ShippingAddress.FaxNumber;
                //shipAddress.FirstName = shoppingCartModel.OrderReviewData.ShippingAddress.FirstName;
                //shipAddress.Id = shoppingCartModel.OrderReviewData.ShippingAddress.Id.ToString();
                //shipAddress.StateProvinceId = shoppingCartModel.OrderReviewData.ShippingAddress.StateProvinceId;
                //shipAddress.StateProvinceName = shoppingCartModel.OrderReviewData.ShippingAddress.StateProvinceName;
                //shipAddress.ZipPostalCode = shoppingCartModel.OrderReviewData.ShippingAddress.ZipPostalCode;
                //orderReviewDataDto.ShippingAddress = shipAddress;
                //orderReviewDataDto.SelectedPickUpInStore = shoppingCartModel.OrderReviewData.SelectedPickUpInStore;
                //var pickAddress = new AddressDto();
                //pickAddress.Address1 = shoppingCartModel.OrderReviewData.PickupAddress.Address1;
                //pickAddress.Address2 = shoppingCartModel.OrderReviewData.PickupAddress.Address2;
                //pickAddress.City = shoppingCartModel.OrderReviewData.PickupAddress.City;
                //pickAddress.Company = shoppingCartModel.OrderReviewData.PickupAddress.Company;
                //pickAddress.CountryId = shoppingCartModel.OrderReviewData.PickupAddress.CountryId;
                //pickAddress.CountryName = shoppingCartModel.OrderReviewData.PickupAddress.CountryName;
                //pickAddress.Email = shoppingCartModel.OrderReviewData.PickupAddress.Email;
                //pickAddress.FaxNumber = shoppingCartModel.OrderReviewData.PickupAddress.FaxNumber;
                //pickAddress.FirstName = shoppingCartModel.OrderReviewData.PickupAddress.FirstName;
                //pickAddress.Id = shoppingCartModel.OrderReviewData.PickupAddress.Id.ToString();
                //pickAddress.StateProvinceId = shoppingCartModel.OrderReviewData.PickupAddress.StateProvinceId;
                //pickAddress.StateProvinceName = shoppingCartModel.OrderReviewData.PickupAddress.StateProvinceName;
                //pickAddress.ZipPostalCode = shoppingCartModel.OrderReviewData.PickupAddress.ZipPostalCode;
                //orderReviewDataDto.PickupAddress = pickAddress;
                //orderReviewDataDto.ShippingMethod = shoppingCartModel.OrderReviewData.ShippingMethod;
                //orderReviewDataDto.PaymentMethod = shoppingCartModel.OrderReviewData.PaymentMethod;
                //orderReviewDataDto.CustomValues = shoppingCartModel.OrderReviewData.CustomValues;
                //shoppingCartDto.OrderReviewData = orderReviewDataDto;

                //shoppingCartDto.ButtonPaymentMethodActionNames = shoppingCartModel.ButtonPaymentMethodActionNames;
                //shoppingCartDto.ButtonPaymentMethodControllerNames = shoppingCartModel.ButtonPaymentMethodControllerNames;
                //shoppingCartDto.ButtonPaymentMethodRouteValues = shoppingCartModel.ButtonPaymentMethodRouteValues;
                //shoppingCartDto.HideCheckoutButton = shoppingCartModel.HideCheckoutButton;
            }

            return shoppingCartDto;
        }
    }
}
