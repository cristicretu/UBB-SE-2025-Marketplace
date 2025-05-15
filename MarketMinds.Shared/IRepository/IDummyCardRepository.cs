using System.Threading.Tasks;

namespace SharedClassLibrary.IRepository
{
    public interface IDummyCardRepository
    {
        Task DeleteCardAsync(string cardNumber);
        Task<double> GetCardBalanceAsync(string cardNumber);
        Task UpdateCardBalanceAsync(string cardNumber, double balance);
    }
}