using Nop.Core.Domain.Events;

namespace Nop.Data.Mapping.Events
{
    public partial class ProductEventMap : NopEntityTypeConfiguration<ProductEvent>
    {
        public ProductEventMap()
        {
            this.ToTable("Product_Event_Mapping");
            this.HasKey(pc => pc.Id);
            
            this.HasRequired(pc => pc.Event)
                .WithMany()
                .HasForeignKey(pc => pc.EventId);


            this.HasRequired(pc => pc.Product)
                .WithMany(p => p.ProductEvents)
                .HasForeignKey(pc => pc.ProductId);
        }
    }
}