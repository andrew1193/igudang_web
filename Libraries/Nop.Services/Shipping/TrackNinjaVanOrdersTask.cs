using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.ExternalApi;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nop.Services.ExternalApi.NinjaVanApiService;

namespace Nop.Services.Shipping
{
    public partial class TrackNinjaVanOrdersTask : ITask
    {
        private readonly INinjaVanApiService _ninjaVanApiService;
        private readonly IShipmentService _shipmentService;
        private readonly IOrderService _orderService;
        public const string PendingPickup = "Pending Pickup";
        public const string Cancelled = "Cancelled";
        public const string Completed = "Completed";
        public const string EnrouteToSortingHub = "En-route to Sorting Hub";
        public const string OnVehicleForDelivery = "On Vehicle for Delivery";
        public const string PendingReschedule = "Pending Reschedule";
        public const string ReturnedToSender = "Returned to Sender";


        public TrackNinjaVanOrdersTask(INinjaVanApiService ninjaVanApiService,
            IShipmentService shipmentService,
            IOrderService orderService)
        {
            this._ninjaVanApiService = ninjaVanApiService;
            this._shipmentService = shipmentService;
            this._orderService = orderService;
        }

        private string GetNinjavanStatusString(int nJvanStatusId)
        {
            string strStatus = "";
            if (nJvanStatusId == 10)
            {
                strStatus = "Pending Pickup";
            }
            else if (nJvanStatusId == 20)
            {
                strStatus = "Completed";
            }
            else if (nJvanStatusId == 30)
            {
                strStatus = "Cancelled";
            }
            else if (nJvanStatusId == 40)
            {
                strStatus = "Returned to Sender";
            }
            else if (nJvanStatusId == 50)
            {
                strStatus = "En-route to Sorting Hub";
            }
            else if (nJvanStatusId == 60)
            {
                strStatus = "On Vehicle for Delivery";
            }
            else if (nJvanStatusId == 70)
            {
                strStatus = "Pending Reschedule";
            }

            return strStatus;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            try
            {
                var trackingIds = _shipmentService.GetPendingNinjaVanTrackingIds();
                var response = _ninjaVanApiService.TrackOrder(trackingIds);
                foreach (var data in response)
                {
                    var shipment = _shipmentService.GetShpmentByTrackingNumber(data.trackingId);
                    if (data.status != GetNinjavanStatusString(shipment.NinjaVanStatusId))
                    {
                        UpdateStatus(data, shipment);
                    }

                    //data.status = OnVehicleForDelivery;
                    //if (data.status!= PendingPickup && data.status != Cancelled)
                    //{
                    //    UpdateStatus(data);
                    //}
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void UpdateStatus(TrackOrderData data, Shipment shipment)
        {
            //var shipment = _shipmentService.GetShpmentByTrackingNumber(data.trackingId);

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (data.status == Completed)
            {
                shipment.NinjaVanStatusId = (int)NinjaVanStatus.Completed;
                shipment.DeliveryDateUtc = Convert.ToDateTime(data.updatedAt);
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
                if(order.PaymentStatus == PaymentStatus.Paid && order.ShippingStatus == ShippingStatus.Delivered)
                {
                    order.OrderStatusId = (int)OrderStatus.Complete;
                }
            }
            else if (data.status == OnVehicleForDelivery)
            {
                shipment.NinjaVanStatusId = (int)NinjaVanStatus.OnVehicleForDelivery;
                shipment.ShippedDateUtc = Convert.ToDateTime(data.updatedAt);
                order.ShippingStatusId = (int)ShippingStatus.Shipped;
            }
            else if (data.status == EnrouteToSortingHub)
            {
                shipment.NinjaVanStatusId = (int)NinjaVanStatus.EnrouteToSortingHub;
            }
            else if (data.status == OnVehicleForDelivery)
            {
                shipment.NinjaVanStatusId = (int)NinjaVanStatus.OnVehicleForDelivery;
            }
            else if (data.status == PendingReschedule)
            {
                shipment.NinjaVanStatusId = (int)NinjaVanStatus.PendingReschedule;
            }
            else if (data.status == ReturnedToSender)
            {
                shipment.NinjaVanStatusId = (int)NinjaVanStatus.ReturnedToSender;
            }
            _shipmentService.UpdateShipment(shipment);
            _orderService.UpdateOrder(order);


        }
    }
}
