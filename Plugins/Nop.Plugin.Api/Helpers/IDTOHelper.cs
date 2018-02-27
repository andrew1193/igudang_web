using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Events;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.DTOs.Categories;
using Nop.Plugin.Api.DTOs.Countries;
using Nop.Plugin.Api.DTOs.Events;
using Nop.Plugin.Api.DTOs.Images;
using Nop.Plugin.Api.DTOs.Orders;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.DTOs.ShippingMethods;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.DTOs.States;
using Nop.Plugin.Api.Models.StoresParameters;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.ShoppingCart;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Helpers
{
    public interface IDTOHelper
    {
        ProductDto PrepareProductDTO(Product product);
        CategoryDto PrepareCategoryDTO(Category category);
        OrderDto PrepareOrderDTO(Order order);
        ShoppingCartItemDto PrepareShoppingCartItemDTO(ShoppingCartItem shoppingCartItem);
        ShippingMethodsDto PrepareShippingMethodsDTO(ShippingMethod shippingMethod);
        ProductReviewDto PrepareProductReviewDto(ProductReview productReview);
        ProductReviewOverviewModel PrepareProductReviewOverviewModel(Product product);
        CountryDto PrepareCountryDTO(Country country);
        StateDto PrepareStateDTO(StateProvince state);
        ImageDto PrepareBannerImageDto(Picture picture);
        SliderDto PrepareSliderDto(SliderModel slider, List<SliderImageModel> sliderImages);
        EventDto PrepareEventDTO(Event ev);
        ShoppingCartDto PrepareShoppingCartDto(ShoppingCartModel shoppingCartModel);
    }
}
