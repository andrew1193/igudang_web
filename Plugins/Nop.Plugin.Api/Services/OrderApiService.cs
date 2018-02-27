using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.DataStructures;

namespace Nop.Plugin.Api.Services
{
    public class OrderApiService : IOrderApiService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;

        public OrderApiService(IRepository<Order> orderRepository,
            IRepository<ShippingMethod> shippingMethodRepository)
        {
            _orderRepository = orderRepository;
            _shippingMethodRepository = shippingMethodRepository;
        }

        public IList<Order> GetOrdersByCustomerId(int customerId)
        {
            var query = from order in _orderRepository.TableNoTracking
                        where order.CustomerId == customerId && !order.Deleted
                        orderby order.Id
                        select order;

            return new ApiList<Order>(query, 0, Configurations.MaxLimit);
        }

        public IList<Order> GetOrders(IList<int> ids = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
           int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId, 
           OrderStatus? status = null, PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null, int? customerId = null)
        {
            var query = GetOrdersQuery(createdAtMin, createdAtMax, status, paymentStatus, shippingStatus, ids, customerId);

            if (sinceId > 0)
            {
                query = query.Where(order => order.Id > sinceId);
            }

            return new ApiList<Order>(query, page - 1, limit);
        }

        public Order GetOrderById(int orderId)
        {
            if (orderId <= 0)
                return null;

            return _orderRepository.Table.FirstOrDefault(order => order.Id == orderId && !order.Deleted);
        }

        public int GetOrdersCount(DateTime? createdAtMin = null, DateTime? createdAtMax = null, OrderStatus? status = null,
                                 PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null,
                                 int? customerId = null)
        {
            var query = GetOrdersQuery(createdAtMin, createdAtMax, status, paymentStatus, shippingStatus, customerId: customerId);

            return query.Count();
        }

        private IQueryable<Order> GetOrdersQuery(DateTime? createdAtMin = null, DateTime? createdAtMax = null, OrderStatus? status = null,
            PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null, IList<int> ids = null, 
            int? customerId = null)
        {
            var query = _orderRepository.TableNoTracking;

            if (customerId != null)
            {
                query = query.Where(order => order.CustomerId == customerId);
            }

            if (ids != null && ids.Count > 0)
            {
                query = query.Where(c => ids.Contains(c.Id));
            }
            
            if (status != null)
            {
                query = query.Where(order => order.OrderStatusId == (int)status);
            }
            
            if (paymentStatus != null)
            {
                query = query.Where(order => order.PaymentStatusId == (int)paymentStatus);
            }
            
            if (shippingStatus != null)
            {
                query = query.Where(order => order.ShippingStatusId == (int)shippingStatus);
            }

            query = query.Where(order => !order.Deleted);

            if (createdAtMin != null)
            {
                query = query.Where(order => order.CreatedOnUtc > createdAtMin.Value.ToUniversalTime());
            }

            if (createdAtMax != null)
            {
                query = query.Where(order => order.CreatedOnUtc < createdAtMax.Value.ToUniversalTime());
            }

            query = query.OrderBy(order => order.Id);

            return query;
        }

        public IList<ShippingMethod> GetAllShippingMethod()
        {
            var shippingMethodList = _shippingMethodRepository.Table.ToList();

            if (shippingMethodList.Count() <= 0)
                return null;

            return shippingMethodList;
        }
    }
}