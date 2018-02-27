using Nop.Core.Domain.Events;

namespace Nop.Data.Mapping.Events
{
    public partial class EventMap : NopEntityTypeConfiguration<Event>
    {
        public EventMap()
        {
            this.ToTable("Event");
            this.HasKey(c => c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(400);
        }
    }
}