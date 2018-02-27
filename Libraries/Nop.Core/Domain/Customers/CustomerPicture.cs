
using Nop.Core.Domain.Media;
using System;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer picture mapping
    /// </summary>
    public partial class CustomerPicture : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public int PictureId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }


        /// <summary>
        /// Gets or sets the upload date and time
        /// </summary>
        public DateTime UploadDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the publish date and time
        /// </summary>
        public DateTime? PublishDateTimeUtc { get; set; }

        /// <summary>
        /// Gets the picture
        /// </summary>
        public virtual Picture Picture { get; set; }

        /// <summary>
        /// Gets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }
    }

}
