using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.NivoSlider.Infrastructure.Cache;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Vendors;

namespace Nop.Plugin.Widgets.NivoSlider.Helpers
{
    /// <summary>
    /// Select list helper
    /// </summary>
    public static class SelectListHelper
    {

        /// <summary>
        /// Get event list
        /// </summary>
        /// <param name="eventService">Event service</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Event list</returns>
        public static List<SelectListItem> GetEventList(IEventService eventService, ICacheManager cacheManager, bool showHidden = false)
        {
            if (eventService == null)
                throw new ArgumentNullException("eventService");

            if (cacheManager == null)
                throw new ArgumentNullException("cacheManager");

            string cacheKey = string.Format("Nop.pres.admin.events.list-{0}", showHidden);
            var listItems = cacheManager.Get(cacheKey, () =>
            {
                var events = eventService.GetAllEvents(showHidden: showHidden);
                return events.Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                });
            });

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }


    }
}