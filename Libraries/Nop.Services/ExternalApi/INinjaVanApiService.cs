using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nop.Services.ExternalApi.NinjaVanApiService;

namespace Nop.Services.ExternalApi
{
    public partial interface INinjaVanApiService
    {
        OAuthData GetAccessToken();

        OrderCreateData CreateOrder(Order order, Warehouse warehouse,
            Address warehouseAddress, string TrackingNumber, DateTime NinjaVanPickupDate,
            DateTime NinjaVanDeliveryDate, int? ParcelSize, int? ParcelVolume, decimal? ParcelWeight, OAuthData oauthData);

        OrderCreateData CreateOrderWithSubShipperId(Order order, Warehouse warehouse,
            Address warehouseAddress, string TrackingNumber, DateTime NinjaVanPickupDate,
            DateTime NinjaVanDeliveryDate, int? ParcelSize, int? ParcelVolume, decimal? ParcelWeight, OAuthData oauthData);

        List<TrackOrderData> TrackOrder(List<string> trackingIds);
    }
}
