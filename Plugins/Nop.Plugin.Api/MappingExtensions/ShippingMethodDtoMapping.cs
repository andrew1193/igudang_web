using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.ShippingMethods;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class ShippingMethodDtoMapping
    {
        public static ShippingMethodsDto ToDto(this ShippingMethod shippingMethod)
        {
            return shippingMethod.MapTo<ShippingMethod, ShippingMethodsDto>();
        }
    }
}
