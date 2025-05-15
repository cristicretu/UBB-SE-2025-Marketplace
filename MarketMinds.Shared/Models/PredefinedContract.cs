namespace SharedClassLibrary.Domain
{
    public class PredefinedContract : IPredefinedContract
    {
        public int ContractID { get; set; }
        public required string ContractContent { get; set; }
        public int ID { get; set; } // this will be ignored in the table creation, no need for contractId, not deleted because it might be used in code by others(delete when merging if not needed) - Alex
    }

    /// <summary>
    /// Enum for the predefined contract types.
    /// </summary>
    public enum PredefinedContractType
    {
        BuyingContract = 1,
        SellingContract = 2,
        BorrowingContract = 3
    }
}
