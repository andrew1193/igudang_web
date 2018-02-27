using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Events
{
    public class EventModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime AvailableStartDateTimeUtc { get; set; }

        public DateTime AvailableEndDateTimeUtc { get; set; }

        public int BackgroundPictureId { get; set; }

        public string BackgroundPictureUrl { get; set; }

        public bool Published { get; set; }
    }
}