using System;
using Nop.Core.Domain.Customers;
using Nop.Services.Tasks;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Customers;
using Nop.Services.Messages;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Represents a task for deleting guest customers
    /// </summary>
    public partial class MarkOrderCancelTask : ITask
    {
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IOrderService _orderService;
        private readonly IWorkflowMessageService _workflowMessageService;
        public MarkOrderCancelTask(ICustomerService customerService, CustomerSettings customerSettings,
            IOrderService orderService,
            IWorkflowMessageService workflowMessageService
            )
        {
            this._customerService = customerService;
            this._orderService = orderService;
            this._customerSettings = customerSettings;
            this._workflowMessageService = workflowMessageService;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var orderRecord = _orderService.GetOrderByStatusAndPaymentStatus((int)OrderStatus.Pending, (int)PaymentStatus.Pending);
            foreach(var record in orderRecord)
            {
                var order = _orderService.GetOrderById(record.Id);
                  order.OrderStatus = OrderStatus.Cancelled;
                _orderService.UpdateOrder(order);
                _workflowMessageService.SendOrderCancelledCustomerNotification(record, record.CustomerLanguageId);

                //by rite should be send email inform customer been cancelled


            }
        }
    }
}
