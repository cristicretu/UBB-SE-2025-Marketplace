using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Service;

public class BuyerAddressService : IBuyerAddressService
{
    private readonly IBuyerRepository _buyerRepository;

    public BuyerAddressService(IBuyerRepository buyerRepository)
    {
        _buyerRepository = buyerRepository;
    }

    public async Task<List<Address>> GetAllAddressesAsync()
    {
        return await _buyerRepository.GetAllAddressesAsync();
    }

    public async Task<Address> GetAddressByIdAsync(int id)
    {
        return await _buyerRepository.GetAddressByIdAsync(id);
    }

    public async Task AddAddressAsync(Address address)
    {
        await _buyerRepository.AddAddressAsync(address);
    }

    public async Task UpdateAddressAsync(Address address)
    {
        await _buyerRepository.UpdateAddressAsync(address);
    }

    public async Task DeleteAddressAsync(int id)
    {
        var address = await _buyerRepository.GetAddressByIdAsync(id);
        if (address != null)
        {
            await _buyerRepository.DeleteAddressAsync(address);
        }
    }

    public async Task<bool> AddressExistsAsync(int id)
    {
        return await _buyerRepository.AddressExistsAsync(id);
    }
}
