using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Events;
using System;

namespace Nop.Services.Events
{
    /// <summary>
    /// Event service interface
    /// </summary>
    public partial interface IEventService
    {
        void DeleteEvent(Event ev);

        IPagedList<Event> GetAllEvents(string eventName = "", int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        Event GetEventById(int eventId);

        void InsertEvent(Event ev);

        void UpdateEvent(Event ev);

        void DeleteProductEvent(ProductEvent productEvent);

        IPagedList<ProductEvent> GetProductEventsByEventId(int eventId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        IList<ProductEvent> GetProductEventsByProductId(int productId, bool showHidden = false);

        IList<ProductEvent> GetProductEventsByProductId(int productId, int storeId, bool showHidden = false);

        ProductEvent GetProductEventById(int productEventId);

        void InsertProductEvent(ProductEvent productEvent);

        void UpdateProductEvent(ProductEvent productEvent);

        string[] GetNotExistingEvents(string[] eventNames);

        IDictionary<int, int[]> GetProductEventIds(int[] productIds);

        Event GetCurrentEvent();

        bool CheckIsOtherEventActive();

        Event GetCurrentActiveEvent();
    }
}
