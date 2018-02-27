using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Events
{
    public partial class EventListModel : BaseNopModel
    {
        public EventListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Events.Events.List.SearchEventName")]
        [AllowHtml]
        public string SearchEventName { get; set; }

        [NopResourceDisplayName("Admin.Events.Events.List.SearchStore")]
        public int SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}