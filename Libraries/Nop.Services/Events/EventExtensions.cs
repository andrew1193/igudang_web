using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Events;

namespace Nop.Services.Events
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class EventExtensions
    {
       
        public static ProductEvent FindProductEvent(this IList<ProductEvent> source,
            int productId, int eventId)
        {
            foreach (var productEvent in source)
                if (productEvent.ProductId == productId && productEvent.EventId == eventId)
                    return productEvent;

            return null;
        }
        
    }
}
