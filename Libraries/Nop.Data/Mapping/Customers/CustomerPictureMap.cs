using Nop.Core.Domain.Customers;

namespace Nop.Data.Mapping.Customers
{
    public partial class CustomerPictureMap : NopEntityTypeConfiguration<CustomerPicture>
    {
        public CustomerPictureMap()
        {
            this.ToTable("Customer_Picture_Mapping");
            this.HasKey(cp => cp.Id);
            
            this.HasRequired(cp => cp.Picture)
                .WithMany()
                .HasForeignKey(cp => cp.PictureId);


            this.HasRequired(cp => cp.Customer)
                .WithMany(cp => cp.CustomerPictures)
                .HasForeignKey(cp => cp.CustomerId);
        }
    }
}