using SharedClassLibrary.Domain;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Interface for managing buyer address-related operations.
    /// </summary>
    public interface IBuyerAddressService
    {
        /// <summary>
        /// Gets all addresses.
        /// </summary>
        /// <returns>A task containing a list of all addresses.</returns>
        Task<List<Address>> GetAllAddressesAsync();

        /// <summary>
        /// Gets an address by its ID.
        /// </summary>
        /// <param name="id">The ID of the address.</param>
        /// <returns>A task containing the address.</returns>
        Task<Address> GetAddressByIdAsync(int id);

        /// <summary>
        /// Adds a new address.
        /// </summary>
        /// <param name="address">The address to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAddressAsync(Address address);

        /// <summary>
        /// Updates an existing address.
        /// </summary>
        /// <param name="address">The address to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAddressAsync(Address address);

        /// <summary>
        /// Deletes an address by its ID.
        /// </summary>
        /// <param name="id">The ID of the address to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAddressAsync(int id);

        /// <summary>
        /// Checks if an address exists by its ID.
        /// </summary>
        /// <param name="id">The ID of the address.</param>
        /// <returns>A task containing a boolean indicating whether the address exists.</returns>
        Task<bool> AddressExistsAsync(int id);
    }
}
