using System;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the borrow product view
    /// </summary>
    public class BorrowProductViewModel
    {
        /// <summary>
        /// Gets or sets the product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product price
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Gets or sets the seller ID
        /// </summary>
        public int SellerId { get; set; }

        /// <summary>
        /// Gets or sets the seller name
        /// </summary>
        public string SellerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product type
        /// </summary>
        public string ProductType { get; set; } = string.Empty;

        private DateTime? _startDate;
        /// <summary>
        /// Gets or sets the start date (availability date)
        /// </summary>
        public DateTimeOffset? StartDate 
        { 
            get => _startDate.HasValue ? new DateTimeOffset(_startDate.Value, TimeSpan.Zero) : null; 
            set
            {
                if (value.HasValue)
                {
                    try
                    {
                        // Use UTC to avoid issues with offsets
                        _startDate = value.Value.UtcDateTime;
                }
                    catch
                    {
                        // Fallback to a safe default if conversion fails
                        _startDate = DateTime.UtcNow;
                    }
                }
                else
                {
                    _startDate = null;
                }
            }
        }

        private DateTime? _endDate;
        /// <summary>
        /// Gets or sets the end date (unavailable until)
        /// </summary>
        public DateTimeOffset? EndDate 
        { 
            get => _endDate.HasValue ? new DateTimeOffset(_endDate.Value, TimeSpan.Zero) : null; 
            set
            {
                if (value.HasValue)
                {
                    try
                    {
                        // Use UTC to avoid issues with offsets
                        _endDate = value.Value.UtcDateTime;
                    }
                    catch
                {
                        // Fallback to a safe default if conversion fails
                        _endDate = DateTime.UtcNow.AddDays(30); // Default to 30 days from now
                    }
                }
                else
                {
                    _endDate = null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the product is available
        /// </summary>
        public bool IsAvailable => !EndDate.HasValue || (_endDate.HasValue && _endDate.Value == DateTime.MinValue);

        /// <summary>
        /// Gets or sets a value indicating whether the current user is on the waitlist
        /// </summary>
        public bool IsOnWaitlist { get; set; }

        /// <summary>
        /// Gets or sets the user's position in the waitlist
        /// </summary>
        public int WaitlistPosition { get; set; }

        /// <summary>
        /// Gets or sets the unread notifications count
        /// </summary>
        public int UnreadNotificationsCount { get; set; }

        /// <summary>
        /// Gets the formatted availability message based on dates
        /// </summary>
        public string AvailabilityMessage
        {
            get
            {
                if (IsAvailable)
                {
                    if (!_startDate.HasValue || _startDate.Value == DateTime.MinValue)
                    {
                        return "Availability: Now";
                    }
                    else
                    {
                        return $"Available after: {_startDate.Value:yyyy-MM-dd}";
                    }
                    }
                else
                {
                    if (_endDate.HasValue)
                    {
                        return $"Unavailable until: {_endDate.Value:yyyy-MM-dd}";
                }
                else
                {
                        return "Availability status unknown";
                    }
                }
            }
        }
    }
} 