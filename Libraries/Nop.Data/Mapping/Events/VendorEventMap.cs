using Nop.Core.Domain.Events;

namespace Nop.Data.Mapping.Events
{
    public partial class VendorEventMap : NopEntityTypeConfiguration<VendorEvent>
    {
        public VendorEventMap()
        {
            this.ToTable("Vendor_Event_Mapping");
            this.HasKey(ve => ve.Id);

            this.HasRequired(ve => ve.Vendor)
                .WithMany(ve => ve.VendorEvents)
                .HasForeignKey(ve => ve.VendorId);

            this.HasRequired(ve => ve.Event)
                .WithMany()
                .HasForeignKey(ve => ve.EventId);

        }
    }
}
