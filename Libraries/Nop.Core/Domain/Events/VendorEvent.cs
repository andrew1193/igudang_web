using Nop.Core.Domain.Vendors;

namespace Nop.Core.Domain.Events
{
    public partial class VendorEvent : BaseEntity
    {
        public int VendorId { get; set; }

        public int EventId { get; set; }

        public virtual Vendor Vendor { get; set; }

        public virtual Event Event { get; set; }
    }
}
