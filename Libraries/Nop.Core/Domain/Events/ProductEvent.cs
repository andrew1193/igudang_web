using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Events
{
    /// <summary>
    /// Represents a product event mapping
    /// </summary>
    public partial class ProductEvent : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the event identifier
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is featured
        /// </summary>
        public bool IsFeaturedProduct { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets the event
        /// </summary>
        public virtual Event Event { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }

    }

}
