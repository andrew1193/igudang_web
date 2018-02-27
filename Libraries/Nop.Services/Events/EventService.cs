using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Events;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Helpers;

namespace Nop.Services.Events
{
    /// <summary>
    /// Event service
    /// </summary>
    public partial class EventService : IEventService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : Event ID
        /// </remarks>
        private const string EVENTS_BY_ID_KEY = "Nop.event.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent Event ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// {4} : include all levels (child)
        /// </remarks>
        private const string EVENTS_BY_PARENT_EVENT_ID_KEY = "Nop.event.byparent-{0}-{1}-{2}-{3}-{4}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : Event ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        private const string PRODUCTEVENTS_ALLBYEVENTID_KEY = "Nop.productevent.allbyeventid-{0}-{1}-{2}-{3}-{4}-{5}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : product ID
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string PRODUCTEVENTS_ALLBYPRODUCTID_KEY = "Nop.productevent.allbyproductid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string EVENTS_PATTERN_KEY = "Nop.event.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTEVENTS_PATTERN_KEY = "Nop.productevent.";

        #endregion

        #region Fields

        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<ProductEvent> _productEventRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IDbContext _dbContext;
        private readonly IDataProvider _dataProvider;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly CommonSettings _commonSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IDateTimeHelper _datetimeHelper;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="eventRepository">Event repository</param>
        /// <param name="productEventRepository">ProductEvent repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="dbContext">DB context</param>
        /// <param name="dataProvider">Data provider</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public EventService(ICacheManager cacheManager,
            IRepository<Event> eventRepository,
            IRepository<ProductEvent> productEventRepository,
            IRepository<Product> productRepository,
            IRepository<AclRecord> aclRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IDbContext dbContext,
            IDataProvider dataProvider,
            IWorkContext workContext,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            CommonSettings commonSettings,
            CatalogSettings catalogSettings,
            IDateTimeHelper dateTimeHelper)
        {
            this._cacheManager = cacheManager;
            this._eventRepository = eventRepository;
            this._productEventRepository = productEventRepository;
            this._productRepository = productRepository;
            this._aclRepository = aclRepository;
            this._storeMappingRepository = storeMappingRepository;
            this._dbContext = dbContext;
            this._dataProvider = dataProvider;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;
            this._storeMappingService = storeMappingService;
            this._aclService = aclService;
            this._commonSettings = commonSettings;
            this._catalogSettings = catalogSettings;
            this._datetimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete Event
        /// </summary>
        /// <param name="ev">Event</param>
        public virtual void DeleteEvent(Event ev)
        {
            if (ev == null)
                throw new ArgumentNullException("event");

            ev.Deleted = true;
            UpdateEvent(ev);

            //event notification
            _eventPublisher.EntityDeleted(ev);

        }

        /// <summary>
        /// Gets all Events
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Events</returns>
        public virtual IPagedList<Event> GetAllEvents(string eventName = "", int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _eventRepository.Table;
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(eventName))
                query = query.Where(c => c.Name.Contains(eventName));
            query = query.Where(c => !c.Deleted);
            query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);


            var events = query.ToList();


            //paging
            return new PagedList<Event>(events, pageIndex, pageSize);
        }


        /// <summary>
        /// Gets a event
        /// </summary>
        /// <param name="eventId">Event identifier</param>
        /// <returns>Event</returns>
        public virtual Event GetEventById(int eventId)
        {
            if (eventId == 0)
                return null;

            string key = string.Format(EVENTS_BY_ID_KEY, eventId);
            return _cacheManager.Get(key, () => _eventRepository.GetById(eventId));
        }

        /// <summary>
        /// Inserts event
        /// </summary>
        /// <param name="ev">Event</param>
        public virtual void InsertEvent(Event ev)
        {
            if (ev == null)
                throw new ArgumentNullException("event");

            _eventRepository.Insert(ev);

            //cache
            _cacheManager.RemoveByPattern(EVENTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTEVENTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(ev);
        }

        /// <summary>
        /// Updates the event
        /// </summary>
        /// <param name="ev">Event</param>
        public virtual void UpdateEvent(Event ev)
        {
            if (ev == null)
                throw new ArgumentNullException("event");


            _eventRepository.Update(ev);

            //cache
            _cacheManager.RemoveByPattern(EVENTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTEVENTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(ev);
        }


        /// <summary>
        /// Deletes a product event mapping
        /// </summary>
        /// <param name="productEvent">Product event</param>
        public virtual void DeleteProductEvent(ProductEvent productEvent)
        {
            if (productEvent == null)
                throw new ArgumentNullException("productEvent");

            _productEventRepository.Delete(productEvent);

            //cache
            _cacheManager.RemoveByPattern(EVENTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTEVENTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productEvent);
        }

        /// <summary>
        /// Gets product event mapping collection
        /// </summary>
        /// <param name="eventId">Event identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a event mapping collection</returns>
        public virtual IPagedList<ProductEvent> GetProductEventsByEventId(int eventId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (eventId == 0)
                return new PagedList<ProductEvent>(new List<ProductEvent>(), pageIndex, pageSize);

            string key = string.Format(PRODUCTEVENTS_ALLBYEVENTID_KEY, showHidden, eventId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _productEventRepository.Table
                            join p in _productRepository.Table on pc.ProductId equals p.Id
                            where pc.EventId == eventId &&
                                  !p.Deleted &&
                                  (showHidden || p.Published)
                            orderby pc.DisplayOrder, pc.Id
                            select pc;


                var productEvents = new PagedList<ProductEvent>(query, pageIndex, pageSize);
                return productEvents;
            });
        }

        /// <summary>
        /// Gets a product event mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product event mapping collection</returns>
        public virtual IList<ProductEvent> GetProductEventsByProductId(int productId, bool showHidden = false)
        {
            return GetProductEventsByProductId(productId, _storeContext.CurrentStore.Id, showHidden);
        }
        /// <summary>
        /// Gets a product event mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> Product event mapping collection</returns>
        public virtual IList<ProductEvent> GetProductEventsByProductId(int productId, int storeId, bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductEvent>();

            string key = string.Format(PRODUCTEVENTS_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id, storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pe in _productEventRepository.Table
                            join c in _eventRepository.Table on pe.EventId equals c.Id
                            where pe.ProductId == productId &&
                                  !c.Deleted &&
                                  (showHidden || c.Published)
                            orderby pe.DisplayOrder, pe.Id
                            select pe;

                var allProductEvents = query.ToList();
                var result = new List<ProductEvent>();
                //no filtering
                result.AddRange(allProductEvents);
                return result;
            });
        }

        /// <summary>
        /// Gets a product event mapping 
        /// </summary>
        /// <param name="productEventId">Product event mapping identifier</param>
        /// <returns>Product event mapping</returns>
        public virtual ProductEvent GetProductEventById(int productEventId)
        {
            if (productEventId == 0)
                return null;

            return _productEventRepository.GetById(productEventId);
        }

        /// <summary>
        /// Inserts a product event mapping
        /// </summary>
        /// <param name="productEvent">>Product event mapping</param>
        public virtual void InsertProductEvent(ProductEvent productEvent)
        {
            if (productEvent == null)
                throw new ArgumentNullException("productEvent");

            _productEventRepository.Insert(productEvent);

            //cache
            _cacheManager.RemoveByPattern(EVENTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTEVENTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productEvent);
        }

        /// <summary>
        /// Updates the product event mapping 
        /// </summary>
        /// <param name="productEvent">>Product event mapping</param>
        public virtual void UpdateProductEvent(ProductEvent productEvent)
        {
            if (productEvent == null)
                throw new ArgumentNullException("productEvent");

            _productEventRepository.Update(productEvent);

            //cache
            _cacheManager.RemoveByPattern(EVENTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTEVENTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productEvent);
        }


        /// <summary>
        /// Returns a list of names of not existing events
        /// </summary>
        /// <param name="eventNames">The nemes of the events to check</param>
        /// <returns>List of names not existing events</returns>
        public virtual string[] GetNotExistingEvents(string[] eventNames)
        {
            if (eventNames == null)
                throw new ArgumentNullException("eventNames");

            var query = _eventRepository.Table;
            var queryFilter = eventNames.Distinct().ToArray();
            var filter = query.Select(c => c.Name).Where(c => queryFilter.Contains(c)).ToList();

            return queryFilter.Except(filter).ToArray();
        }


        /// <summary>
        /// Get event IDs for products
        /// </summary>
        /// <param name="productIds">Products IDs</param>
        /// <returns>Event IDs for products</returns>
        public virtual IDictionary<int, int[]> GetProductEventIds(int[] productIds)
        {
            var query = _productEventRepository.Table;

            return query.Where(p => productIds.Contains(p.ProductId))
                .Select(p => new { p.ProductId, p.EventId }).ToList()
                .GroupBy(a => a.ProductId)
                .ToDictionary(items => items.Key, items => items.Select(a => a.EventId).ToArray());
        }

        public virtual Event GetCurrentEvent()
        {
            var query = _eventRepository.Table.ToList();
            query = query.Where(x => x.Published == true && x.Deleted == false).ToList();
            //DateTime now = _datetimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc);
            DateTime now = _datetimeHelper.ConvertToUtcTime(DateTime.Now, DateTimeKind.Local);
            query = query.Where(x => now <= x.EndedOnUtc && now >= x.StartedOnUtc).OrderBy(x => x.DisplayOrder).ToList();
            var currentEvent = new Event();
            if (query.Count() > 0)
            {
                currentEvent = query.FirstOrDefault();
                currentEvent.StartedOnUtc = _datetimeHelper.ConvertToUserTime(currentEvent.StartedOnUtc, DateTimeKind.Utc);
                currentEvent.EndedOnUtc = _datetimeHelper.ConvertToUserTime(currentEvent.EndedOnUtc, DateTimeKind.Utc);
            }
            return currentEvent;
        }

        public bool CheckIsOtherEventActive()
        {
            var query = _eventRepository.Table;

            var result = query.Any(x => x.Published);

            return result;
        }

        public Event GetCurrentActiveEvent()
        {
            var query = _eventRepository.Table.Where(x => x.Deleted == false && x.Published == true).OrderBy(x => x.DisplayOrder);

            var ev = query.FirstOrDefault();

            return ev;
        }
        #endregion
    }
}
