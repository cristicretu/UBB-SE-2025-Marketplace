using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClassLibrary.Domain
{
    /// <summary>
    /// Represents order details along with product information.
    /// </summary>
    public class OrderDisplayInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier of the order.
        /// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// Gets or sets the product type name.
        /// </summary>
        public string? ProductTypeName { get; set; }

        /// <summary>
        /// Gets or sets the order date as a formatted string.
        /// </summary>
        public string? OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the payment method used for the order.
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the order summary.
        /// </summary>
        public int OrderSummaryID { get; set; }

        /// <summary>
        /// Gets or sets the product category (either "new" or "borrowed").
        /// </summary>
        public string? ProductCategory { get; set; }
    }
}
